using System.Security.Claims;
using Xunit;

namespace JwtDemo.Tests.Services
{
    public class JwtServiceTests
    {
        private readonly JwtService _jwtService;
        private const string TestSecretKey = "test-256-bit-secret-key-for-testing-purposes-only";

        public JwtServiceTests()
        {
            _jwtService = new JwtService(TestSecretKey);
        }

        [Fact]
        public void GenerateToken_WithValidCredentials_ReturnsValidJwtToken()
        {
            // Arrange
            var username = "testuser";
            var userId = "12345";

            // Act
            var token = _jwtService.GenerateToken(username, userId);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
            Assert.Contains(".", token); // JWT tokens contain dots
            
            // Verify token has 3 parts (header.payload.signature)
            var tokenParts = token.Split('.');
            Assert.Equal(3, tokenParts.Length);
        }

        [Fact]
        public void GenerateToken_WithDifferentUsers_GeneratesDifferentTokens()
        {
            // Arrange
            var user1 = "user1";
            var user2 = "user2";

            // Act
            var token1 = _jwtService.GenerateToken(user1, "1");
            var token2 = _jwtService.GenerateToken(user2, "2");

            // Assert
            Assert.NotEqual(token1, token2);
        }

        [Fact]
        public void ValidateToken_WithValidToken_ReturnsClaimsPrincipal()
        {
            // Arrange
            var username = "testuser";
            var userId = "12345";
            var token = _jwtService.GenerateToken(username, userId);

            // Act
            var principal = _jwtService.ValidateToken(token);

            // Assert
            Assert.NotNull(principal);
            Assert.Equal(username, principal.FindFirst(ClaimTypes.Name)?.Value);
            Assert.Equal(userId, principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            Assert.Equal(username, principal.FindFirst("username")?.Value);
        }

        [Fact]
        public void ValidateToken_WithInvalidToken_ReturnsNull()
        {
            // Arrange
            var invalidToken = "invalid.jwt.token";

            // Act
            var principal = _jwtService.ValidateToken(invalidToken);

            // Assert
            Assert.Null(principal);
        }

        [Fact]
        public void ValidateToken_WithExpiredToken_ReturnsNull()
        {
            // Arrange - Create service with very short expiration
            var shortExpiryService = new TestJwtService(TestSecretKey, -1); // Expired immediately
            var token = shortExpiryService.GenerateToken("testuser", "12345");
            
            // Wait to ensure token is expired
            Thread.Sleep(100);

            // Act
            var principal = _jwtService.ValidateToken(token);

            // Assert
            Assert.Null(principal);
        }

        [Fact]
        public void ValidateToken_WithNullOrEmptyToken_ReturnsNull()
        {
            // Act & Assert
            Assert.Null(_jwtService.ValidateToken(null));
            Assert.Null(_jwtService.ValidateToken(string.Empty));
            Assert.Null(_jwtService.ValidateToken("   "));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void GenerateToken_WithInvalidUsername_ThrowsArgumentException(string username)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _jwtService.GenerateToken(username, "12345"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void GenerateToken_WithInvalidUserId_ThrowsArgumentException(string userId)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _jwtService.GenerateToken("testuser", userId));
        }
    }

    // Helper class for testing with custom expiration
    public class TestJwtService : JwtService
    {
        private readonly int _expirationMinutes;

        public TestJwtService(string secretKey, int expirationMinutes) : base(secretKey)
        {
            _expirationMinutes = expirationMinutes;
        }

        public new string GenerateToken(string username, string userId)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId cannot be null or empty", nameof(userId));

            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var key = System.Text.Encoding.ASCII.GetBytes("test-256-bit-secret-key-for-testing-purposes-only");
            var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim("username", username)
                }),
                Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                    new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key), 
                    Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}