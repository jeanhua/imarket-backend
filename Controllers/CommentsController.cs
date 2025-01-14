using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using imarket.service.IService;
using Microsoft.Extensions.Caching.Memory;
using imarket.models;

namespace imarket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService commentService;
        private readonly IUserService userService;
        private readonly IMemoryCache _cache;
        public CommentsController(ICommentService commentService, IUserService userService, IMemoryCache cache)
        {
            this.commentService = commentService;
            this.userService = userService;
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
    }
}
