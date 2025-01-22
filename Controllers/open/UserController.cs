using imarket.service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;

namespace imarket.Controllers.open
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> logger;
        private readonly IMemoryCache cache;
        private readonly IUserService userService;
        private readonly IPostService postService;

        public UserController(IUserService userService, IMemoryCache _cache, IPostService postService, ILogger<UserController> logger)
        {
            this.userService = userService;
            this.cache = _cache;
            this.postService = postService;
            this.logger = logger;
        }

        [HttpGet("Posts")] // api/User/Posts?username=xxx
        [Authorize]
        public async Task<IActionResult> GetUserPosts([FromQuery][Required]string? username)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await userService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return NotFound();
            }
            var posts = await postService.GetPostsByUserIdAsync(user.Id);
            return Ok(posts);
        }

        [HttpGet("Info")] // api/User/Info?username=xxx
        [Authorize]
        public async Task<IActionResult> GetInfo([FromQuery][Required] string? username)
        {
            if(!ModelState.IsValid)
                { return BadRequest(ModelState); }
            var user = await userService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(new
            {
                Id = user.Id,
                Username = user.Username,
                Nickname = user.Nickname,
                Avatar = user.Avatar,
                Status = user.Status
            });
        }
    }
}
