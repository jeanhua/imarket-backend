using imarket.utils;
using Microsoft.AspNetCore.Mvc;
using imarket.models;
using Microsoft.AspNetCore.Authorization;
using imarket.service.IService;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Caching.Memory;

namespace imarket.Controllers.open
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtTokenGenerator tokenGenerator;
        private readonly IUserService userService;
        private readonly ILoginService loginService;
        private readonly IMessageService messageService;
        private readonly ILogger<AuthController> logger;
        private readonly IMailService mailService;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        public AuthController(JwtTokenGenerator tokenGenerator, IMessageService messageService, IUserService userService, ILoginService loginService, ILogger<AuthController> _logger, IMemoryCache _cache,IConfiguration configuration,IMailService mailService)
        {
            this.tokenGenerator = tokenGenerator;
            this.userService = userService;
            this.loginService = loginService;
            this.logger = _logger;
            this._cache = _cache;
            this.messageService = messageService;
            this._configuration = configuration;
            this.mailService = mailService;
        }

        [HttpPost("Login")] // api/Auth/Login
        public async Task<IActionResult> Login([FromBody][Required] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // 登录认证：查找用户
            _cache.TryGetValue($"login:{loginRequest.Username}", out var login_cache);
            var loginNums = login_cache as int? ?? 0;
            var ip = IPtool.GetClientIP(HttpContext);
            _cache.TryGetValue("ip:" + ip, out var ip_cache);
            var ipNums = ip_cache as int? ?? 0;
            if (ipNums >= 10)
            {
                return StatusCode(403, new { message = "Too many login attempts. Try 5 minutes later !" });
            }
            _cache.Set("ip:" + ip, ipNums + 1, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
            if (loginNums >= 5)
            {
                return StatusCode(403, new { message = "Too many login attempts. Try 5 minutes later !" });
            }
            var userCheck = await userService.GetUserByUsernameAsync(loginRequest.Username!);
            if (userCheck == null)
            {
                logger.LogInformation($"IP:{ip} login with username:{loginRequest.Username} failed");
                _cache.Set($"login:{loginRequest.Username}", loginNums + 1, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
                return Unauthorized("Invalid username or password.");
            }
            if (userCheck.PasswordHash != loginRequest.Password)
            {
                logger.LogInformation($"IP:{ip} login with username:{loginRequest.Username} failed");
                _cache.Set($"login:{loginRequest.Username}", loginNums + 1, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
                return Unauthorized("Invalid username or password.");
            }
            // 生成 JWT Token
            TokenModels _token;
            if (userCheck.Status == 0)
            {
                _token = tokenGenerator.GenerateToken(userCheck.Username, userCheck.Id, "unverified")!;
            }
            else if (userCheck.Status == 1)
            {
                _token = tokenGenerator.GenerateToken(userCheck.Username, userCheck.Id, userCheck.Role)!;
            }
            else if (userCheck.Status == 2)
            {
                _token = tokenGenerator.GenerateToken(userCheck.Username, userCheck.Id, "banned")!;
            }
            else
            {
                return Unauthorized("Invalid Role.");
            }
            if (_token == null)
            {
                return StatusCode(500);
            }
            return Ok(new { success = true, token = _token });
        }

        [HttpPost("Logout")] // api/Auth/Logout
        public IActionResult Logout()
        {
            return Ok();
        }

        [HttpPost("Register")] // api/Auth/Register
        public async Task<IActionResult> Register([FromBody][Required] RegisterRequest registerRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var ip = IPtool.GetClientIP(HttpContext);
            _cache.TryGetValue("ip:" + ip, out var ip_cache);
            var ipNums = ip_cache as int? ?? 0;
            if (ipNums >= 5)
            {
                return StatusCode(403, new { message = "Too many register attempts. Try 5 minutes later !" });
            }
            // 注册新用户
            // 检查用户名是否已存在
            var userCheck = await userService.GetUserByUsernameAsync(registerRequest.Username!);
            if (userCheck != null)
            {
                return BadRequest("Username already exists.");
            }
            // 检查邮箱是否已存在
            userCheck = await userService.GetUserByEmailAsync(registerRequest.Email!);
            if (userCheck != null)
            {
                return BadRequest("Email already exists.");
            }
            // 检查用户名是否符合要求
            if (UsernameValidator.IsValidUsername(registerRequest.Username!) == false)
            {
                return BadRequest("Invalid username");
            }
            // 检查密码是否符合要求
            if (registerRequest.Password == "" || registerRequest.Password!.Length != 64)
            {
                return BadRequest("Invalid password");
            }
            // 检查邮箱是否符合要求
            if(EmailValidator.IsValidEmail(registerRequest.Email!) == false || registerRequest.Email.Length>50)
            {
                return BadRequest("Invalid email");
            }
            _cache.Set("ip:" + ip, ipNums + 1, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
            // 添加用户
            var newUser = new UserModels
            {
                Id = Guid.NewGuid().ToString(),
                Username = registerRequest.Username!,
                PasswordHash = registerRequest.Password,
                Email = registerRequest.Email!,
                Nickname = RandomChineseNicknameGenerator.GenerateRandomNickname(),
                Avatar = "/images/defaultAvatar.png",
                Role = "user",
                Status = 0,
                CreatedAt = DateTime.Now,
            };
            await loginService.RegisterAsync(newUser);
            return Ok(new { success = true });
        }


        [HttpPost("ChangePassword")] // api/Auth/ChangePassword
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody][Required] ChangePasswordRequest changePasswordRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
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
            if (changePasswordRequest.NewPassword == "" || changePasswordRequest.NewPassword!.Length != 64)
            {
                return BadRequest("Invalid new password.");
            }
            user.PasswordHash = changePasswordRequest.NewPassword;
            await userService.UpdateUserAsync(user.Id, user);
            return Ok(new { success = true });
        }

        [HttpGet("Refresh")] // api/Auth/Refresh
        [Authorize(Roles = "refresh")]
        public async Task<IActionResult> Refresh()
        {
            var userCheck = await userService.GetUserByUsernameAsync(User.Identity!.Name);
            if (userCheck == null)
            {
                return Unauthorized();
            }
            TokenModels _token;
            if (userCheck.Status == 0)
            {
                _token = tokenGenerator.GenerateToken(userCheck.Username, userCheck.Id, "unverified")!;
            }
            else if (userCheck.Status == 1)
            {
                _token = tokenGenerator.GenerateToken(userCheck.Username, userCheck.Id, userCheck.Role)!;
            }
            else if (userCheck.Status == 2)
            {
                _token = tokenGenerator.GenerateToken(userCheck.Username, userCheck.Id, "banned")!;
            }
            else
            {
                return Unauthorized("Invalid Role.");
            }
            if (_token == null)
            {
                return StatusCode(500);
            }
            return Ok(new { success = true, token = _token });
        }

        [HttpPost("ForgotPassword")] // api/Auth/ForgotPassword
        public async Task<IActionResult> ForgotPassword([FromBody][Required] ForgotPasswordRequest forgotPasswordRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await userService.GetUserByEmailAsync(forgotPasswordRequest.Email!);
            if (user == null)
            {
                return BadRequest("User not found.");
            }
            var admin = await userService.GetUserByUsernameAsync(_configuration["admin:Username"]);
            await messageService.CreateMessageAsync(new MessageModels
            {
                Id = Guid.NewGuid().ToString(),
                Content = $"User：[{user.Username}][{user.Nickname}] requests to reset password.",
                SenderId = user.Id,
                ReceiverId = admin.Id,
                CreatedAt = DateTime.Now
            });
            // 发送重置密码邮件
            if (Convert.ToBoolean(_configuration["MailSetting:Enable"]))
            {
                var token = SHA256Encryptor.Encrypt(Guid.NewGuid().ToString());
                _cache.Set(token, user, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                });
                var result = await mailService.SendMail(user.Email, $"{_configuration["SiteInfo:SiteName"]} 密码重置",
                    _configuration["SiteInfo:RestPasswordEmailContent"]!.Replace("{Url}",$"{_configuration["SiteInfo:RestPasswordUrl"]}?token="+token));
                if (result == false)
                {
                    return StatusCode(500, "Send Email Fail.");
                }
                return Ok(new {success = true});
            }
            return Ok(new
            {
                success = true
            });
        }

        [HttpGet("Certificate")] // api/Auth/Certificate?token=xxx
        public async Task<IActionResult> Certificate([FromQuery][Required] string token)
        {
            if(_cache.TryGetValue(token, out var Cache))
            {
                var userCache = Cache as UserModels;
                var user = await userService.GetUserByIdAsync(userCache.Id);
                if (user == null)
                {
                    return NotFound();
                }
                return Ok(new {success=true});
            }
            else
            {
                return BadRequest("Token is invalid or has expired.");
            }
        }

        [HttpPost("Reset")] // api/Auth/Reset
        public async Task<IActionResult> Reset([FromBody][Required]ResetPasswordRequest request)
        {
            if (_cache.TryGetValue(request.Token!, out var Cache))
            {
                var userCache = Cache as UserModels;
                var user = await userService.GetUserByIdAsync(userCache.Id);
                if (user == null)
                {
                    return NotFound();
                }
                user.PasswordHash = request.NewPassword!;
                await userService.UpdateUserAsync(user.Id, user);
                _cache.Remove(request.Token!);
                return Ok(new { success = true });
            }
            else
            {
                return BadRequest("Token is invalid or has expired.");
            }
        }

        public class LoginRequest
        {
            [Required]
            public string? Username { get; set; }
            [Required]
            public string? Password { get; set; }
        }

        public class RegisterRequest
        {
            [Required]
            public string? Username { get; set; }
            [Required]
            public string? Password { get; set; }
            [Required]
            public string? Email { get; set; }
        }

        public class ChangePasswordRequest
        {
            [Required]
            public string? OldPassword { get; set; }
            [Required]
            public string? NewPassword { get; set; }
        }

        public class ForgotPasswordRequest
        {
            [Required]
            public string? Email { get; set; }
        }

        public class ResetPasswordRequest
        {
            [Required] 
            public string? Token { get; set; }
            [Required]
            public string? NewPassword { get; set; }
        }
    }
}
