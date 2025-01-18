using imarket.service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using imarket.models;
using Microsoft.AspNetCore.Authorization;

namespace imarket.Controllers.admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class PostsController : ControllerBase
    {
        public readonly IPostService postService;
        public readonly IPostCategoriesService postCategoriesService;
        public PostsController(IPostService postService, IPostCategoriesService postCategoriesService)
        {
            this.postService = postService;
            this.postCategoriesService = postCategoriesService;
        }
        [HttpGet("createCategories")] // api/admin/Posts/createCategories?name=xxx&description=xxx
        public async Task<IActionResult> CreateCatogory([FromQuery] string name, [FromQuery] string description)
        {
            await postCategoriesService.CreateCategoryAsync(new CategoryModels
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Description = description
            });
            return Ok(new { success = true });
        }
    }
}
