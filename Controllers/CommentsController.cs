using Microsoft.AspNetCore.Mvc;
using imarket.service.IService;
using Microsoft.Extensions.Caching.Memory;
using imarket.models;
using Microsoft.AspNetCore.Authorization;

namespace imarket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService commentService;
        private readonly IUserService userService;
        private readonly IPostService postService;
        private readonly IMemoryCache _cache;
        private readonly ILikeService likeService;
        private readonly ILogger<CommentsController> _logger;
        public CommentsController(ILikeService likeService, ICommentService commentService,IPostService postService ,IUserService userService, IMemoryCache cache, ILogger<CommentsController> _logger)
        {
            this.commentService = commentService;
            this.userService = userService;
            this.postService = postService;
            this.likeService = likeService;
            this._logger = _logger;
            _cache = cache;
        }
        [HttpGet("{postid}")] // api/Comments/{postid}
        public async Task<IActionResult> GetCommentsByPostIdAsync([FromRoute]string postid)
        {
            try
            {
                _cache.TryGetValue(postid, out var comments);
                if (comments != null)
                {
                    return Ok(new { success = true, comments = comments });
                }
                var Comments = await commentService.GetCommentsByPostIdAsync(postid);
                var response = new List<CommentResponse>();
                foreach (var comment in Comments)
                {
                    var avatar = "/images/defaultAvatar.jpg";
                    var likeNum = 0;
                    try
                    {
                        avatar = (await userService.GetUserByIdAsync(comment.UserId))!.Avatar;
                        likeNum = await likeService.GetCommentLikeNumsByCommentIdAsync(comment.Id);
                    }
                    catch
                    {
                        avatar = "/images/defaultAvatar.jpg";
                    }
                    response.Add(new CommentResponse
                    {
                        Id = comment.Id,
                        UserId = comment.UserId,
                        UserAvatar = avatar,
                        Content = comment.Content,
                        likeNum = likeNum,
                        CreatedAt = comment.CreatedAt
                    });
                }
                _cache.Set(postid, response, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
                return Ok(new { success = true, comments = response });
            }
            catch (Exception e)
            {
                _logger.LogError("/api/Comments/{postid}: " + e.ToString());
                System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString() + "\t" + e.ToString() + "\n");
                return StatusCode(500, "Internal Server Error");
            }
        }
        [HttpPost("Create")] // api/Comments/Create
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> CreateCommentAsync([FromBody] CommentPostRequest comment)
        {
            try
            {

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
                if(comment.Content == null)
                {
                    return BadRequest("Content is required.");
                }
                if (comment.Content.Length == 0)
                {
                    return BadRequest("Content is required.");
                }
                if (post == null) {
                    return NotFound("Post not found.");
                }
                if (post.Status == 1)
                {
                    return BadRequest("Post is finished");
                }
                var result = await commentService.CreateCommentAsync(
                    new CommentModels
                    {
                        Id = Guid.NewGuid().ToString(),
                        PostId = comment.PostId!,
                        Content = comment.Content,
                        UserId = user.Id,
                        CreatedAt = DateTime.Now
                    }
                    );
                if (result == 0)
                {
                    return StatusCode(500);
                }
                return Ok(new { success = true, commentId = result });
            }
            catch (Exception e)
            {
                _logger.LogError("/api/Comments/Create: " + e.ToString());
                System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString() + "\t" + e.ToString() + "\n");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }

    public class CommentPostRequest
    {
        public string? PostId { get; set; }
        public string? Content { get; set; }
    }
    public class CommentResponse
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string? UserAvatar { get; set; }
        public string? Content { get; set; }
        public int? likeNum { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
