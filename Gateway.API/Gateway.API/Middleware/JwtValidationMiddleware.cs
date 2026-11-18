using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Gateway.API.Middleware
{
    public class JwtValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly IDatabase _redis;
        private readonly ILogger<JwtValidationMiddleware> _logger;

        // Routes qui ne nécessitent PAS de JWT on bypass la validation pour ces paths

        private static readonly string[] PublicRoutes =
          [
             "/api/auth/login",
             "/api/auth/register",
             "/api/auth/refresh"
           ];
        public JwtValidationMiddleware(RequestDelegate next,
                                       IConfiguration configuration,
                                       IConnectionMultiplexer connectionMultiplexer,
                                       ILogger<JwtValidationMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _redis = connectionMultiplexer.GetDatabase();
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // 1. Bypass pour les routes publiques (login, register, refresh)
            if (IsPublicRoute(path))
            {
                await _next(context);
                return;
            }

            // 2. Récupérer le token depuis le header Authorization Format attendu : "Bearer eyJhbGc..."
            var authHeader = context.Request.Headers.Authorization.ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                _logger.LogWarning("Requête sans token JWT : {Path}", path);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Authorization token is required."
                });
                return;
            }

            var token = authHeader["Bearer ".Length..].Trim();

            // 3. Valider le token JWT
            var (isValid, principal, jti) = ValidateToken(token);

            if (!isValid || principal is null)
            {
                _logger.LogWarning("Token JWT invalide pour : {Path}", path);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Invalid or expired token."
                });
                return;
            }

            // 4. Vérifier la blacklist Redis Si le token a été blacklisté (après logout)

            if (!string.IsNullOrEmpty(jti))
            {
                var isBlacklisted = await _redis.KeyExistsAsync($"blacklist:{jti}");
                if (isBlacklisted)
                {
                    _logger.LogWarning("Token blacklisté utilisé : jti={Jti}", jti);
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = "Token has been revoked. Please login again."
                    });
                    return;
                }
            }

            // 5. Enrichir les headers — passer UserId et Roles aux microservices chaque service downstream peut lire ces headers sans avoir à redécoder le JWT
            var userId = principal.FindFirst("sub")?.Value
                ?? principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var userEmail = principal.FindFirst("email")?.Value
                ?? principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            var roles = principal.FindAll(System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            if (!string.IsNullOrEmpty(userId))
            {
                // Ces headers sont disponibles dans Catalog.API et Booking.API
                context.Request.Headers["X-UserId"] = userId;
                context.Request.Headers["X-UserEmail"] = userEmail ?? "";
                context.Request.Headers["X-UserRoles"] = string.Join(",", roles);
            }

            _logger.LogInformation(
                "Requête autorisée : UserId={UserId} Path={Path}",
                userId, path);

            // 6. Tout est OK laisser passer vers YARP
            await _next(context);
        }

        private (bool isValid, System.Security.Claims.ClaimsPrincipal? principal, string? jti)
            ValidateToken(string token)
        {
            try
            {
                var jwtSection = _configuration.GetSection("Jwt");
                var secret = jwtSection["Secret"]!;
                var issuer = jwtSection["Issuer"]!;
                var audience = jwtSection["Audience"]!;

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero 
                };

                var handler = new JwtSecurityTokenHandler();
                handler.InboundClaimTypeMap.Clear(); // Désactiver le remapping

                var principal = handler.ValidateToken(
                    token, validationParameters, out var validatedToken);

                // Extraire le jti pour la vérification blacklist
                var jti = (validatedToken as JwtSecurityToken)
                    ?.Claims
                    .FirstOrDefault(c => c.Type == "jti")
                    ?.Value;

                return (true, principal, jti);
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogWarning("Token expiré");
                return (false, null, null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Erreur validation token : {Message}", ex.Message);
                return (false, null, null);
            }
        }

        private static bool IsPublicRoute(string path)
            => PublicRoutes.Any(r => path.StartsWith(r, StringComparison.OrdinalIgnoreCase));
    
}
}
