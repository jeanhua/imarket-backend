using imarket.models;
using imarket.service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;

namespace imarket.Controllers.open
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IPostService postService;
        private readonly IPostCategoriesService postCategoriesService;
        private readonly ICommentService commentService;
        private readonly IImageService imageService;
        private readonly IFavoriteService favoriteService;
        private readonly ILikeService likeService;
        private readonly ILogger<PostController> _logger;
        // 缓存
        private readonly IMemoryCache _cache;
        public PostController(IUserService userService, IFavoriteService favoriteService, ILikeService likeService, IPostService postService, IPostCategoriesService postCategoriesService, IImageService imageService, ICommentService commentService, IMemoryCache cache, ILogger<PostController> logger)
        {
            this.userService = userService;
            this.postService = postService;
            this.commentService = commentService;
            this.postCategoriesService = postCategoriesService;
            this.imageService = imageService;
            this.likeService = likeService;
            this.favoriteService = favoriteService;
            _logger = logger;
            _cache = cache;
        }

        [HttpGet("Posts")] // api/post/Posts
        public async Task<IActionResult> GetPosts([FromQuery] int page, [FromQuery] int pageSize)
        {
            IEnumerable<PostModels>? posts;
            if (_cache.TryGetValue($"Posts_cache{page},{pageSize}", out var post_cache))
            {
                posts = post_cache as IEnumerable<PostModels>;
                return Ok(new { success = true, posts });
            }
            posts = await postService.GetAllPostsAsync(page, pageSize);
            _cache.Set("Posts_cache", posts, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
            });
            return Ok(new { success = true, posts });
        }

        [HttpGet("categories")] // api/post/categories
        public async Task<IActionResult> GetCategories()
        {
            var categories = await postCategoriesService.GetAllCategoriesAsync();
            return Ok(new { success = true, categories });
        }

        [HttpGet("CategorisedPosts")] // api/post/CategorisedPosts
        public async Task<IActionResult> GetCategorisedPosts([FromQuery] int page, [FromQuery] int pageSize, [FromQuery] string categoryId)
        {
            IEnumerable<PostModels>? posts;
            if (_cache.TryGetValue($"CategorisedPosts_cache{page},{pageSize},{categoryId}", out var post_cache))
            {
                posts = post_cache as IEnumerable<PostModels>;
                return Ok(new { success = true, posts });
            }
            posts = await postService.GetPostsByCategoryIdAsync(categoryId, page, pageSize);
            _cache.Set($"CategorisedPosts_cache{page},{pageSize},{categoryId}", posts, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
            });
            return Ok(new { success = true, posts });
        }

        [HttpGet("{id}")] // api/post/{id}
        public async Task<IActionResult> GetPost([FromRoute] string id)
        {
            if (_cache.TryGetValue($"Post_cache{id}", out var post_cache))
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
            var likes = await likeService.GetPostLikeNumsByPostIdAsync(postfind.Id);
            var images = await imageService.GetImagesByPostId(postfind.Id);
            var isLiked = await likeService.CheckUserLikePostAsync(User.Identity!.Name!, postfind.Id);
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
                    likes,
                    isLiked,
                    postfind.CreatedAt,
                    user?.Nickname,
                    user?.Avatar
                },
                comments,
            };
            _cache.Set($"Post_cache{id}", response, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
            });
            return Ok(response);
        }

        [HttpPost("create")] // api/post/create
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest postReq)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (postReq.Title == "" || postReq.Content == "")
            {
                return BadRequest("Invalid post.");
            }
            if (postReq.Content!.Length > 3000)
            {
                return BadRequest("Content is too long.");
            }
            var categorys = await postCategoriesService.GetAllCategoriesAsync();
            var categorysExist = false;
            foreach (var category in categorys)
            {
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
            var postId = Guid.NewGuid().ToString();
            var user = await userService.GetUserByUsernameAsync(User.Identity!.Name!);
            var post = new PostModels
            {
                Id = postId,
                Title = postReq.Title!,
                Content = postReq.Content!,
                UserId = user.Id,
                Status = 0,
                CreatedAt = DateTime.Now
            };
            var result1 = await postService.CreatePostAsync(post);
            if (postReq.Images != null)
            {
                foreach (var image in postReq.Images!)
                {
                    var resut2 = await imageService.SaveImageAsync(new ImageModels
                    {
                        Id = Guid.NewGuid().ToString(),
                        Url = image,
                        PostId = postId,
                        CreatedAt = DateTime.Now
                    });
                    if (resut2 == 0)
                    {
                        return StatusCode(500, $"{image}upload failed");
                    }
                }
            }
            if (result1 == 0)
            {
                return StatusCode(500, "create post failed");
            }
            return Ok(new { success = true });
        }

        [HttpPost("delete")] // api/post/delete?postId=xxx
        [Authorize(Roles = "admin,user")]
        public async Task<IActionResult> DeletePost([FromQuery] string postId)
        {
            var post = await postService.GetPostByIdAsync(postId);
            if (post == null)
            {
                return NotFound();
            }
            var author = await userService.GetUserByIdAsync(post.UserId);
            if (author?.Username != User.Identity!.Name! && User.IsInRole("admin") == false)
            {
                return BadRequest("You are not the author of this post.");
            }
            var result = await postService.DeletePostAsync(postId);
            await imageService.DeleteImagesByPostIdAsync(postId);
            if (result == 0)
            {
                return StatusCode(500);
            }
            return Ok(new { success = true });
        }

        [HttpGet("finish")] //api/post/finish?postId=xxx
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> FinishPost([FromQuery] string postId)
        {
            var user = await userService.GetUserByUsernameAsync(User.Identity.Name);
            var post = await postService.GetPostByIdAsync(postId);
            if (post == null)
            {
                return NotFound();
            }
            if(post.UserId != user.Id)
            {
                return BadRequest("you are not the author !");
            }
            post.Status = 1;
            var result = await postService.UpdatePostAsync(post);
            if (result == 0)
            {
                return StatusCode(500);
            } 
            return Ok(new { success = true });
        }

        [HttpGet("like")] // api/post/like?postId=xxx
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> LikePost([FromQuery] string postId)
        {
            var post = await postService.GetPostByIdAsync(postId);
            if (post == null)
            {
                return NotFound("Post not found.");
            }
            var user = await userService.GetUserByUsernameAsync(User.Identity!.Name!);
            if (user == null)
            {
                return Unauthorized("Invalid user.");
            }
            var like = new LikeModels
            {
                Id = Guid.NewGuid().ToString(),
                PostId = postId,
                CommentId = null,
                UserId = user.Id,
                CreatedAt = DateTime.Now
            };
            var result = await likeService.CreateLikeAsync(like);
            if (result == 0)
            {
                return StatusCode(500);
            }
            return Ok(new { success = true });
        }

        [HttpGet("favorite")] // api/post/favorite?postId=xxx
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> FavoritePost([FromQuery] string postId)
        {
            var post = await postService.GetPostByIdAsync(postId);
            if (post == null)
            {
                return NotFound("Post not found.");
            }
            var user = await userService.GetUserByUsernameAsync(User.Identity!.Name!);
            if (user == null)
            {
                return Unauthorized("Invalid user.");
            }
            var favorite = new FavoriteModels
            {
                Id = Guid.NewGuid().ToString(),
                PostId = postId,
                UserId = user.Id,
                CreatedAt = DateTime.Now,
            };
            var result = await favoriteService.CreatePostFavoriteAsync(postId,user.Id);
            if (result == 0)
            {
                return StatusCode(500);
            }
            return Ok(new { success = true });
        }

        [HttpGet("unfavorite")] // api/post/unfavorite?postId=xxx
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> UnFavoritePost([FromQuery] string postId)
        {
            var post = await postService.GetPostByIdAsync(postId);
            if (post == null)
            {
                return NotFound("Post not found.");
            }
            var user = await userService.GetUserByUsernameAsync(User.Identity!.Name!);
            if (user == null)
            {
                return Unauthorized("Invalid user.");
            }
            var result = await favoriteService.DeletePostFavoriteAsync(postId, user.Id);
            if (result == 0)
            {
                return StatusCode(500);
            }
            return Ok(new { success = true });
        }

        [HttpGet("unlike")] // api/post/unlike?postId=xxx
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> UnLikePost([FromQuery] string postId)
        {
            var post = await postService.GetPostByIdAsync(postId);
            if (post == null)
            {
                return NotFound("Post not found.");
            }
            var user = await userService.GetUserByUsernameAsync(User.Identity!.Name!);
            if (user == null)
            {
                return Unauthorized("Invalid user.");
            }
            var result = await likeService.DeleteLikeAsync(new LikeModels
            {
                Id = Guid.NewGuid().ToString(),
                PostId = postId,
                UserId = user.Id,
                CommentId = null,
                CreatedAt = DateTime.Now
            });
            if (result == 0)
            {
                return StatusCode(500);
            }
            return Ok(new { success = true });
        }

        [HttpGet("GetFavorites")] // api/post/GetFavorites
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> GetFavorites([FromQuery] int page, [FromQuery] int pageSize)
        {
            var user = await userService.GetUserByUsernameAsync(User.Identity!.Name!);
            if (user == null)
            {
                return Unauthorized("Invalid user.");
            }
            var favorites = await favoriteService.GetPostFavoriteByUserId(user.Id, page, pageSize);
            return Ok(new { success = true, favorites });
        }
    }

    public class CreatePostRequest
    {
        [Required]
        public string? Title { get; set; }
        [Required]
        public string? Content { get; set; }
        [Required]
        public string? CategoryId { get; set; }
        public string[]? Images { get; set; }
    }
}
