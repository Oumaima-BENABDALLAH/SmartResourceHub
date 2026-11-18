using FluentAssertions;
using Identity.Application.Interfaces;
using Identity.Application.Services;
using Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.UnitTests.Services
{
    public  class AuthServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ITokenCacheService> _tokenCacheMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // UserManager nécessite un IUserStore mock minimum
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            // RoleManager nécessite un IRoleStore mock minimum
            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                roleStoreMock.Object, null, null, null, null);

            _configurationMock = new Mock<IConfiguration>();
            _tokenCacheMock = new Mock<ITokenCacheService>();

            // Configuration JWT minimale pour les tests
            SetupJwtConfiguration();

            _authService = new AuthService(
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _configurationMock.Object,
                _tokenCacheMock.Object);
        }
        [Fact]
        public async Task RegisterAsync_WithNewEmail_ShouldCreateUserAndReturnDto()
        {
            // ARRANGE
            _userManagerMock
                .Setup(u => u.FindByEmailAsync("test@test.com"))
                .ReturnsAsync((ApplicationUser?)null); // email pas encore utilisé

            _userManagerMock
                .Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), "Test1234!"))
                .ReturnsAsync(IdentityResult.Success);

            _roleManagerMock
                .Setup(r => r.RoleExistsAsync("User"))
                .ReturnsAsync(false);

            _roleManagerMock
                .Setup(r => r.CreateAsync(It.IsAny<IdentityRole>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(u => u.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string> { "User" });

            // ACT
            var result = await _authService.RegisterAsync(
                "Oumaima", "Ben", "test@test.com", "Test1234!");

            // ASSERT
            result.Should().NotBeNull();
            result.Email.Should().Be("test@test.com");
            result.FirstName.Should().Be("Oumaima");
            result.Roles.Should().Contain("User");
        }

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ShouldThrowInvalidOperationException()
        {
            // ARRANGE — email déjà utilisé
            _userManagerMock
                .Setup(u => u.FindByEmailAsync("existing@test.com"))
                .ReturnsAsync(new ApplicationUser { Email = "existing@test.com" });

            // ACT
            var act = async () => await _authService.RegisterAsync(
                "Test", "User", "existing@test.com", "Test1234!", It.IsAny<CancellationToken>());

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already registered*");
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponse()
        {
            // ARRANGE
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "test@test.com",
                UserName = "test@test.com",
                FirstName = "Oumaima",
                LastName = "Ben"
            };

            _userManagerMock
                .Setup(u => u.FindByEmailAsync("test@test.com"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(u => u.CheckPasswordAsync(user, "Test1234!"))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(u => u.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            _tokenCacheMock
                .Setup(t => t.SetRefreshTokenAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // ACT
            var result = await _authService.LoginAsync("test@test.com", "Test1234!");

            // ASSERT
            result.Should().NotBeNull();
            result.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
            result.Email.Should().Be("test@test.com");

            // Vérifier que le Refresh Token a bien été stocké dans Redis
            _tokenCacheMock.Verify(
                t => t.SetRefreshTokenAsync(
                    user.Id,
                    It.IsAny<string>(),
                    TimeSpan.FromDays(7),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithWrongPassword_ShouldThrowInvalidOperationException()
        {
            // ARRANGE
            var user = new ApplicationUser { Email = "test@test.com" };

            _userManagerMock
                .Setup(u => u.FindByEmailAsync("test@test.com"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(u => u.CheckPasswordAsync(user, "WrongPassword!"))
                .ReturnsAsync(false); // mauvais mot de passe

            // ACT
            var act = async () => await _authService.LoginAsync("test@test.com", "WrongPassword!");

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Invalid email or password*");

            // Redis ne doit PAS être appelé si login échoue
            _tokenCacheMock.Verify(
                t => t.SetRefreshTokenAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WithUnknownEmail_ShouldThrowInvalidOperationException()
        {
            // ARRANGE
            _userManagerMock
                .Setup(u => u.FindByEmailAsync("unknown@test.com"))
                .ReturnsAsync((ApplicationUser?)null); // user introuvable

            // ACT
            var act = async () => await _authService.LoginAsync("unknown@test.com", "Test1234!");

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Invalid email or password*");
        }

        [Fact]
        public async Task LogoutAsync_ShouldRemoveRefreshToken_AndBlacklistAccessToken()
        {
            // ARRANGE
            var userId = "user-123";
            var jti = "jwt-id-456";
            var remainingTime = TimeSpan.FromMinutes(30);

            _tokenCacheMock
                .Setup(t => t.RemoveRefreshTokenAsync(userId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _tokenCacheMock
                .Setup(t => t.BlacklistAccessTokenAsync(jti, remainingTime, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // ACT
            await _authService.LogoutAsync(userId, jti, remainingTime);

            // ASSERT — les deux opérations Redis doivent être appelées
            _tokenCacheMock.Verify(
                t => t.RemoveRefreshTokenAsync(userId, It.IsAny<CancellationToken>()),
                Times.Once);

            _tokenCacheMock.Verify(
                t => t.BlacklistAccessTokenAsync(jti, remainingTime, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // Helper privé
        

        private void SetupJwtConfiguration()
        {
            var jwtSection = new Mock<IConfigurationSection>();
            jwtSection.Setup(s => s["Secret"])
                .Returns("SmartResourceHub_SuperSecretKey_2026_Min32Chars!");
            jwtSection.Setup(s => s["Issuer"])
                .Returns("SmartResourceHub.Identity");
            jwtSection.Setup(s => s["Audience"])
                .Returns("SmartResourceHub.Client");
            jwtSection.Setup(s => s["ExpirationMinutes"])
                .Returns("60");

            _configurationMock
                .Setup(c => c.GetSection("Jwt"))
                .Returns(jwtSection.Object);
        }
    }
}
