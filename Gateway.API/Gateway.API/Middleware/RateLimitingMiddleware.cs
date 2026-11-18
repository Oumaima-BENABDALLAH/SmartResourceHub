using StackExchange.Redis;

namespace Gateway.API.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDatabase _redis;
        private readonly ILogger<RateLimitingMiddleware> _logger;

        // Configuration : 100 requêtes max par minute par IP
        private const int MaxRequests = 100;
        private const int WindowSeconds = 60;

        public RateLimitingMiddleware(
            RequestDelegate next,
            IConnectionMultiplexer connectionMultiplexer,
            ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _redis = connectionMultiplexer.GetDatabase();
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Identifier le client par son IP
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Clé Redis : "rate_limit:192.168.1.1"
            var key = $"rate_limit:{clientIp}";

            // Incrémenter le compteur de requêtes
            // Redis INCR est atomique  pas de race condition possible
            var requestCount = await _redis.StringIncrementAsync(key);

            if (requestCount == 1)
            {
                // Première requête de cette fenêtre → définir le TTL Après 60 secondes, Redis supprime la clé automatiquement et le compteur repart à zéro
                await _redis.KeyExpireAsync(key, TimeSpan.FromSeconds(WindowSeconds));
            }

            // Ajouter les headers informatifs dans la réponse
            // Permettent au client de savoir combien de requêtes il lui reste
            context.Response.Headers["X-RateLimit-Limit"] = MaxRequests.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] =
                Math.Max(0, MaxRequests - (int)requestCount).ToString();

            if (requestCount > MaxRequests)
            {
                _logger.LogWarning(
                    "Rate limit dépassé pour IP : {ClientIp} ({Count} req)",
                    clientIp, requestCount);

                context.Response.StatusCode = 429; // Too Many Requests
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Too many requests. Please try again in a minute.",
                    retryAfterSeconds = WindowSeconds
                });
                return;
            }

            await _next(context);
        }
    }
}
