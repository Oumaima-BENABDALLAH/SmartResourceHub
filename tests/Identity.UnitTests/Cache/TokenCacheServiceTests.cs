using FluentAssertions;
using Identity.Infrastructure.Cache;
using Moq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.UnitTests.Cache
{
    public  class TokenCacheServiceTests
    {
        private readonly Mock<IDatabase> _databaseMock;
        private readonly Mock<IConnectionMultiplexer> _multiplexerMock;
        private readonly TokenCacheService _service;

        public TokenCacheServiceTests()
        {
            _databaseMock = new Mock<IDatabase>();
            _multiplexerMock = new Mock<IConnectionMultiplexer>();

            // Quand GetDatabase() est appelé  retourne le mock
            _multiplexerMock
                .Setup(m => m.GetDatabase(-1, null))
                .Returns(_databaseMock.Object);

            _service = new TokenCacheService(_multiplexerMock.Object);
        }


        [Fact]
        public async Task SetRefreshTokenAsync_ShouldCallRedisSet_WithCorrectKey()
        {
            // ARRANGE
            var userId = "user-123";
            var token = "my-refresh-token";
            var expiration = TimeSpan.FromDays(7);

            _databaseMock
                .Setup(d => d.StringSetAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<bool>(),
                    It.IsAny<When>(),
                    It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            // ACT
            await _service.SetRefreshTokenAsync(userId, token, expiration);

            // ASSERT  vérifie que Redis SET a été appelé avec la bonne clé
            _databaseMock.Verify(
               d => d.StringSetAsync(
               $"refresh_token:{userId}",   // La clé exacte (ce qui compte vraiment)
               token,                       // La valeur exacte
               It.IsAny<TimeSpan?>(),       // On accepte n'importe quel TTL
               It.IsAny<bool>(),            // On accepte n'importe quelle valeur pour keepTtl
               It.IsAny<When>(),            // On accepte n'importe quelle condition
               It.IsAny<CommandFlags>()),   // On accepte n'importe quels flags
               Times.Once);
        }

        [Fact]
        public async Task GetRefreshTokenAsync_WhenTokenExists_ShouldReturnToken()
        {
            // ARRANGE
            var userId = "user-123";
            var expectedToken = "my-refresh-token";

            _databaseMock
                .Setup(d => d.StringGetAsync(
                    $"refresh_token:{userId}",
                    It.IsAny<CommandFlags>()))
                .ReturnsAsync(expectedToken);

            // ACT
            var result = await _service.GetRefreshTokenAsync(userId);

            // ASSERT
            result.Should().Be(expectedToken);
        }

        [Fact]
        public async Task GetRefreshTokenAsync_WhenTokenExpired_ShouldReturnNull()
        {
            // ARRANGE
            var userId = "user-123";

            _databaseMock
                .Setup(d => d.StringGetAsync(
                    $"refresh_token:{userId}",
                    It.IsAny<CommandFlags>()))
                .ReturnsAsync(RedisValue.Null); // simule token expiré

            // ACT
            var result = await _service.GetRefreshTokenAsync(userId);

            // ASSERT
            result.Should().BeNull();
        }

        [Fact]
        public async Task RemoveRefreshTokenAsync_ShouldCallRedisDelete()
        {
            // ARRANGE
            var userId = "user-123";

            _databaseMock
                .Setup(d => d.KeyDeleteAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            // ACT
            await _service.RemoveRefreshTokenAsync(userId);

            // ASSERT
            _databaseMock.Verify(
                d => d.KeyDeleteAsync(
                    $"refresh_token:{userId}",
                    It.IsAny<CommandFlags>()),
                Times.Once);
        }

        [Fact]
        public async Task BlacklistAccessTokenAsync_ShouldStoreWithCorrectPrefix()
        {
            // ARRANGE
            var jti = "unique-jwt-id";
            var expiration = TimeSpan.FromMinutes(30);

            _databaseMock
                .Setup(d => d.StringSetAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<bool>(),
                    It.IsAny<When>(),
                    It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            // ACT
            await _service.BlacklistAccessTokenAsync(jti, expiration);

            // ASSERT
            _databaseMock.Verify(
                d => d.StringSetAsync(
                    $"blacklist:{jti}",  // clé avec bon préfixe
                    "true",
                    expiration,
                    It.IsAny<bool>(),
                    It.IsAny<When>(),
                    It.IsAny<CommandFlags>()),
                Times.Once);
        }

        [Fact]
        public async Task IsAccessTokenBlacklistedAsync_WhenBlacklisted_ShouldReturnTrue()
        {
            // ARRANGE
            var jti = "unique-jwt-id";

            _databaseMock
                .Setup(d => d.KeyExistsAsync(
                    $"blacklist:{jti}",
                    It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            // ACT
            var result = await _service.IsAccessTokenBlacklistedAsync(jti);

            // ASSERT
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsAccessTokenBlacklistedAsync_WhenNotBlacklisted_ShouldReturnFalse()
        {
            // ARRANGE
            var jti = "unique-jwt-id";

            _databaseMock
                .Setup(d => d.KeyExistsAsync(
                    $"blacklist:{jti}",
                    It.IsAny<CommandFlags>()))
                .ReturnsAsync(false);

            // ACT
            var result = await _service.IsAccessTokenBlacklistedAsync(jti);

            // ASSERT
            result.Should().BeFalse();
        }
    }
}
