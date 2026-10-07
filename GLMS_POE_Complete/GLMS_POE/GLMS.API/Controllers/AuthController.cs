using GLMS.API.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GLMS.API.Controllers
{
    /// <summary>JWT Authentication — Register and Login.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _users;
        private readonly IConfiguration _config;
        public AuthController(UserManager<IdentityUser> users, IConfiguration config)
        { _users = users; _config = config; }

        /// <summary>Register a new user account.</summary>
        [HttpPost("register")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var user = new IdentityUser { UserName = dto.Email, Email = dto.Email };
            var result = await _users.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors.Select(e => e.Description));
            return StatusCode(201, new { message = "User registered. Please log in." });
        }

        /// <summary>Login and receive a JWT Bearer token.</summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _users.FindByEmailAsync(dto.Email);
            if (user == null || !await _users.CheckPasswordAsync(user, dto.Password))
                return Unauthorized(new { message = "Invalid credentials." });
            return Ok(BuildToken(user));
        }

        private AuthResponseDto BuildToken(IdentityUser user)
        {
            var expiry = DateTime.UtcNow.AddHours(8);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,   user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
            };
            var key  = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwt  = new JwtSecurityToken(_config["Jwt:Issuer"], _config["Jwt:Issuer"],
                claims, expires: expiry, signingCredentials: cred);
            return new AuthResponseDto
            {
                Token  = new JwtSecurityTokenHandler().WriteToken(jwt),
                Email  = user.Email!,
                Expiry = expiry
            };
        }
    }
}
