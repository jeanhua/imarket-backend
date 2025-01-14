using imarket.models;
using imarket.service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace imarket.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IPostService postService;
        private readonly IPostCategoriesService postCategoriesService;
        private readonly ICommentService commentService;
        // 缓存
        private readonly IMemoryCache _cache;
        public PostController(IUserService userService, IPostService postService,IPostCategoriesService postCategoriesService,IMemoryCache cache)
        {
            this.userService = userService;
            this.postService = postService;
            this.postCategoriesService = postCategoriesService;
            _cache = cache;
        }

        [HttpGet("Posts")] // api/post/Posts
        public async Task<IActionResult> GetPosts([FromQuery]int page, [FromQuery]int pageSize)
        {
            try
            {
                IEnumerable<PostModels> posts;
                if (_cache.TryGetValue($"Posts_cache{page},{pageSize}", out var post_cache))
                {
                    posts = post_cache as IEnumerable<PostModels>;
                    return Ok(new { success = true, posts = posts });
                }
                posts = await postService.GetAllPostsAsync(page, pageSize);
                _cache.Set("Posts_cache", posts, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
                });
                return Ok(new { success = true, posts = posts });
            }
            catch (Exception e)
            {
                System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString() + "\t" + e.ToString() + "\n");
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet("CategorisedPosts")] // api/post/CategorisedPosts
        public async Task<IActionResult> GetCategorisedPosts([FromQuery] int page, [FromQuery] int pageSize, [FromQuery] int categoryId)
        {
            try
            {
                if (categoryId <= 0)
                {
                    return BadRequest("Invalid category id.");
                }
                IEnumerable<PostModels> posts;
                if (_cache.TryGetValue($"CategorisedPosts_cache{page},{pageSize},{categoryId}", out var post_cache))
                {
                    posts = post_cache as IEnumerable<PostModels>;
                    return Ok(new { success = true, posts = posts });
                }
                posts = await postService.GetPostsByCategoryIdAsync(categoryId, page, pageSize);
                _cache.Set($"CategorisedPosts_cache{page},{pageSize},{categoryId}", posts, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
                });
                return Ok(new { success = true, posts = posts });
            }
            catch (Exception e)
            {
                System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString() + "\t" + e.ToString() + "\n");
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet("{id}")] // api/post/{id}
        public async Task<IActionResult> GetPost([FromRoute] int id)
        {
            try
            {
                if(id <= 0)
                {
                    return BadRequest("Invalid post id.");
                }
                if(_cache.TryGetValue($"Post_cache{id}", out var post_cache))
                {
                    // 缓存命中
                    return Ok(post_cache);
                }
                // 缓存未命中
                var postfind = await postService.GetPostByIdAsync(id);
                if (postfind == null)
                {
                    return NotFound();
                }
                var categoryID = await postCategoriesService.GetPostCategoriesByPostIdAsync(postfind.Id);
                var user = await userService.GetUserByIdAsync(postfind.UserId);
                var comments = await commentService.GetCommentsByPostIdAsync(postfind.Id);
                var response = new
                {
                    success = true,
                    post = new
                    {
                        postfind.Id,
                        postfind.Title,
                        postfind.Content,
                        postfind.Status,
                        categoryID,
                        postfind.CreatedAt,
                        user.Nickname,
                        user.Avatar
                    },
                    comments
                };
                _cache.Set($"Post_cache{id}", response, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
                });
                return Ok(response);
            }
            catch (Exception e)
            {
                System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString() + "\t" + e.ToString() + "\n");
                return StatusCode(500, e.Message);
            }
        }
    }
}
