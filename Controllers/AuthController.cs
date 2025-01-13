using imarket.utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using imarket.models;
using Microsoft.AspNetCore.Authorization;

namespace imarket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtTokenGenerator _tokenGenerator;

        public AuthController(JwtTokenGenerator tokenGenerator)
        {
            _tokenGenerator = tokenGenerator;
        }

        [HttpPost("login")] // api/auth/login
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            // 登录认证：查找用户
            try
            {
                var userCheck = await service.Service.getInstance().GetUserByUsernameAsync(loginRequest.Username);
                if (userCheck == null)
                {
                    return Unauthorized("Invalid username or password.");
                }
                if (userCheck.PasswordHash != loginRequest.Password)
                {
                    return Unauthorized("Invalid username or password.");
                }
                // 生成 JWT Token
                var _token = _tokenGenerator.GenerateToken(userCheck.Nickname, userCheck.Id);
                if (_token == null)
                {
                    return StatusCode(500);
                }
                return Ok(new { success=true,token = _token });
            }
            catch (Exception e)
            {
                System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString() + "\t" + e.ToString() + "\n");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("logout")] // api/auth/logout
        public IActionResult Logout()
        {
            return Ok();
        }

        [HttpPost("register")] // api/auth/register
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            // 注册新用户
            try
            {
                var imarketService = service.Service.getInstance();
                // 检查用户名是否已存在
                var userCheck = await imarketService.GetUserByUsernameAsync(registerRequest.Username);
                if (userCheck != null)
                {
                    return BadRequest("Username already exists.");
                }
                // 检查邮箱是否已存在
                userCheck = await imarketService.GetUserByEmailAsync(registerRequest.Email);
                if (userCheck != null)
                {
                    return BadRequest("Email already exists.");
                }
                // 检查用户名是否符合要求
                if (UsernameValidator.IsValidUsername(registerRequest.Username)==false)
                {
                    return BadRequest("Invalid username");
                }
                // 检查密码是否符合要求
                if (registerRequest.Password == "" || registerRequest.Password.Length!= 64)
                {
                    return BadRequest("Invalid password");
                }
                // 添加用户
                var newUser = new UserModels
                {
                    Id = 0,// 无关
                    Username = registerRequest.Username,
                    PasswordHash = registerRequest.Password,
                    Email = registerRequest.Email,
                    Nickname = RandomChineseNicknameGenerator.GenerateRandomNickname(),
                    Avatar = "/images/defaultAvatar.jpg",
                    Role = "user",
                    Status = 0,
                    CreatedAt = DateTime.Now,
                };
                await imarketService.RegisterAsync(newUser);
                return Ok(new {success=true});
            }
            catch (Exception e)
            {
                System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString() + "\t" + e.ToString() + "\n");
                return StatusCode(500, e.Message);
            }
        }


        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest changePasswordRequest)
        {
            try
            {
                if (User.Identity.IsAuthenticated == false)
                {
                    return Unauthorized();
                }
                var user = await service.Service.getInstance().GetUserByUsernameAsync(User.Identity.Name!);
                if (user == null)
                {
                    return Unauthorized();
                }
                if (user.PasswordHash != changePasswordRequest.OldPassword)
                {
                    return BadRequest("Invalid old password.");
                }
                if (changePasswordRequest.NewPassword == "" || changePasswordRequest.NewPassword.Length != 64)
                {
                    return BadRequest("Invalid new password.");
                }
                user.PasswordHash = changePasswordRequest.NewPassword;
                await service.Service.getInstance().UpdateUserAsync(user.Id, user);
                return Ok(new {success=true});
            }
            catch (Exception e)
            {
                System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString() + "\t" + e.ToString() + "\n");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest forgotPasswordRequest)
        {
            // 忘记密码：发送重置密码邮件
            try
            {
                var user = await service.Service.getInstance().GetUserByEmailAsync(forgotPasswordRequest.Email);
                if (user == null)
                {
                    return BadRequest("User not found.");
                }
                // 发送重置密码邮件

                var response = new
                {
                    success = true
                };
                return Ok(response);
            }
            catch (Exception e)
            {
                System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString() + "\t" + e.ToString() + "\n");
                return StatusCode(500, e.Message);
            }
        }

        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class RegisterRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string Email { get; set; }
        }

        public class ChangePasswordRequest
        {
            public string OldPassword { get; set; }
            public string NewPassword { get; set; }
        }

        public class ForgotPasswordRequest
        {
            public string Email { get; set; }
        }
    }
}
