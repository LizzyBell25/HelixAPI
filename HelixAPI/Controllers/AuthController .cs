using HelixAPI.Models;
using HelixAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelixAPI.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(AuthService authService) : ControllerBase
    {
        private readonly AuthService _authService = authService;

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel login)
        {
            // Validate user credentials (e.g., check against a database)
            // For simplicity, we'll assume the login is always successful

            var token = _authService.GenerateJwtToken(login.UserId);

            return Ok(new { Token = token });
        }
    }
}