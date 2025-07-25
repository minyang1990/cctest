using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace JwtDemo.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<JwtService> _mockJwtService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockJwtService = new Mock<JwtService>("test-secret-key");
            _controller = new AuthController(_mockJwtService.Object);
        }

        [Fact]
        public void Login_WithValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var loginModel = new LoginModel
            {
                Username = "admin",
                Password = "password123"
            };
            var expectedToken = "test.jwt.token";
            _mockJwtService.Setup(s => s.GenerateToken("admin", It.IsAny<string>()))
                          .Returns(expectedToken);

            // Act
            var result = _controller.Login(loginModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<LoginResponse>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Login successful", response.Message);
            Assert.NotNull(response.Data);
            Assert.Equal(expectedToken, response.Data.Token);
            Assert.Equal("admin", response.Data.Username);
        }

        [Fact]
        public void Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginModel = new LoginModel
            {
                Username = "admin",
                Password = "wrongpassword"
            };

            // Act
            var result = _controller.Login(loginModel);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ApiResponse<object>>(unauthorizedResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Invalid username or password", response.Message);
        }

        [Fact]
        public void Login_WithNonExistentUser_ReturnsUnauthorized()
        {
            // Arrange
            var loginModel = new LoginModel
            {
                Username = "nonexistent",
                Password = "password123"
            };

            // Act
            var result = _controller.Login(loginModel);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ApiResponse<object>>(unauthorizedResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Invalid username or password", response.Message);
        }

        [Theory]
        [InlineData("", "password123")]
        [InlineData("admin", "")]
        [InlineData(null, "password123")]
        [InlineData("admin", null)]
        public void Login_WithInvalidModel_ReturnsBadRequest(string username, string password)
        {
            // Arrange
            var loginModel = new LoginModel
            {
                Username = username,
                Password = password
            };
            
            // Simulate model validation failure
            _controller.ModelState.AddModelError("Username", "Username is required");

            // Act
            var result = _controller.Login(loginModel);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Invalid input data", response.Message);
        }

        [Theory]
        [InlineData("admin", "password123")]
        [InlineData("user", "userpass")]
        [InlineData("demo", "demo123")]
        public void Login_WithAllValidDemoUsers_ReturnsSuccess(string username, string password)
        {
            // Arrange
            var loginModel = new LoginModel
            {
                Username = username,
                Password = password
            };
            var expectedToken = $"token-for-{username}";
            _mockJwtService.Setup(s => s.GenerateToken(username, It.IsAny<string>()))
                          .Returns(expectedToken);

            // Act
            var result = _controller.Login(loginModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<LoginResponse>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(expectedToken, response.Data.Token);
            Assert.Equal(username, response.Data.Username);
        }

        [Fact]
        public void ValidateToken_WithValidToken_ReturnsSuccess()
        {
            // Arrange
            var authHeader = "Bearer valid.jwt.token";
            var mockPrincipal = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim("username", "testuser")
                }));
            
            _mockJwtService.Setup(s => s.ValidateToken("valid.jwt.token"))
                          .Returns(mockPrincipal);

            // Act
            var result = _controller.ValidateToken(authHeader);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Token is valid", response.Message);
        }

        [Fact]
        public void ValidateToken_WithInvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var authHeader = "Bearer invalid.jwt.token";
            _mockJwtService.Setup(s => s.ValidateToken("invalid.jwt.token"))
                          .Returns((System.Security.Claims.ClaimsPrincipal)null);

            // Act
            var result = _controller.ValidateToken(authHeader);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ApiResponse<object>>(unauthorizedResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Invalid or expired token", response.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("InvalidHeader")]
        [InlineData("Basic dGVzdA==")]
        [InlineData(null)]
        public void ValidateToken_WithInvalidAuthHeader_ReturnsUnauthorized(string authHeader)
        {
            // Act
            var result = _controller.ValidateToken(authHeader);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ApiResponse<object>>(unauthorizedResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Missing or invalid authorization header", response.Message);
        }
    }
}