
 using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Identity.Application.Services
{
    public class AuthService : IAuthService
    {
        // UserManager : gère la création, recherche, validation des users

        private readonly UserManager<ApplicationUser> _userManager;

        // RoleManager : gère les rôles (Admin, User..)
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly IConfiguration _configuration;
        private readonly ITokenCacheService _tokenCache; //Redis

        public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        ITokenCacheService tokenCache)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _tokenCache = tokenCache;
        }
        public async  Task<UserDto?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDto( user.Id,
                                user.Email!,
                                user.FirstName,
                                user.LastName,
                                user.FullName,
                                roles,
                                user.CreatedAt);
        }

        public async  Task<AuthResponse> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            // 1. Chercher l'utilisateur par email
            var user = await _userManager.FindByEmailAsync(email)
                ?? throw new InvalidOperationException("Invalid email or password.");

            // 2. Vérifier le mot de passe (CheckPasswordAsync compare avec le hash)
            var isValidPassword = await _userManager.CheckPasswordAsync(user, password);
            if (!isValidPassword)
                throw new InvalidOperationException("Invalid email or password.");

            // 3. Récupérer les rôles de l'utilisateur
            var roles = await _userManager.GetRolesAsync(user);

            // 4. Générer le JWT
            var (accessToken, expiresAt , jti) = GenerateJwtToken(user, roles);

            // 5. Générer le Refresh Token
            var refreshToken = GenerateRefreshToken();

            // 6.  Stocke le Refresh Token dans Redis (TTL 7 jours)
            await _tokenCache.SetRefreshTokenAsync(
                user.Id, refreshToken, TimeSpan.FromDays(7), cancellationToken); ;


            return new AuthResponse(
                accessToken,
                refreshToken,
                expiresAt,
                user.Id,
                user.Email!,
                user.FullName,
                roles);

        }
        private (string token, DateTime expiresAt , string jti) GenerateJwtToken( ApplicationUser user, IList<string> roles)
        {
            var jwtSection = _configuration.GetSection("Jwt");
            var secret = jwtSection["Secret"]!;
            var issuer = jwtSection["Issuer"]!;
            var audience = jwtSection["Audience"]!;
            var expirationMinutes = int.Parse(jwtSection["ExpirationMinutes"] ?? "60");


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            
            // jti = identifiant unique du token
            // Permet de blacklister précisément UN token sans affecter les autres

            var jti = Guid.NewGuid().ToString();

            // Les Claims sont les données embarquées dans le token
            // Chaque microservice peut lire ces claims sans appeler Identity.API
            var claims = new List<Claim>
             {
                  new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                  new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                  new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                  new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                  new Claim(JwtRegisteredClaimNames.Jti, jti),
           };

           // Ajouter chaque rôle comme un Claim séparé
           foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

            // Construire le token
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt, jti);


        }
        private static string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
        public async  Task<UserDto> RegisterAsync(string firstName, string lastName, string email, string password, CancellationToken cancellationToken = default)
        {

            // 1. Vérifier si l'email est déjà utilisé
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser is not null)
                throw new InvalidOperationException($"Email '{email}' is already registered.");

            // 2. Créer l'entité utilisateur
            var user = new ApplicationUser
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                UserName = email, // Identity utilise UserName pour l'auth
                CreatedAt = DateTime.UtcNow
            };

            // 3. CreateAsync hash le mot de passe automatiquement
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Registration failed: {errors}");
            }

            // 4. Créer le rôle "User" s'il n'existe pas encore
            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new IdentityRole("User"));

            // 5. Assigner le rôle User par défaut
            await _userManager.AddToRoleAsync(user, "User");

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDto(
                user.Id,
                user.Email!,
                user.FirstName,
                user.LastName,
                user.FullName,
                roles,
                user.CreatedAt);

        }

        public async Task LogoutAsync(string userId, string jti, TimeSpan accessTokenRemainingTime, CancellationToken cancellationToken = default)
        {
            // 1. Supprimer le Refresh Token  plus de renouvellement possible
            await _tokenCache.RemoveRefreshTokenAsync(userId, cancellationToken);

            // 2. Blacklister l'Access Token courant même s'il n'est pas encore expiré il est  rejeté par le Gateway

            if (accessTokenRemainingTime > TimeSpan.Zero)
                await _tokenCache.BlacklistAccessTokenAsync(
                    jti, accessTokenRemainingTime, cancellationToken);

        }

        public async Task<AuthResponse> RefreshTokenAsync(string userId, string refreshToken, CancellationToken cancellationToken = default)
        {
            // 1. Vérifier le Refresh Token dans Redis

            var storedToken = await _tokenCache.GetRefreshTokenAsync(userId, cancellationToken);

            if (storedToken is null || storedToken != refreshToken)
                throw new InvalidOperationException("Invalid or expired refresh token.");


            // 2. Récupérer l'utilisateur
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new InvalidOperationException("User not found.");

            var roles = await _userManager.GetRolesAsync(user);

            // 3. Nouveau JWT
            var (accessToken, expiresAt, jti) = GenerateJwtToken(user, roles);

            var newRefreshToken = GenerateRefreshToken();
            await _tokenCache.SetRefreshTokenAsync(
                userId, newRefreshToken, TimeSpan.FromDays(7), cancellationToken);

            return new AuthResponse(
                accessToken, newRefreshToken, expiresAt,
                user.Id, user.Email!, user.FullName, roles);
        }
    }
}
