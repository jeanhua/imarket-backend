using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using imarket.service.IService;

namespace imarket.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserService userService;
        public AccountController(IUserService userService)
        {
            this.userService = userService;
        }
        [HttpGet("info")] // api/account/info
        public async Task<IActionResult> getinfo()
        {
            try
            {
                if (User.Identity.IsAuthenticated == false)
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
            catch (Exception e)
            {
                Console.WriteLine("/api/account/info: " + e.ToString());
                System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString() + "\t" + e.ToString() + "\n");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("edit")]
        public async Task<IActionResult> edit([FromBody] EditRequest user)
        {
            try
            {
                if (User.Identity.IsAuthenticated == false)
                {
                    return Unauthorized();
                }
                var userCheck = await userService.GetUserByUsernameAsync(User.Identity.Name!);
                if (userCheck == null)
                {
                    return Unauthorized();
                }
                userCheck.Nickname = user.Nickname;
                userCheck.Avatar = user.Avatar;
                userCheck.Email = user.Email;
                await userService.UpdateUserAsync(userCheck.Id, userCheck);
                return Ok(new { success = true });
            }
            catch (Exception e)
            {
                Console.WriteLine("/api/account/edit: " + e.ToString());
                System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString() + "\t" + e.ToString() + "\n");
                return StatusCode(500, e.Message);
            }
        }
    }

    public class EditRequest
    {
        public string Nickname { get; set; }
        public string Avatar { get; set; }
        public string Email { get; set; }
    }
}
