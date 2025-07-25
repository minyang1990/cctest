using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Xunit;

namespace JwtDemo.Tests.Controllers
{
    public class ProtectedControllerTests
    {
        private readonly ProtectedController _controller;

        public ProtectedControllerTests()
        {
            _controller = new ProtectedController();
            
            // Setup mock user context
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.NameIdentifier, "12345"),
                new Claim("username", "testuser")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };
        }

        [Fact]
        public void GetProfile_WithAuthenticatedUser_ReturnsProfileData()
        {
            // Act
            var result = _controller.GetProfile();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
            
            Assert.True(response.Success);
            Assert.Equal("Profile data retrieved successfully", response.Message);
            Assert.NotNull(response.Data);
            
            // Verify profile data structure
            var profileData = response.Data;
            var profileType = profileData.GetType();
            
            Assert.NotNull(profileType.GetProperty("Username"));
            Assert.NotNull(profileType.GetProperty("UserId"));
            Assert.NotNull(profileType.GetProperty("Role"));
            Assert.NotNull(profileType.GetProperty("LastLogin"));
            Assert.NotNull(profileType.GetProperty("Permissions"));
        }

        [Fact]
        public void GetProtectedData_WithAuthenticatedUser_ReturnsProtectedData()
        {
            // Act
            var result = _controller.GetProtectedData();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
            
            Assert.True(response.Success);
            Assert.Equal("Protected data accessed successfully", response.Message);
            Assert.NotNull(response.Data);
            
            // Verify protected data structure
            var protectedData = response.Data;
            var dataType = protectedData.GetType();
            
            Assert.NotNull(dataType.GetProperty("AccessedBy"));
            Assert.NotNull(dataType.GetProperty("AccessTime"));
            Assert.NotNull(dataType.GetProperty("SecretData"));
            Assert.NotNull(dataType.GetProperty("Numbers"));
            Assert.NotNull(dataType.GetProperty("Settings"));
        }

        [Fact]
        public void PerformAction_WithAuthenticatedUser_ReturnsActionResult()
        {
            // Arrange
            var actionData = new { action = "test", value = 42 };

            // Act
            var result = _controller.PerformAction(actionData);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
            
            Assert.True(response.Success);
            Assert.Equal("Action performed successfully", response.Message);
            Assert.NotNull(response.Data);
            
            // Verify action result structure
            var resultData = response.Data;
            var resultType = resultData.GetType();
            
            Assert.NotNull(resultType.GetProperty("PerformedBy"));
            Assert.NotNull(resultType.GetProperty("ActionTime"));
            Assert.NotNull(resultType.GetProperty("ActionData"));
            Assert.NotNull(resultType.GetProperty("Result"));
        }

        [Fact]
        public void GetProfile_ExtractsCorrectUserInformation()
        {
            // Act
            var result = _controller.GetProfile();
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
            
            // Use reflection to get property values
            var profileData = response.Data;
            var username = profileData.GetType().GetProperty("Username")?.GetValue(profileData);
            var userId = profileData.GetType().GetProperty("UserId")?.GetValue(profileData);
            
            // Assert
            Assert.Equal("testuser", username);
            Assert.Equal("12345", userId);
        }

        [Fact] 
        public void GetProtectedData_IncludesUserInformation()
        {
            // Act
            var result = _controller.GetProtectedData();
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
            
            // Use reflection to get property values
            var protectedData = response.Data;
            var accessedBy = protectedData.GetType().GetProperty("AccessedBy")?.GetValue(protectedData);
            
            // Assert
            Assert.Equal("testuser", accessedBy);
        }

        [Fact]
        public void PerformAction_IncludesUserAndActionInformation()
        {
            // Arrange
            var testData = new { message = "test action", id = 123 };

            // Act
            var result = _controller.PerformAction(testData);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
            
            // Use reflection to get property values
            var resultData = response.Data;
            var performedBy = resultData.GetType().GetProperty("PerformedBy")?.GetValue(resultData);
            var actionData = resultData.GetType().GetProperty("ActionData")?.GetValue(resultData);
            
            // Assert
            Assert.Equal("testuser", performedBy);
            Assert.NotNull(actionData);
        }
    }
}