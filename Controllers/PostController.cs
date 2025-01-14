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
        private readonly IImageService imageService;
        // 缓存
        private readonly IMemoryCache _cache;
        public PostController(IUserService userService, IPostService postService,IPostCategoriesService postCategoriesService,IImageService imageService,IMemoryCache cache)
        {
            this.userService = userService;
            this.postService = postService;
            this.postCategoriesService = postCategoriesService;
            this.imageService = imageService;
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
                Console.WriteLine("/api/post/Posts: " + e.ToString());
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
                Console.WriteLine("/api/post/CategorisedPosts: " + e.ToString());
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
                var images = await imageService.GetImagesByPostId(postfind.Id);
                var response = new
                {
                    success = true,
                    post = new
                    {
                        postfind.Id,
                        postfind.Title,
                        postfind.Content,
                        images,
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
                Console.WriteLine("/api/post/{id}: " + e.ToString());
                System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString() + "\t" + e.ToString() + "\n");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("create")] // api/post/create
        public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest postReq)
        {
            try
            {
                if (postReq.Title == "" || postReq.Content == "")
                {
                    return BadRequest("Invalid post.");
                }
                var categorys = await postCategoriesService.GetAllCategoriesAsync();
                var categorysExist = false;
                foreach (var category in categorys) {
                    if (category.Id == postReq.CategoryId)
                    {
                        categorysExist = true;
                        break;
                    }
                }
                if (!categorysExist)
                {
                    return BadRequest("Invalid category.");
                }
                var post = new PostModels
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = postReq.Title,
                    Content = postReq.Content,
                    UserId = User.Identity!.Name!,
                    Status = 0,
                    CreatedAt = DateTime.Now
                };
                var result = await postService.CreatePostAsync(post);
                if (result == 0)
                {
                    return StatusCode(500);
                }
                return Ok(new { success = true });
            }
            catch (Exception e)
            {
                System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString() + "\t" + e.ToString() + "\n");
                return StatusCode(500, e.Message);
            }
        }
    }

    public class CreatePostRequest
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string CategoryId { get; set; }
        public string[] Images { get; set; }
    }
}
