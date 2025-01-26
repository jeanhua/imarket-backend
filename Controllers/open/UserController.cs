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

        public UserController(IUserService userService, IMemoryCache _cache, IPostService postService, ILogger<UserController> logger, IFavoriteService favoriteService, ILikeService likeService, IConfiguration configuration)
        {
            this.userService = userService;
            this.cache = _cache;
            this.postService = postService;
            this.logger = logger;
            this.favoriteService = favoriteService;
            this.likeService = likeService;
            this.configuration = configuration;
        }

        /// <summary>
        /// 获取用户帖子
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpGet("Posts")] // api/User/Posts?username=xxx
        [Authorize]
        public async Task<IActionResult> GetUserPosts([FromQuery][Required]string? username, [FromQuery]int page = 1, [FromQuery]int pageSize = 10)
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
            // 缓存
            if(cache.TryGetValue($"userPosts_{user.Username}_{page}_{pageSize}",out var posts_cache))
            {
                return Ok(new { success = true, posts = posts_cache });
            }
            var posts = await postService.GetPostsByUserIdAsync(user.Id, page, pageSize);
            var response = new List<PostsResponse>();
            foreach (var post in posts)
            {
                response.Add(new PostsResponse
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
            cache.Set($"userPosts_{user.Username}_{page}_{pageSize}", response, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(int.Parse(configuration["Cache:Posts"]))
            });
            return Ok(new{success=true ,posts = response});
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
