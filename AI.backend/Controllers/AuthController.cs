using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AI.backend.Data;
using AI.backend.Models;

namespace AI.backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                Console.WriteLine($"Login attempt: {request.Username}");
                
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == request.Username);
                
                if (user == null)
                {
                    Console.WriteLine("User not found");
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                // For demo purposes, compare directly since you used 'temp123' as plain text
                if (user.PasswordHash != request.Password) 
                {
                    Console.WriteLine($"Password mismatch. DB has: {user.PasswordHash}, received: {request.Password}");
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                Console.WriteLine($"Login successful: {user.Username} ({user.Role})");
                return Ok(new { 
                    userId = user.Id, 
                    username = user.Username, 
                    role = user.Role 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("debug-users")]
        public async Task<IActionResult> DebugUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                Console.WriteLine($"Found {users.Count} users in database");
                foreach (var user in users)
                {
                    Console.WriteLine($"User: {user.Username}, Password: {user.PasswordHash}, Role: {user.Role}");
                }
                return Ok(users);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting users: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}