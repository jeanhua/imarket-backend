using imarket.entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace imarket.Controllers
{
    [Route("api/[controller]")] // api/auth/
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtTokenGenerator _tokenGenerator;

        public AuthController(JwtTokenGenerator tokenGenerator)
        {
            _tokenGenerator = tokenGenerator;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            // 登录认证
            if (loginRequest.Username == "test" && loginRequest.Password == "password")
            {
                var token = _tokenGenerator.GenerateToken(loginRequest.Username);
                return Ok(new { Token = token });
            }
            return Unauthorized("Invalid username or password");
        }
    }
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
