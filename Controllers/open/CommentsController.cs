using Microsoft.AspNetCore.Mvc;
using imarket.service.IService;
using Microsoft.Extensions.Caching.Memory;
using imarket.models;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace imarket.Controllers.open
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService commentService;
        private readonly IUserService userService;
        private readonly IPostService postService;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration configuration;
        private readonly ILikeService likeService;
        private readonly ILogger<CommentsController> _logger;
        public CommentsController(IConfiguration configuration, ILikeService likeService, ICommentService commentService, IPostService postService, IUserService userService, IMemoryCache cache, ILogger<CommentsController> _logger)
        {
            this.configuration = configuration;
            this.commentService = commentService;
            this.userService = userService;
            this.postService = postService;
            this.likeService = likeService;
            this._logger = _logger;
            _cache = cache;
        }
        [HttpGet("{postid}")] // api/Comments/{postid}
        public async Task<IActionResult> GetCommentsByPostIdAsync([FromRoute] string postid)
        {
            _cache.TryGetValue(postid, out var comments);
            if (comments != null)
            {
                return Ok(new { success = true, comments });
            }
            var Comments = await commentService.GetCommentsByPostIdAsync(postid);
            var response = new List<CommentResponse>();
            foreach (var comment in Comments)
            {
                var avatar = "/images/defaultAvatar.png";
                var likeNum = 0;
                var isLike = false;
                try
                {
                    avatar = (await userService.GetUserByIdAsync(comment.UserId))!.Avatar;
                    likeNum = await likeService.GetCommentLikeNumsByCommentIdAsync(comment.Id);
                    isLike = await likeService.CheckUserLikeCommentAsync(comment.Id, User.Identity!.Name!);
                }
                catch
                {
                    avatar = "/images/defaultAvatar.png";
                }
                response.Add(new CommentResponse
                {
                    Id = comment.Id,
                    UserId = comment.UserId,
                    UserAvatar = avatar,
                    Content = comment.Content,
                    isLike = isLike,
                    likeNum = likeNum,
                    CreatedAt = comment.CreatedAt
                });
            }
            _cache.Set(postid, response, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(int.Parse(configuration["Cache:SinglePost"]))
            });
            return Ok(new { success = true, comments = response });
        }
        [HttpPost("Create")] // api/Comments/Create
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> CreateCommentAsync([FromBody] CommentPostRequest comment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await userService.GetUserByUsernameAsync(User.Identity!.Name!);
            var post = await postService.GetPostByIdAsync(comment.PostId!);
            if (post == null)
            {
                return NotFound("Post not found.");
            }
            if (user == null)
            {
                return Unauthorized("Invalid user.");
            }
            if (comment.Content == null)
            {
                return BadRequest("Content is required.");
            }
            if (comment.Content.Length == 0)
            {
                return BadRequest("Content is required.");
            }
            if (comment.Content.Length > 1000)
            {
                return BadRequest("Content is too long.");
            }
            if (post == null)
            {
                return NotFound("Post not found.");
            }
            if (post.Status == 1)
            {
                return BadRequest("Post is finished");
            }
            var comment_new = new CommentModels
            {
                Id = Guid.NewGuid().ToString(),
                PostId = comment.PostId!,
                Content = comment.Content,
                UserId = user.Id,
                CreatedAt = DateTime.Now
            };
            var result = await commentService.CreateCommentAsync(comment_new);
            if (result == 0)
            {
                return StatusCode(500);
            }
            return Ok(new { success = true, commentId = comment_new.Id });
        }

        [HttpGet("Delete")] // api/Comments/Delete?commentId=xxx
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> DeleteCommentAsync([FromQuery] string commentId)
        {
            var user = await userService.GetUserByUsernameAsync(User.Identity!.Name!);
            if (user == null)
            {
                return Unauthorized("Invalid user.");
            }
            var comment = await commentService.GetCommentByIdAsync(commentId!);
            if (comment == null)
            {
                return NotFound("Comment not found.");
            }
            if (comment.UserId != user.Id && User.IsInRole("admin") == false)
            {
                return Unauthorized("You are not the author of this comment.");
            }
            var result = await commentService.DeleteCommentAsync(commentId!);
            if (result == 0)
            {
                return StatusCode(500);
            }
            return Ok(new { success = true });
        }

        [HttpGet("Like")] // api/Comments/Like?commentId=xxx
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> LikeCommentAsync([FromQuery] string commentId)
        {
            var user = await userService.GetUserByUsernameAsync(User.Identity!.Name!);
            if (user == null)
            {
                return Unauthorized("Invalid user.");
            }
            var comment = await commentService.GetCommentByIdAsync(commentId!);
            if (comment == null)
            {
                return NotFound("Comment not found.");
            }
            var isLike = await likeService.CheckUserLikeCommentAsync(user.Id, commentId);
            if(isLike)
            {
                return Ok(new { success = true,message = "you have liked it." });
            }
            var result = await likeService.CreateLikeAsync(new LikeModels
            {
                Id = Guid.NewGuid().ToString(),
                PostId = null,
                CommentId = commentId!,
                UserId = user.Id,
                CreatedAt = DateTime.Now
            });
            if (result == 0)
            {
                return StatusCode(500);
            }
            return Ok(new { success = true });
        }

        [HttpGet("UnLike")] // api/Comments/UnLike?commentId=xxx
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> UnLikeCommentAsync([FromQuery] string commentId)
        {
            var user = await userService.GetUserByUsernameAsync(User.Identity!.Name!);
            if (user == null)
            {
                return Unauthorized("Invalid user.");
            }
            var comment = await commentService.GetCommentByIdAsync(commentId!);
            if (comment == null)
            {
                return NotFound("Comment not found.");
            }
            var isLike = await likeService.CheckUserLikeCommentAsync(user.Id, commentId);
            if (!isLike)
            {
                return Ok(new { success = true, message = "you have unliked it." });
            }
            var result = await likeService.DeleteLikeAsync(new LikeModels
            {
                Id = comment.Id,
                CommentId = commentId!,
                UserId = user.Id,
                CreatedAt = comment.CreatedAt,
                PostId = null
            });
            if (result == 0)
            {
                return StatusCode(500);
            }
            return Ok(new { success = true });
        }
    }

    public class CommentPostRequest
    {
        [Required]
        public string? PostId { get; set; }
        [Required]
        public string? Content { get; set; }
    }
    public class CommentResponse
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string? UserAvatar { get; set; }
        public string? Content { get; set; }

        public bool isLike { get; set; }
        public int? likeNum { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
