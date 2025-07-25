using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtService _jwtService;
    
    // Demo users - in production, this would be a database
    private readonly Dictionary<string, string> _demoUsers = new()
    {
        { "admin", "password123" },
        { "user", "userpass" },
        { "demo", "demo123" }
    };

    public AuthController(JwtService jwtService)
    {
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Invalid input data"
            });
        }

        // Validate credentials
        if (!_demoUsers.TryGetValue(model.Username, out var storedPassword) || 
            storedPassword != model.Password)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "Invalid username or password"
            });
        }

        // Generate JWT token
        var token = _jwtService.GenerateToken(model.Username, Guid.NewGuid().ToString());
        var expires = DateTime.UtcNow.AddMinutes(60);

        return Ok(new ApiResponse<LoginResponse>
        {
            Success = true,
            Message = "Login successful",
            Data = new LoginResponse
            {
                Token = token,
                Expires = expires,
                Username = model.Username
            }
        });
    }

    [HttpPost("validate")]
    public IActionResult ValidateToken([FromHeader(Name = "Authorization")] string authHeader)
    {
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "Missing or invalid authorization header"
            });
        }

        var token = authHeader.Substring("Bearer ".Length);
        var principal = _jwtService.ValidateToken(token);

        if (principal == null)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "Invalid or expired token"
            });
        }

        var username = principal.FindFirst("username")?.Value;
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Token is valid",
            Data = new { Username = username }
        });
    }
}