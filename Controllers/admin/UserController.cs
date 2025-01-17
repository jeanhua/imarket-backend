using imarket.models;
using imarket.service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace imarket.Controllers.admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles= "admin")]
    public class UserController : ControllerBase
    {
        public readonly IUserService userService;
        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpGet("list")] // api/admin/user/list?page=xx&size=xx
        public async Task<IActionResult> GetUserList([FromQuery]int page, [FromQuery]int size)
        {
            var users = await userService.GetAllUsers(page, size);
            return Ok(new { success = true, users = users });
        }
        [HttpGet("ban")] // api/admin/user/ban?id=xxx
        public async Task<IActionResult> BanUser([FromQuery] string id)
        {
            var user = await userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            user.Status = 2;
            var result = await userService.UpdateUserAsync(id, user);
            if (result == 0)
            {
                return StatusCode(500);
            }
            return Ok(new { success = true });
        }
        [HttpGet("unban")] // api/admin/user/unban?id=xxx
        public async Task<IActionResult> UnbanUser([FromQuery] string id)
        {
            var user = await userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            user.Status = 1;
            var result = await userService.UpdateUserAsync(id, user);
            if (result == 0)
            {
                return StatusCode(500);
            }
            return Ok(new { success = true });
        }
        [HttpPost("create")] // api/admin/user/create
        public async Task<IActionResult> CreateUser([FromBody] UserCreateRequest user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userCheck = await userService.GetUserByUsernameAsync(user.Username!);
            if (userCheck != null)
            {
                return BadRequest("Username already exists.");
            }
            userCheck = await userService.GetUserByEmailAsync(user.Email!);
            if (userCheck != null)
            {
                return BadRequest("Email already exists.");
            }
            var result = await userService.CreateUserAsync(new UserModels
            {
                Id = Guid.NewGuid().ToString(),
                Username = user.Username!,
                Nickname = user.Nickname!,
                PasswordHash = user.PasswordHash!,
                Avatar = user.Avatar?? "/images/defaultAvatar.png",
                Email = user.Email!,
                Role = user.Role!,
                CreatedAt = DateTime.Now,
                Status = user.Status
            });
            if (result == 0)
            {
                return StatusCode(500);
            }
            return Ok(new { success = true });
        }
    }

    public class UserCreateRequest
    {
        [Required]
        public string? Username { get; set; }
        [Required]
        public string? Nickname { get; set; }
        [Required]
        public string? PasswordHash { get; set; }
        public string? Avatar { get; set; }
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? Role { get; set; }
        public int Status { get; set; }
    }
}
