using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProtectedController : ControllerBase
{
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Profile data retrieved successfully",
            Data = new
            {
                Username = username,
                UserId = userId,
                Role = "Demo User",
                LastLogin = DateTime.UtcNow.AddHours(-2),
                Permissions = new[] { "read", "write", "demo" }
            }
        });
    }

    [HttpGet("data")]
    public IActionResult GetProtectedData()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Protected data accessed successfully",
            Data = new
            {
                AccessedBy = username,
                AccessTime = DateTime.UtcNow,
                SecretData = "This is protected information that requires authentication",
                Numbers = new[] { 1, 2, 3, 42, 100 },
                Settings = new
                {
                    Theme = "dark",
                    Language = "en",
                    Notifications = true
                }
            }
        });
    }

    [HttpPost("action")]
    public IActionResult PerformAction([FromBody] dynamic data)
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Action performed successfully",
            Data = new
            {
                PerformedBy = username,
                ActionTime = DateTime.UtcNow,
                ActionData = data,
                Result = "Action completed successfully"
            }
        });
    }
}