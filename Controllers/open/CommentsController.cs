using Microsoft.AspNetCore.Mvc;
using imarket.service.IService;
using Microsoft.Extensions.Caching.Memory;
using imarket.models;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using imarket.plugin;

namespace imarket.Controllers.open
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService commentService;
        private readonly IUserService userService;
        private readonly IPostService postService;
        private readonly IMemoryCache cache;
        private readonly IConfiguration configuration;
        private readonly ILikeService likeService;
        private readonly ILogger<CommentsController> logger;
        private readonly PluginManager pluginManager;
        public CommentsController(IConfiguration configuration, ILikeService likeService, ICommentService commentService, IPostService postService, IUserService userService, IMemoryCache cache, ILogger<CommentsController> _logger,PluginManager pluginManager)
        {
            this.configuration = configuration;
            this.commentService = commentService;
            this.userService = userService;
            this.postService = postService;
            this.likeService = likeService;
            this.logger = _logger;
            this.cache = cache;
            this.pluginManager = pluginManager;
        }

        /// <summary>
        /// 获取帖子的评论
        /// </summary>
        /// <param name="postid"></param>
        /// <returns></returns>
        [HttpGet("{postid}")] // api/Comments/{postid}
        public async Task<IActionResult> GetCommentsByPostIdAsync([FromRoute][Required] ulong postid)
        {
            var args = new object[] { postid };
            var result_before = await pluginManager.ExecuteBeforeAsync("api/Comments/{postid}", args, User?.Identity?.Name);
            if (result_before != null)
            {
                return Ok(result_before);
            }
            if (!User.Identity!.IsAuthenticated)
            {
                cache.TryGetValue(postid, out var comments);
                if (comments != null)
                {
                    return Ok(new { success = true, comments });
                }
            }
            var Comments = await commentService.GetCommentsByPostIdAsync(postid);
            var result = new List<CommentResponse>();
            foreach (var comment in Comments)
            {
                var avatar = "/images/defaultAvatar.svg";
                var likeNum = 0;
                var isLike = false;
                try
                {
                    avatar = (await userService.GetUserByIdAsync(comment.UserId))?.Avatar;
                    likeNum = await likeService.GetCommentLikeNumsByCommentIdAsync(comment.Id);
                    if(User.Identity!.IsAuthenticated)
                    {
                        isLike = await likeService.CheckUserLikeCommentAsync((await userService.GetUserByUsernameAsync(User.Identity!.Name!)).Id, comment.Id);
                    }
                }
                catch
                {
                    avatar = "/images/defaultAvatar.svg";
                }
                result.Add(new CommentResponse
                {
                    Id = comment.Id,
                    Nickname = (await userService.GetUserByIdAsync(comment.UserId))?.Nickname,
                    Username = (await userService.GetUserByIdAsync(comment.UserId))?.Username,
                    UserAvatar = avatar,
                    Content = comment.Content,
                    isLike = isLike,
                    likeNum = likeNum,
                    CreatedAt = comment.CreatedAt
                });
            }
            if (!User.Identity.IsAuthenticated)
            {
                cache.Set(postid, result, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(int.Parse(configuration["Cache:SinglePost"]!))
                });
            }
            var response = new
            {
                success = true,
                comments = result
            };
            var result_after = await pluginManager.ExecuteAfterAsync("api/Comments/{postid}", response, User?.Identity?.Name);
            if (result_after != null)
            {
                return Ok(result_after);
            }
            return Ok(response);
        }

        /// <summary>
        /// 发表评论
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        [HttpPost("Create")] // api/Comments/Create
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> CreateCommentAsync([FromBody][Required] CommentPostRequest comment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var args = new object[] { comment };
            var result_before = await pluginManager.ExecuteBeforeAsync("api/Comments/Create", args, User?.Identity?.Name);
            if (result_before != null)
            {
                return Ok(result_before);
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
            cache.Remove(post.Id);
            var response = new
            {
                success = true,
                commentId = comment_new.Id
            };
            var result_after = await pluginManager.ExecuteAfterAsync("api/Comments/Create", response, User?.Identity?.Name);
            if (result_after != null)
            {
                return Ok(result_after);
            }
            return Ok(response);
        }

        /// <summary>
        /// 删除评论
        /// </summary>
        /// <param name="commentId"></param>
        /// <returns></returns>
        [HttpGet("Delete")] // api/Comments/Delete?commentId=xxx
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> DeleteCommentAsync([FromQuery][Required] ulong commentId)
        {
            var args = new object[] { commentId };
            var result_before = await pluginManager.ExecuteBeforeAsync("api/Comments/Delete", args, User?.Identity?.Name);
            if (result_before != null)
            {
                return Ok(result_before);
            }
            var user = await userService.GetUserByUsernameAsync(User.Identity!.Name!);
            if (user == null)
            {
                return Unauthorized("Invalid user.");
            }
            var comment = await commentService.GetCommentByIdAsync(commentId);
            if (comment == null)
            {
                return NotFound("Comment not found.");
            }
            if (comment.UserId != user.Id && User.IsInRole("admin") == false)
            {
                return Unauthorized("You are not the author of this comment.");
            }
            var result = await commentService.DeleteCommentAsync(commentId);
            if (result == 0)
            {
                return StatusCode(500);
            }
            var response = new
            {
                success = true
            };
            var result_after = await pluginManager.ExecuteAfterAsync("api/Comments/Delete", response, User?.Identity?.Name);
            if (result_after != null)
            {
                return Ok(result_after);
            }
            return Ok(response);
        }

        /// <summary>
        /// 点赞评论
        /// </summary>
        /// <param name="commentId"></param>
        /// <returns></returns>
        [HttpGet("Like")] // api/Comments/Like?commentId=xxx
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> LikeCommentAsync([FromQuery][Required] ulong commentId)
        {
            var args = new object[] { commentId };
            var result_before = await pluginManager.ExecuteBeforeAsync("api/Comments/Like", args, User?.Identity?.Name);
            if (result_before != null)
            {
                return Ok(result_before);
            }
            var user = await userService.GetUserByUsernameAsync(User.Identity!.Name!);
            if (user == null)
            {
                return Unauthorized("Invalid user.");
            }
            var comment = await commentService.GetCommentByIdAsync(commentId);
            if (comment == null)
            {
                return NotFound("Comment not found.");
            }
            var isLike = await likeService.CheckUserLikeCommentAsync(user.Id, commentId);
            if(isLike)
            {
                return Ok(new { success = true,message = "you have already liked it." });
            }
            var result = await likeService.CreateLikeAsync(new LikeModels
            {
                PostId = null,
                CommentId = commentId!,
                UserId = user.Id,
                CreatedAt = DateTime.Now
            });
            if (result == 0)
            {
                return StatusCode(500);
            }
            var response = new
            {
                success = true
            };
            var result_after = await pluginManager.ExecuteAfterAsync("api/Comments/Like", response, User?.Identity?.Name);
            if (result_after != null)
            {
                return Ok(result_after);
            }
            return Ok(response);
        }

        /// <summary>
        /// 取消点赞评论
        /// </summary>
        /// <param name="commentId"></param>
        /// <returns></returns>
        [HttpGet("UnLike")] // api/Comments/UnLike?commentId=xxx
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> UnLikeCommentAsync([FromQuery][Required] ulong commentId)
        {
            var args = new object[] { commentId };
            var result_before = await pluginManager.ExecuteBeforeAsync("api/Comments/UnLike", args, User?.Identity?.Name);
            if (result_before != null)
            {
                return Ok(result_before);
            }
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
                return Ok(new { success = true, message = "you have already unliked it." });
            }
            var result = await likeService.DeleteLikeAsync(new LikeModels
            {
                CommentId = commentId!,
                UserId = user.Id,
                CreatedAt = comment.CreatedAt,
                PostId = null
            });
            if (result == 0)
            {
                return StatusCode(500);
            }
            var response = new
            {
                success = true
            };
            var result_after = await pluginManager.ExecuteAfterAsync("api/Comments/UnLike", response, User?.Identity?.Name);
            return Ok(response);
        }
    }

    public class CommentPostRequest
    {
        [Required]
        public ulong PostId { get; set; }
        [Required]
        public string? Content { get; set; }
    }
    public class CommentResponse
    {
        public ulong Id { get; set; }
        public string? Nickname { get; set; }
        public string? Username { get; set; }
        public string? UserAvatar { get; set; }
        public string? Content { get; set; }

        public bool isLike { get; set; }
        public int? likeNum { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
