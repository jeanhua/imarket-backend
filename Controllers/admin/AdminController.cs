using imarket.models;
using imarket.service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace imarket.Controllers.admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        public readonly IPostService postService;
        public readonly IPostCategoriesService postCategoriesService;
        public readonly IUserService userService;
        public AdminController(IPostService postService, IPostCategoriesService postCategoriesService, IUserService userService)
        {
            this.postService = postService;
            this.postCategoriesService = postCategoriesService;
            this.userService = userService;
        }

        [HttpGet("createCategories")] // api/admin/createCategories?name=xxx&description=xxx
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

        [HttpGet("editCategories")] // api/admin/editCategories?id=xxx&name=xxx&description=xxx
        public async Task<IActionResult> EditCatogory([FromQuery] string id, [FromQuery] string name, [FromQuery] string description)
        {
            var category = await postCategoriesService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            category.Name = name;
            category.Description = description;
            await postCategoriesService.UpdateCategoryAsync(id, category);
            return Ok(new { success = true });
        }

        [HttpGet("deleteCategories")] // api/admin/deleteCategories?id=xxx
        public async Task<IActionResult> DeleteCatogory([FromQuery] string id)
        {
            var category = await postCategoriesService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            var posts = await postService.GetPostsByCategoryIdAsync(id,1,1);
            if (posts != null)
            {
                return BadRequest("some posts of the category have not been deleted!");
            }
            await postCategoriesService.DeleteCategoryAsync(id);
            return Ok(new { success = true });
        }

        [HttpGet("listUsers")] // api/admin/listUsers?page=1&size=10
        public async Task<IActionResult> GetUserList([FromQuery] int page, [FromQuery] int size)
        {
            var users = await userService.GetAllUsers(page, size);
            return Ok(new { success = true, users = users });
        }
        [HttpGet("banUser")] // api/admin/banUser?id=xxx
        public async Task<IActionResult> BanUser([FromQuery] string id)
        {
            var user = await userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            user.Status = 2;
            var result = await userService.UpdateUserAsync(id, user);
            if (result == 0)
            {
                return StatusCode(500);
            }
            return Ok(new { success = true });
        }
        [HttpGet("unbanUser")] // api/admin/unbanUser?id=xxx
        public async Task<IActionResult> UnbanUser([FromQuery] string id)
        {
            var user = await userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            user.Status = 1;
            var result = await userService.UpdateUserAsync(id, user);
            if (result == 0)
            {
                return StatusCode(500);
            }
            return Ok(new { success = true });
        }
        [HttpPost("createUser")] // api/admin/createUser
        public async Task<IActionResult> CreateUser([FromBody] UserCreateRequest user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userCheck = await userService.GetUserByUsernameAsync(user.Username!);
            if (userCheck != null)
            {
                return BadRequest("Username already exists.");
            }
            userCheck = await userService.GetUserByEmailAsync(user.Email!);
            if (userCheck != null)
            {
                return BadRequest("Email already exists.");
            }
            var result = await userService.CreateUserAsync(new UserModels
            {
                Id = Guid.NewGuid().ToString(),
                Username = user.Username!,
                Nickname = user.Nickname!,
                PasswordHash = user.PasswordHash!,
                Avatar = user.Avatar ?? "/images/defaultAvatar.png",
                Email = user.Email!,
                Role = user.Role!,
                CreatedAt = DateTime.Now,
                Status = user.Status
            });
            if (result == 0)
            {
                return StatusCode(500);
            }
            return Ok(new { success = true });
        }
        [HttpPost("editUser")] // api/admin/editUser
        public async Task<IActionResult> EditUser([FromBody] UserEditRequest user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userCheck = await userService.GetUserByIdAsync(user.Id!);
            if (userCheck == null)
            {
                return NotFound();
            }
            userCheck.Nickname = user.Nickname ?? userCheck.Nickname;
            userCheck.Avatar = user.Avatar ?? userCheck.Avatar;
            userCheck.Email = user.Email ?? userCheck.Email;
            userCheck.Role = user.Role ?? userCheck.Role;
            userCheck.Status = user.Status ?? userCheck.Status;
            await userService.UpdateUserAsync(user.Id!, userCheck);
            return Ok(new { success = true });
        }
        [HttpGet("deleteUser")] // api/admin/deleteUser?userId=xxx
        public async Task<IActionResult> DeleteUser([FromQuery] string userId)
        {
            var posts = await postService.GetPostsByUserIdAsync(userId);
            if (posts != null)
            {
                return BadRequest("some posts of the user have not been deleted!");
            }
            var result = await userService.DeleteUserAsync(userId);
            if (result == 0)
            {
                return StatusCode(500);
            }
            return Ok(new { success = true });
        }

        [HttpGet("deletePosts")] // api/admin/deletePosts?userId=xxx
        public async Task<IActionResult> DeletePosts([FromQuery] string userId)
        {
            var posts = await postService.GetPostsByUserIdAsync(userId);
            if (posts != null)
            {
                foreach (var post in posts)
                {
                    await postService.DeletePostAsync(post.Id);
                }
            }
            return Ok(new { success = true });
        }
    }
    public class UserCreateRequest
    {
        [Required]
        public string? Username { get; set; }
        [Required]
        public string? Nickname { get; set; }
        [Required]
        public string? PasswordHash { get; set; }
        public string? Avatar { get; set; }
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? Role { get; set; }
        public int Status { get; set; }
    }

    public class UserEditRequest
    {
        [Required]
        public string? Id { get; set; }
        public string? Nickname { get; set; }
        public string? Avatar { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public int? Status { get; set; }
    }
}
