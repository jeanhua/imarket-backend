using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using imarket.service.IService;
using System.ComponentModel.DataAnnotations;
using imarket.plugin;

namespace imarket.Controllers.open
{
    [Route("api/[controller]")]
    [Authorize(Roles = "user,unverified,banned,admin")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly PluginManager pluginManager;
        public AccountController(IUserService userService,PluginManager pluginManager)
        {
            this.userService = userService;
            this.pluginManager = pluginManager;
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
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
            var result_before = await pluginManager.ExecuteBeforeAsync("api/Account/Info", new object[] {user});
            if (result_before != null)
            {
                return Ok(result_before);
            }
            var response = new
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
            };
            var result_after = await pluginManager.ExecuteAfterAsync("api/Account/Info", response);
            if (result_after != null)
            {
                return Ok(result_after);
            }
            return Ok(response);
        }

        /// <summary>
        /// 编辑用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost("Edit")] // api/Account/Edit
        public async Task<IActionResult> edit([FromBody][Required] EditRequest user)
        {
            if (User.Identity!.IsAuthenticated == false)
            {
                return Unauthorized();
            }
            var args = new object[] { user };
            var result_before = await pluginManager.ExecuteBeforeAsync("api/Account/Edit", args);
            if (result_before != null)
            {
                return Ok(result_before);
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
            var response = new
            {
                success = true
            };
            var result_after = await pluginManager.ExecuteAfterAsync("api/Account/Edit", response);
            if (result_after != null)
            {
                return Ok(result_after);
            }
            return Ok(response);
        }
    }

    public class EditRequest
    {
        public string? Nickname { get; set; }
        public string? Avatar { get; set; }
        public string? Email { get; set; }
    }
}
