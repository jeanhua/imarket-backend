using imarket.plugin;
using imarket.service.IService;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IFavoriteService favoriteService;
        private readonly ILikeService likeService;
        private readonly IConfiguration configuration;
        private readonly PluginManager pluginManager;

        public UserController(IUserService userService, IMemoryCache _cache, IPostService postService, ILogger<UserController> logger, IFavoriteService favoriteService, ILikeService likeService, IConfiguration configuration,PluginManager pluginManager)
        {
            this.userService = userService;
            this.cache = _cache;
            this.postService = postService;
            this.logger = logger;
            this.favoriteService = favoriteService;
            this.likeService = likeService;
            this.configuration = configuration;
            this.pluginManager = pluginManager;
        }

        /// <summary>
        /// 获取用户帖子
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpGet("Posts")] // api/User/Posts?username=xxx
        [Authorize]
        public async Task<IActionResult> GetUserPosts([FromQuery][Required]string username, [FromQuery]int page = 1, [FromQuery]int pageSize = 10)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var args = new object[] { username, page, pageSize };
            var result_before = await pluginManager.ExecuteBeforeAsync(HttpContext.Request.Path.Value!, args, User?.Identity?.Name);
            if (result_before != null)
            {
                return Ok(result_before);
            }
            var user = await userService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return NotFound();
            }
            // 缓存
            if(cache.TryGetValue($"userPosts_{user.Username}_{page}_{pageSize}",out var posts_cache))
            {
                return Ok(new { success = true, posts = posts_cache });
            }
            var posts = await postService.GetPostsByUserIdAsync(user.Id, page, pageSize);
            var result = new List<PostsResponse>();
            foreach (var post in posts)
            {
                result.Add(new PostsResponse
                {
                    Id = post.Id,
                    Title = post.Title,
                    Content = post.Content,
                    Nickname = user.Nickname,
                    Avatar = user.Avatar,
                    FavoriteNums = await favoriteService.GetFavoriteNumsByPostIdAsync(post.Id),
                    LikeNums = await likeService.GetPostLikeNumsByPostIdAsync(post.Id),
                    CreatedAt = post.CreatedAt
                });
            }
            cache.Set($"userPosts_{user.Username}_{page}_{pageSize}", result, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(int.Parse(configuration["Cache:Posts"]))
            });
            var response = new
            {
                success = true,
                posts = result
            };
            var result_after = await pluginManager.ExecuteAfterAsync(HttpContext.Request.Path.Value!, response, User?.Identity?.Name);
            if (result_after != null)
            {
                return Ok(result_after);
            }
            return Ok(response);
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpGet("Info")] // api/User/Info?username=xxx
        [Authorize]
        public async Task<IActionResult> GetInfo([FromQuery][Required] string? username)
        {
            if(!ModelState.IsValid)
                { return BadRequest(ModelState); }
            var args = new object[] { username };
            var result_before = await pluginManager.ExecuteBeforeAsync(HttpContext.Request.Path.Value!, args, User?.Identity?.Name);
            if (result_before != null)
            {
                return Ok(result_before);
            }
            var user = await userService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return NotFound();
            }
            var response = new
            {
                success = true,
                username = user.Username,
                nickname = user.Nickname,
                avatar = user.Avatar,
                email = user.Email,
                status = user.Status
            };
            var result_after = await pluginManager.ExecuteAfterAsync(HttpContext.Request.Path.Value!, response, User?.Identity?.Name);
            if (result_after != null)
            {
                return Ok(result_after);
            }
            return Ok(response);
        }
    }
}
