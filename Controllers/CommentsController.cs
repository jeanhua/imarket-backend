using Microsoft.AspNetCore.Http;
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
        public CommentsController(ICommentService commentService,IPostService postService ,IUserService userService, IMemoryCache cache)
        {
            this.commentService = commentService;
            this.userService = userService;
            this.postService = postService;
            _cache = cache;
        }
        [HttpGet("{postid}")] // api/Comments/{postid}
        public async Task<IActionResult> GetCommentsByPostIdAsync([FromRoute]string postid)
        {
            try
            {
                IEnumerable<CommentModels> comments;
                if (_cache.TryGetValue($"Comments_cache{postid}", out var comment_cache))
                {
                    comments = comment_cache as IEnumerable<CommentModels>;
                    return Ok(new { success = true, comments });
                }
                comments = await commentService.GetCommentsByPostIdAsync(postid);
                _cache.Set($"Comments_cache{postid}", comments, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
                });
                return Ok(new { success = true, comments});
            }
            catch (Exception e)
            {
                Console.WriteLine("/api/Comments/{postid}: " + e.ToString());
                System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString() + "\t" + e.ToString() + "\n");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("Create")] // api/Comments/Create
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> CreateCommentAsync([FromBody] CommentPostRequest comment)
        {
            try
            {

                var user = await userService.GetUserByUsernameAsync(User.Identity!.Name!);
                var post = await postService.GetPostByIdAsync(comment.PostId);
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
                        PostId = comment.PostId,
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
                Console.WriteLine("/api/Comments/Create: " + e.ToString());
                System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString() + "\t" + e.ToString() + "\n");
                return StatusCode(500, e.Message);
            }
        }
    }

    public class CommentPostRequest
    {
        public string PostId { get; set; }
        public string Content { get; set; }
    }
}
