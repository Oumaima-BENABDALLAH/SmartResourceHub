using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Application.Interfaces
{
    // Interface qui définit le contrat de cache Redis
    
    public interface ITokenCacheService
    {
        // Stocke le Refresh Token d'un user dans Redis avec TTL automatique
        // TTL (Time To Live) Redis supprime automatiquement après expiration
        Task SetRefreshTokenAsync(
        string userId,
        string refreshToken,
        TimeSpan expiration,
        CancellationToken cancellationToken = default);


        // Récupère le Refresh Token stocké  null si expiré ou inexistant
        Task<string?> GetRefreshTokenAsync(string userId, CancellationToken cancellationToken = default);

        // Supprime le Refresh Token  utilisé lors du Logout
        Task RemoveRefreshTokenAsync( string userId,CancellationToken cancellationToken = default);

        // Ajoute l'Access Token dans une blacklist Redis même s'il n'est pas expiré, il sera rejeté par le Gateway
        Task BlacklistAccessTokenAsync( string jti, TimeSpan expiration,CancellationToken cancellationToken = default);


        // Vérifie si un Access Token est blacklisté
        Task<bool> IsAccessTokenBlacklistedAsync( string jti, CancellationToken cancellationToken = default);
    }
}
