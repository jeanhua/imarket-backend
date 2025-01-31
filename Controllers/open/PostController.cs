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
        private readonly IConfiguration configuration;
        private readonly ILogger<PostController> logger;
        // 缓存
        private readonly IMemoryCache _cache;
        public PostController(IUserService userService, IConfiguration configuration, IFavoriteService favoriteService, ILikeService likeService, IPostService postService, IPostCategoriesService postCategoriesService, IImageService imageService, ICommentService commentService, IMemoryCache cache, ILogger<PostController> logger)
        {
            this.userService = userService;
            this.postService = postService;
            this.commentService = commentService;
            this.configuration = configuration;
            this.postCategoriesService = postCategoriesService;
            this.imageService = imageService;
            this.likeService = likeService;
            this.favoriteService = favoriteService;
            this.logger = logger;
            _cache = cache;
        }

        /// <summary>
        /// 获取所有帖子
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("Posts")] // api/Post/Posts?page=1&pageSize=10
        public async Task<IActionResult> GetPosts([FromQuery] int page=1, [FromQuery] int pageSize=10)
        {
            if(_cache.TryGetValue($"Posts_cache_page{page}_pageSize{pageSize}", out var posts_cache))
            {
                return Ok(posts_cache);
            }
            var posts = await postService.GetAllPostsAsync(page,pageSize);
            var postsResponse = new List<PostsResponse>();
            foreach(var post in posts)
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
            _cache.Set($"Posts_cache_page{page}_pageSize{pageSize}", response, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(int.Parse(configuration["Cache:Posts"]))
            });
            return Ok(response);
        }

        /// <summary>
        /// 获取所有分类
        /// </summary>
        /// <returns></returns>
        [HttpGet("Categories")] // api/Post/Categories
        public async Task<IActionResult> GetCategories()
        {
            const string cacheKey = "AllCategories";
            if (_cache.TryGetValue(cacheKey, out var cachedCategories))
            {
                return Ok(new { success = true, categories = cachedCategories });
            }
            var categories = await postCategoriesService.GetAllCategoriesAsync();
            _cache.Set(cacheKey, categories, TimeSpan.FromMinutes(10));
            return Ok(new { success = true, categories });
        }

        /// <summary>
        /// 获取分类详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Categories/{id}")] // api/Post/Categories/{id}
        public async Task<IActionResult> GetCategoryById([FromRoute][Required] ulong id)
        {
            var cacheKey = $"Category_{id}";
            if (_cache.TryGetValue(cacheKey, out var cachedCategory))
            {
                return Ok(new { success = true, category = cachedCategory });
            }
            var category = await postCategoriesService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound(new { success = false, message = "Category not found." });
            }
            _cache.Set(cacheKey, category, TimeSpan.FromMinutes(10)); // 缓存 10 分钟
            return Ok(new { success = true, category });
        }

        /// <summary>
        /// 获取分类下的帖子
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("CategorisedPosts")] // api/Post/CategorisedPosts
        public async Task<IActionResult> GetCategorisedPosts([FromQuery][Required] ulong categoryId,[FromQuery] int page=1, [FromQuery] int pageSize=10)
        {
            if (_cache.TryGetValue($"GetCategorisedPosts{categoryId}_page{page}_pageSize{pageSize}", out var posts_cache))
            {
                return Ok(posts_cache);
            }
            var posts = await postService.GetPostsByCategoryIdAsync(categoryId,page, pageSize);
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
            _cache.Set($"GetCategorisedPosts{categoryId}_page{page}_pageSize{pageSize}", response, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(int.Parse(configuration["Cache:Posts"]))
            });
            return Ok(response);
        }

        /// <summary>
        /// 获取帖子详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")] // api/Post/{id}
        public async Task<IActionResult> GetPost([FromRoute][Required] ulong id)
        {
            // 检查用户是否已登录
            var isUserAuthenticated = User.Identity.IsAuthenticated;
            // 如果用户未登录，尝试从缓存中获取数据
            if (!isUserAuthenticated && _cache.TryGetValue($"Post_cache{id}", out var post_cache))
            {
                // 缓存命中
                return Ok(post_cache);
            }
            // 缓存未命中或用户已登录，从数据库获取数据
            var postfind = await postService.GetPostByIdAsync(id);
            if (postfind == null)
            {
                return NotFound();
            }
            var categoryID = await postCategoriesService.GetPostCategoriesByPostIdAsync(postfind.Id);
            var user = await userService.GetUserByIdAsync(postfind.UserId);
            var likeNums = await likeService.GetPostLikeNumsByPostIdAsync(postfind.Id);
            var favoriteNms = await favoriteService.GetFavoriteNumsByPostIdAsync(postfind.Id);
            var images = await imageService.GetImagesByPostId(postfind.Id);
            bool isLiked = false;
            bool isFavorite = false;
            if (isUserAuthenticated)
            {
                var me = await userService.GetUserByUsernameAsync(User.Identity.Name);
                isLiked = await likeService.CheckUserLikePostAsync(me.Id, postfind.Id);
                isFavorite = await favoriteService.CheckIsFavorite(me.Id, postfind.Id);
            }
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
                    likeNums,
                    favoriteNms,
                    isLiked,
                    isFavorite,
                    postfind.CreatedAt,
                    user?.Username,
                    user?.Nickname,
                    user?.Avatar
                },
            };
            // 如果用户未登录，将结果存入缓存
            if (!isUserAuthenticated)
            {
                _cache.Set($"Post_cache{id}", response, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(int.Parse(configuration["Cache:SinglePost"]))
                });
            }
            return Ok(response);
        }

        /// <summary>
        /// 创建帖子
        /// </summary>
        /// <param name="postReq"></param>
        /// <returns></returns>
        [HttpPost("Create")] // api/Post/Create
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> CreatePost([FromBody][Required] CreatePostRequest postReq)
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
            var user = await userService.GetUserByUsernameAsync(User.Identity!.Name!);
            var post = new PostModels
            {
                Title = postReq.Title!,
                Content = postReq.Content!,
                UserId = user.Id,
                Status = 0,
                CreatedAt = DateTime.Now
            };
            var result1 = await postService.CreatePostAsync(post);
            await postCategoriesService.CreatePostCategoryAsync(new PostCategoryModels()
            {
                CategoryId = postReq.CategoryId,
                PostId = result1,
            });
            if (postReq.Images != null)
            {
                foreach (var image in postReq.Images!)
                {
                    var resut2 = await imageService.SaveImageAsync(new ImageModels
                    {
                        Url = image,
                        PostId = result1,
                        CreatedAt = DateTime.Now
                    });
                    if (resut2 == 0)
                    {
                        return StatusCode(500, $"{image}upload failed.");
                    }
                }
            }
            return Ok(new { success = true });
        }

        /// <summary>
        /// 删除帖子
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        [HttpGet("Delete")] // api/Post/Delete?postId=xxx
        [Authorize(Roles = "admin,user")]
        public async Task<IActionResult> DeletePost([FromQuery][Required] ulong postId)
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
            // 删除帖子
            var result = await postService.DeletePostAsync(postId);
            if (result == 0)
            {
                return StatusCode(500);
            }
            return Ok(new { success = true });
        }

        /// <summary>
        /// 结束帖子
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        [HttpGet("Finish")] //api/Post/Finish?postId=xxx
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> FinishPost([FromQuery][Required] ulong postId)
        {
            var user = await userService.GetUserByUsernameAsync(User.Identity.Name);
            var post = await postService.GetPostByIdAsync(postId);
            if (post == null)
            {
                return NotFound();
            }
            if(post.UserId != user.Id)
            {
                return BadRequest("you are not the author.");
            }
            post.Status = 1;
            var result = await postService.UpdatePostAsync(post);
            if (result == 0)
            {
                return StatusCode(500);
            } 
            return Ok(new { success = true });
        }

        /// <summary>
        /// 收藏帖子
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        [HttpGet("Favorite")] // api/Post/Favorite?postId=xxx
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> FavoritePost([FromQuery][Required] ulong postId)
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
            var isFavorite = await favoriteService.CheckIsFavorite(user.Id, postId);
            if (isFavorite)
            {
                return Ok(new { success = true,message = "your have favorited it." });
            }
            var result = await favoriteService.CreatePostFavoriteAsync(postId,user.Id);
            if (result == 0)
            {
                return StatusCode(500);
            }
            return Ok(new { success = true });
        }

        /// <summary>
        /// 取消收藏帖子
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        [HttpGet("Unfavorite")] // api/Post/Unfavorite?postId=xxx
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> UnFavoritePost([FromQuery][Required] ulong postId)
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
            var isFavorite = await favoriteService.CheckIsFavorite(user.Id,postId);
            if(isFavorite)
            {
                var result = await favoriteService.DeletePostFavoriteAsync(postId, user.Id);
            }
            _cache.Remove($"Post_cache{postId}");
            return Ok(new { success = true });
        }

        /// <summary>
        /// 点赞帖子
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        [HttpGet("Like")] // api/Post/Like?postId=xxx
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> LikePost([FromQuery][Required] ulong postId)
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
            var isLike = await likeService.CheckUserLikePostAsync(user.Id, postId);
            if (isLike)
            {
                return Ok(new { success = true,message = "you have already liked it." });
            }
            var like = new LikeModels
            {
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
            _cache.Remove($"Post_cache{postId}");
            return Ok(new { success = true });
        }

        /// <summary>
        /// 取消点赞帖子
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        [HttpGet("Unlike")] // api/Post/Unlike?postId=xxx
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> UnLikePost([FromQuery][Required] ulong postId)
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
            var isLike = await likeService.CheckUserLikePostAsync(user.Id,postId);
            if(!isLike)
            {
                return Ok(new { success = true ,message = "you have already unliked it."});
            }
            var result = await likeService.DeleteLikeAsync(new LikeModels
            {
                PostId = postId,
                UserId = user.Id,
                CommentId = null,
                CreatedAt = DateTime.Now
            });
            if (result == 0)
            {
                return StatusCode(500);
            }
            _cache.Remove($"Post_cache{postId}");
            return Ok(new { success = true });
        }
        
        /// <summary>
        /// 获取用户收藏的帖子
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("GetFavorites")] // api/post/GetFavorites
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> GetFavorites([FromQuery] int page=1, [FromQuery] int pageSize=10)
        {
            var user = await userService.GetUserByUsernameAsync(User.Identity!.Name!);
            if (user == null)
            {
                return Unauthorized("Invalid user.");
            }
            var favorites = await favoriteService.GetPostFavoriteByUserId(user.Id, page, pageSize);
            var favorite = new List<dynamic>();
            foreach (var fav in favorites)
            {
                favorite.Add(new
                {
                    PostId = fav.PostId,
                    CreatedAt = fav.CreatedAt
                });
            }
            return Ok(new { success = true, favorite });
        }
    }

    public class CreatePostRequest
    {
        [Required]
        public string? Title { get; set; }
        [Required]
        public string? Content { get; set; }
        [Required]
        public ulong CategoryId { get; set; }
        public string[]? Images { get; set; }
    }

    public class PostsResponse
    {
        public ulong Id { get; set; }
        public string Title { get; set; }
        public string Nickname { get; set; }
        public string Avatar { get; set; }
        public string Content { get; set; }
        public int FavoriteNums { get; set; }
        public int LikeNums { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
