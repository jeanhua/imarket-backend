using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using imarket.service.IService;
using System.ComponentModel.DataAnnotations;

namespace imarket.Controllers.open
{
    [Route("api/[controller]")]
    [Authorize(Roles = "user,unverified,banned,admin")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserService userService;
        public AccountController(IUserService userService)
        {
            this.userService = userService;
        }
        [HttpGet("Info")] // api/Account/Info
        public async Task<IActionResult> getinfo()
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
            return Ok(new
            {
                success = true,
                account = new
                {
                    username = user.Username,
                    nickname = user.Nickname,
                    avatar = user.Avatar,
                    email = user.Email,
                    status = user.Status
                }
            });
        }

        [HttpPost("Edit")] // api/Account/Edit
        public async Task<IActionResult> edit([FromBody][Required] EditRequest user)
        {
            if (User.Identity!.IsAuthenticated == false)
            {
                return Unauthorized();
            }
            var userCheck = await userService.GetUserByUsernameAsync(User.Identity.Name!);
            if (userCheck == null)
            {
                return Unauthorized();
            }
            userCheck.Nickname = user.Nickname ?? userCheck.Nickname;
            userCheck.Avatar = user.Avatar ?? userCheck.Avatar;
            userCheck.Email = user.Email ?? userCheck.Email;
            await userService.UpdateUserAsync(userCheck.Id, userCheck);
            return Ok(new { success = true });
        }
    }

    public class EditRequest
    {
        public string? Nickname { get; set; }
        public string? Avatar { get; set; }
        public string? Email { get; set; }
    }
}
