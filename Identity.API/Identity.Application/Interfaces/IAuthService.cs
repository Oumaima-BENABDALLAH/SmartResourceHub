using Identity.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Application.Interfaces
{
    public interface IAuthService
    {
        // Crée un nouveau compte utilisateur
        Task<UserDto> RegisterAsync(
            string firstName,
            string lastName,
            string email,
            string password,
            CancellationToken cancellationToken = default);


        // Vérifie les credentials et génère un JWT
        Task<AuthResponse> LoginAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default);

        // Récupère un utilisateur par son Id
        Task<UserDto?> GetUserByIdAsync(
            string userId,
            CancellationToken cancellationToken = default);


        //  Logout avec blacklist du token
        Task LogoutAsync(
            string userId,
            string jti,
            TimeSpan accessTokenRemainingTime,
            CancellationToken cancellationToken = default);

        //  Renouvellement du token via Refresh Token
        Task<AuthResponse> RefreshTokenAsync(
            string userId,
            string refreshToken,
            CancellationToken cancellationToken = default);

    }
}
