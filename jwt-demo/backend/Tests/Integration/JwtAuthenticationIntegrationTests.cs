using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;

namespace JwtDemo.Tests.Integration
{
    public class JwtAuthenticationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public JwtAuthenticationIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task CompleteAuthenticationFlow_ShouldWork()
        {
            // Step 1: Login
            var loginRequest = new LoginModel
            {
                Username = "admin",
                Password = "password123"
            };

            var loginContent = new StringContent(
                JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8,
                "application/json");

            var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
            
            // Assert login success
            loginResponse.EnsureSuccessStatusCode();
            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(
                loginResponseContent, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(loginResult);
            Assert.True(loginResult.Success);
            Assert.NotNull(loginResult.Data);
            Assert.NotEmpty(loginResult.Data.Token);

            // Step 2: Use token to access protected endpoint
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.Data.Token);

            var protectedResponse = await _client.GetAsync("/api/protected/profile");
            
            // Assert protected access success
            protectedResponse.EnsureSuccessStatusCode();
            var protectedContent = await protectedResponse.Content.ReadAsStringAsync();
            var protectedResult = JsonSerializer.Deserialize<ApiResponse<object>>(
                protectedContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(protectedResult);
            Assert.True(protectedResult.Success);
            Assert.Equal("Profile data retrieved successfully", protectedResult.Message);

            // Step 3: Validate token
            var validateResponse = await _client.PostAsync("/api/auth/validate", new StringContent(""));
            validateResponse.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginModel
            {
                Username = "admin",
                Password = "wrongpassword"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/auth/login", content);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ProtectedEndpoint_WithoutToken_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/protected/profile");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ProtectedEndpoint_WithInvalidToken_ShouldReturnUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid.jwt.token");

            // Act
            var response = await _client.GetAsync("/api/protected/profile");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData("admin", "password123")]
        [InlineData("user", "userpass")]
        [InlineData("demo", "demo123")]
        public async Task Login_WithAllValidUsers_ShouldSucceed(string username, string password)
        {
            // Arrange
            var loginRequest = new LoginModel
            {
                Username = username,
                Password = password
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/auth/login", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(username, result.Data.Username);
        }

        [Fact]
        public async Task ProtectedEndpoints_WithValidToken_ShouldAllReturnSuccess()
        {
            // Step 1: Get valid token
            var token = await GetValidTokenAsync();
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Step 2: Test all protected endpoints
            var endpoints = new[]
            {
                "/api/protected/profile",
                "/api/protected/data"
            };

            foreach (var endpoint in endpoints)
            {
                var response = await _client.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<object>>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                Assert.NotNull(result);
                Assert.True(result.Success);
            }
        }

        [Fact]
        public async Task ProtectedAction_WithValidToken_ShouldWork()
        {
            // Step 1: Get valid token
            var token = await GetValidTokenAsync();
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Step 2: Perform action
            var actionData = new { action = "test", data = "integration test" };
            var content = new StringContent(
                JsonSerializer.Serialize(actionData),
                Encoding.UTF8,
                "application/json");

            var response = await _client.PostAsync("/api/protected/action", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<object>>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Action performed successfully", result.Message);
        }

        private async Task<string> GetValidTokenAsync()
        {
            var loginRequest = new LoginModel
            {
                Username = "admin",
                Password = "password123"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8,
                "application/json");

            var response = await _client.PostAsync("/api/auth/login", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result.Data.Token;
        }
    }
}