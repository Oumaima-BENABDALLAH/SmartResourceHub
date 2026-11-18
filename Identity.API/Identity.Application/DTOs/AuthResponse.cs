using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Application.DTOs
{
    // Ce DTO est retourné au client après un Login réussi
    // Il contient le JWT et les infos de base de l'utilisateur
    public sealed record AuthResponse(
        string AccessToken,      // Le JWT à utiliser dans les prochaines requêtes
        string RefreshToken,     // Pour renouveler le token sans re-login
        DateTime ExpiresAt,      // Date d'expiration du AccessToken
        string UserId,
        string Email,
        string FullName,
        IList<string> Roles      // ex: ["Admin", "User"]
    );
}
