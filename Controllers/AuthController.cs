using ClinicApi.DTOs;
using ClinicApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClinicApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var result = await _authService.RegisterAsync(request);

            if (result.StartsWith("Error"))
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);

          
          //if email and password are not correct then return error
            if (result.StartsWith("Error"))
            {
                return BadRequest(result);
            }

            
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, 
                Secure = false,  
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(15) 
            };
            // cookies name JWT 
            Response.Cookies.Append("jwt_token", result, cookieOptions);

            return Ok(new { Message = "Login successful! Token has been securely stored in an HttpOnly cookie." });
        }
    }
}