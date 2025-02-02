using imarket.plugin;
using imarket.service.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;

namespace imarket.Controllers.open
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ILogger<SearchController> logger;
        private readonly IPostService postService;
        private readonly IUserService userService;
        private readonly IMemoryCache _cache;
        private readonly IFavoriteService favoriteService;
        private readonly ILikeService likeService;
        private readonly IConfiguration configuration;
        private readonly PluginManager pluginManager;
        public SearchController(ILogger<SearchController> logger, IPostService postService, IUserService userService, IMemoryCache _cache, IFavoriteService favoriteService, ILikeService likeService, IConfiguration configuration,PluginManager pluginManager)
        {
            this.logger = logger;
            this.postService = postService;
            this.userService = userService;
            this._cache = _cache;
            this.favoriteService = favoriteService;
            this.likeService = likeService;
            this.configuration = configuration;
            this.pluginManager = pluginManager;
        }

        /// <summary>
        /// 搜索帖子
        /// </summary>
        /// <param name="keyWord"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("Posts")]
        public async Task<IActionResult> SearchPost([FromQuery][Required] string keyWord, [FromQuery]int page = 1, [FromQuery]int pageSize=10)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest("require keyWord");
            }
            var args = new object[] { keyWord, page, pageSize };
            var result_before = await pluginManager.ExecuteBeforeAsync("api/Search/Posts", args, User?.Identity?.Name);
            if (result_before != null)
            {
                return Ok(result_before);
            }
            if (_cache.TryGetValue($"Posts_cache_Search{keyWord}_page{page}_pageSize{pageSize}", out var posts_cache))
            {
                return Ok(posts_cache);
            }
            var posts = await postService.SearchPostsAsync(keyWord, page, pageSize);
            var postsResponse = new List<PostsResponse>();
            foreach (var post in posts)
            {
                var user = await userService.GetUserByIdAsync(post.UserId);
                postsResponse.Add(new PostsResponse
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
            var response = new
            {
                success = true,
                posts = postsResponse
            };
            _cache.Set($"Posts_cache_Search{keyWord}_page{page}_pageSize{pageSize}", response, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(int.Parse(configuration["Cache:Posts"]))
            });
            var result_after = await pluginManager.ExecuteAfterAsync("api/Search/Posts", response, User?.Identity?.Name);
            if (result_after != null)
            {
                return Ok(result_after);
            }
            return Ok(response);
        }

        /// <summary>
        /// 获取热门帖子
        /// </summary>
        /// <returns></returns>
        [HttpGet("HotRanking")]
        public async Task<IActionResult> GetHotRankong()
        {
            var args = new object[] { };
            var result_before = await pluginManager.ExecuteBeforeAsync("api/Search/HotRanking", args, User?.Identity?.Name);
            if (result_before != null)
            {
                return Ok(result_before);
            }
            if (_cache.TryGetValue("hotranking",out var res))
            {
                return Ok(res);
            }
            var likeMost = await likeService.GetHotRankingAsync();
            var favoriteMost = await favoriteService.GetHotRankingAsync();
            var response = new
            {
                success = true,
                like = likeMost,
                favorite = favoriteMost
            };
            _cache.Set("hotranking", response, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(int.Parse(configuration["Cache:Posts"]))
            });
            var result_after = await pluginManager.ExecuteAfterAsync("api/Search/HotRanking", response, User?.Identity?.Name);
            if (result_after != null)
            {
                return Ok(result_after);
            }
            return Ok(response);
        }

    }
}
