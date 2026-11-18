using Identity.API.Models;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
namespace Identity.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
            => _authService = authService;

        [HttpPost("register")]
        [AllowAnonymous] // Pas besoin d'être connecté pour s'inscrire
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDto>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
        {

            try
            {
                var user = await _authService.RegisterAsync(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Password,
                cancellationToken);

                return StatusCode(201, user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { ex.Message });


            }
        }


        [HttpPost("login")]
        [AllowAnonymous] // Pas besoin d'être connecté pour s'inscrire
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResponse>> login(
        [FromBody] LoginRequest login,
        CancellationToken cancellationToken)
        {
            try
            {
                var reponse = await _authService.LoginAsync(
                login.Email,
                login.Password,
                cancellationToken);

                return Ok(reponse);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { ex.Message });

            }

        }


        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // Force le JWT
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDto>> me(CancellationToken cancellationToken)
        {
            // Le UserId est extrait automatiquement du JWT par ASP.NET

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _authService.GetUserByIdAsync(userId, cancellationToken);

            if (user is null) return Unauthorized();

            return Ok(user);

        }


        // Logout — invalide les deux tokens
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(jti))
                return Unauthorized();

            // Calculer le temps restant du token pour le TTL Redis
            var expClaim = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
            var remainingTime = TimeSpan.FromMinutes(60);

            if (expClaim is not null)
            {
                var expDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim));
                remainingTime = expDate - DateTimeOffset.UtcNow;
                if (remainingTime < TimeSpan.Zero) remainingTime = TimeSpan.Zero;
            }

            await _authService.LogoutAsync(userId, jti, remainingTime, cancellationToken);

            return Ok(new { message = "Logged out successfully." });
        }

        // Refresh renouvelle l'Access Token
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Refresh(
            [FromBody] RefreshTokenRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await _authService.RefreshTokenAsync(
                    request.UserId, request.RefreshToken, cancellationToken);

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}
