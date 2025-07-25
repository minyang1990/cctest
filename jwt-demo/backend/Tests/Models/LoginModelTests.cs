using System.ComponentModel.DataAnnotations;
using Xunit;

namespace JwtDemo.Tests.Models
{
    public class LoginModelTests
    {
        [Fact]
        public void LoginModel_WithValidData_PassesValidation()
        {
            // Arrange
            var model = new LoginModel
            {
                Username = "testuser",
                Password = "password123"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.Empty(validationResults);
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        public void LoginModel_WithInvalidUsername_FailsValidation(string username)
        {
            // Arrange
            var model = new LoginModel
            {
                Username = username,
                Password = "password123"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Username"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        public void LoginModel_WithInvalidPassword_FailsValidation(string password)
        {
            // Arrange
            var model = new LoginModel
            {
                Username = "testuser",
                Password = password
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Password"));
        }

        [Fact]
        public void LoginModel_WithShortUsername_FailsValidation()
        {
            // Arrange
            var model = new LoginModel
            {
                Username = "ab", // Less than 3 characters
                Password = "password123"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Username"));
        }

        [Fact]
        public void LoginModel_WithShortPassword_FailsValidation()
        {
            // Arrange
            var model = new LoginModel
            {
                Username = "testuser",
                Password = "12345" // Less than 6 characters
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Password"));
        }

        [Fact]
        public void LoginModel_WithLongUsername_FailsValidation()
        {
            // Arrange
            var model = new LoginModel
            {
                Username = new string('a', 51), // More than 50 characters
                Password = "password123"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Username"));
        }

        [Fact]
        public void LoginModel_WithLongPassword_FailsValidation()
        {
            // Arrange
            var model = new LoginModel
            {
                Username = "testuser",
                Password = new string('a', 101) // More than 100 characters
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Password"));
        }

        [Fact]
        public void LoginResponse_PropertiesSetCorrectly()
        {
            // Arrange
            var token = "test.jwt.token";
            var expires = DateTime.UtcNow.AddMinutes(60);
            var username = "testuser";

            // Act
            var response = new LoginResponse
            {
                Token = token,
                Expires = expires,
                Username = username
            };

            // Assert
            Assert.Equal(token, response.Token);
            Assert.Equal(expires, response.Expires);
            Assert.Equal(username, response.Username);
        }

        [Fact]
        public void ApiResponse_PropertiesSetCorrectly()
        {
            // Arrange
            var testData = new { message = "test" };
            
            // Act
            var response = new ApiResponse<object>
            {
                Success = true,
                Message = "Test message",
                Data = testData
            };

            // Assert
            Assert.True(response.Success);
            Assert.Equal("Test message", response.Message);
            Assert.Equal(testData, response.Data);
        }

        private static IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, context, validationResults, true);
            return validationResults;
        }
    }
}