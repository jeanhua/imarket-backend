using imarket.service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public PostController(IUserService userService, IPostService postService,IPostCategoriesService postCategoriesService)
        {
            this.userService = userService;
            this.postService = postService;
            this.postCategoriesService = postCategoriesService;
        }

        [HttpGet("Posts")] // api/post/Posts
        public async Task<IActionResult> GetPosts([FromHeader]int page, [FromHeader]int pageSize)
        {
            try
            {
                var posts = await postService.GetAllPostsAsync(page,pageSize);
                return Ok(new { success = true, posts = posts });
            }
            catch (Exception e)
            {
                System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString() + "\t" + e.ToString() + "\n");
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet("CategorisedPosts")] // api/post/CategorisedPosts
        public async Task<IActionResult> GetCategorisedPosts([FromHeader] int page, [FromHeader] int pageSize, [FromHeader] int categoryId)
        {
            try
            {
                var posts = await postService.GetPostsByCategoryIdAsync(categoryId,page, pageSize);
                return Ok(new { success = true, posts = posts });
            }
            catch (Exception e)
            {
                System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString() + "\t" + e.ToString() + "\n");
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet("Post")] // api/post/Post
        public async Task<IActionResult> GetPost([FromHeader] int id)
        {
            try
            {
                var postfind = await postService.GetPostByIdAsync(id);
                if (postfind == null)
                {
                    return NotFound();
                }
                var categoryID = await postCategoriesService.GetPostCategoriesByPostIdAsync(postfind.Id);
                var user = await userService.GetUserByIdAsync(postfind.UserId);
                return Ok(new { 
                    success = true, 
                    post = new { 
                        Id = postfind.Id,
                        Title = postfind.Title,
                        Content = postfind.Content,
                        Status = postfind.Status,
                        CategoryId = categoryID,
                        CreateTime = postfind.CreatedAt,
                        Nickname = user.Nickname,
                        Avatar = user.Avatar
                    },
                    comments = new
                    {

                    }
                });
            }
            catch (Exception e)
            {
                System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString() + "\t" + e.ToString() + "\n");
                return StatusCode(500, e.Message);
            }
        }
    }
}
