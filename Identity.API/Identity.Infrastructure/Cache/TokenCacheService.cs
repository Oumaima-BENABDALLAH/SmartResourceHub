using Identity.Application.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Infrastructure.Cache
{
    public class TokenCacheService : ITokenCacheService
    {

        // IDatabase = interface Redis pour toutes les opérations :  SET, GET, DEL, EXISTS, EXPIRE
        private readonly StackExchange.Redis.IDatabase _redis;

        //toujours préfixer les clés Redis pour éviter les collisions entre différentes applications qui partagent la même instance Redis

        private const string RefreshTokenPrefix = "refresh_token:";
        private const string BlacklistPrefix = "blacklist:";

        public TokenCacheService(IConnectionMultiplexer connectionMultiplexer)
        {
            // GetDatabase() retourne la DB par défaut (index 0)
            // Redis supporte 16 bases de données (0 à 15)
           
            _redis = connectionMultiplexer.GetDatabase();
        }

        public async Task BlacklistAccessTokenAsync(string jti, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            // jti (JWT ID) identifiant unique embarqué dans chaque token
            var key = $"{BlacklistPrefix}{jti}";
            await _redis.StringSetAsync(key, "true", expiration, false, When.Always, CommandFlags.None);
            // On stocke "true" avec le TTL restant du token quand le token expire naturellement Redis nettoie automatiquement
            
        }

        public async  Task<string?> GetRefreshTokenAsync(string userId, CancellationToken cancellationToken = default)
        {

            var key = $"{RefreshTokenPrefix}{userId}";
            // StringGetAsync = commande Redis GET retourne RedisValue.Null si clé inexistante ou expirée
            var value = await _redis.StringGetAsync(key);
            return value.IsNullOrEmpty ? null : value.ToString();
        }

        public async  Task<bool> IsAccessTokenBlacklistedAsync(string jti, CancellationToken cancellationToken = default)
        {
            var key = $"{BlacklistPrefix}{jti}";
            return await _redis.KeyExistsAsync(key);
        }

        public async  Task RemoveRefreshTokenAsync(string userId, CancellationToken cancellationToken = default)
        {
            var key = $"{RefreshTokenPrefix}{userId}";
            await _redis.KeyDeleteAsync(key);
        }

        public async  Task SetRefreshTokenAsync(string userId, string refreshToken, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            var key = $"{RefreshTokenPrefix}{userId}";
            await _redis.StringSetAsync(key, refreshToken, expiration, false, When.Always, CommandFlags.None);
        }
    }
}
