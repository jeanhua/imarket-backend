using imarket.utils;
using Microsoft.AspNetCore.Mvc;
using imarket.models;
using Microsoft.AspNetCore.Authorization;
using imarket.service.IService;

namespace imarket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtTokenGenerator _tokenGenerator;
        private readonly IUserService userService;
        private readonly ILoginService loginService;
        private readonly ILogger<AuthController> _logger;
        public AuthController(JwtTokenGenerator tokenGenerator,IUserService userService,ILoginService loginService, ILogger<AuthController> _logger)
        {
            _tokenGenerator = tokenGenerator;
            this.userService = userService;
            this.loginService = loginService;
            this._logger = _logger;
        }

        [HttpPost("login")] // api/auth/login
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            // 登录认证：查找用户
            try
            {
                var userCheck = await userService.GetUserByUsernameAsync(loginRequest.Username);
                if (userCheck == null)
                {
                    return Unauthorized("Invalid username or password.");
                }
                if (userCheck.PasswordHash != loginRequest.Password)
                {
                    return Unauthorized("Invalid username or password.");
                }
                // 生成 JWT Token
                string _token;
                if(userCheck.Status==0)
                {
                    _token = _tokenGenerator.GenerateToken(userCheck.Username,userCheck.Id, "unverified")!;
                }
                else if(userCheck.Status == 1)
                {
                    _token = _tokenGenerator.GenerateToken(userCheck.Username, userCheck.Id, userCheck.Role)!;
                }
                else if (userCheck.Status == 2)
                {
                    _token = _tokenGenerator.GenerateToken(userCheck.Username, userCheck.Id, "banned")!;
                }
                else
                {
                    return Unauthorized("Invalid Role.");
                }
                if (_token == null)
                {
                    return StatusCode(500);
                }
                return Ok(new { success=true,token = _token });
            }
            catch (Exception e)
            {
                _logger.LogError("/api/auth/login: " + e.ToString());
                System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString() + "\t" + e.ToString() + "\n");
                return StatusCode(500, "Internal Server Error");
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
                // 检查用户名是否已存在
                var userCheck = await userService.GetUserByUsernameAsync(registerRequest.Username);
                if (userCheck != null)
                {
                    return BadRequest("Username already exists.");
                }
                // 检查邮箱是否已存在
                userCheck = await userService.GetUserByEmailAsync(registerRequest.Email);
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
                    Id = Guid.NewGuid().ToString(),
                    Username = registerRequest.Username,
                    PasswordHash = registerRequest.Password,
                    Email = registerRequest.Email,
                    Nickname = RandomChineseNicknameGenerator.GenerateRandomNickname(),
                    Avatar = "/images/defaultAvatar.jpg",
                    Role = "user",
                    Status = 0,
                    CreatedAt = DateTime.Now,
                };
                await loginService.RegisterAsync(newUser);
                return Ok(new {success=true});
            }
            catch (Exception e)
            {
                _logger.LogError("/api/auth/register: " + e.ToString());
                System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString() + "\t" + e.ToString() + "\n");
                return StatusCode(500, "Internal Server Error");
            }
        }


        [HttpPost("change-password")] // api/auth/change-password
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest changePasswordRequest)
        {
            try
            {
                if (User.Identity!.IsAuthenticated == false)
                {
                    return Unauthorized();
                }
                var user = await userService.GetUserByUsernameAsync(User.Identity.Name!);
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
                await userService.UpdateUserAsync(user.Id, user);
                return Ok(new {success=true});
            }
            catch (Exception e)
            {
                _logger.LogError("/api/auth/change-password: " + e.ToString());
                System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString() + "\t" + e.ToString() + "\n");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("forgot-password")] // api/auth/forgot-password
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest forgotPasswordRequest)
        {
            // 忘记密码：发送重置密码邮件
            try
            {
                var user = await userService.GetUserByEmailAsync(forgotPasswordRequest.Email);
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
                _logger.LogError("/api/auth/forgot-password: " + e.ToString());
                System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString() + "\t" + e.ToString() + "\n");
                return StatusCode(500, "Internal Server Error");
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
