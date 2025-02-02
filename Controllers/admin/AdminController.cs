using imarket.models;
using imarket.plugin;
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
        private readonly IPostService postService;
        private readonly IPostCategoriesService postCategoriesService;
        private readonly IUserService userService;
        private readonly ILikeService likeService;
        private readonly IFavoriteService favoriteService;
        private readonly IConfiguration configuration;
        private readonly PluginManager pluginManager;
        
        public AdminController(IFavoriteService favoriteService, ILikeService likeService, IPostService postService, IPostCategoriesService postCategoriesService, IUserService userService,IConfiguration configuration,PluginManager pluginManager)
        {
            this.postService = postService;
            this.postCategoriesService = postCategoriesService;
            this.userService = userService;
            this.likeService = likeService;
            this.favoriteService = favoriteService;
            this.configuration = configuration;
            this.pluginManager = pluginManager;
        }

        /// <summary>
        /// 创建分类
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        [HttpGet("CreateCategories")] // api/Admin/CreateCategories?name=xxx&description=xxx
        public async Task<IActionResult> CreateCatogory([FromQuery][Required] string name, [FromQuery][Required] string description)
        {
            var args = new object[] { name, description };
            var result_before = await pluginManager.ExecuteBeforeAsync("api/Admin/CreateCategories", args,User?.Identity?.Name);
            if (result_before != null)
            {
                return Ok(result_before);
            }
            await postCategoriesService.CreateCategoryAsync(new CategoryModels
            {
                Name = name,
                Description = description
            });
            var result_after = await pluginManager.ExecuteAfterAsync("api/Admin/CreateCategories", true, User?.Identity?.Name);
            if (result_after != null)
            {
                return Ok(result_after);
            }
            return Ok(new { success = true });
        }

        /// <summary>
        /// 编辑分类
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        [HttpGet("EditCategories")] // api/Admin/EditCategories?id=xxx&name=xxx&description=xxx
        public async Task<IActionResult> EditCatogory([FromQuery][Required] ulong id, [FromQuery][Required] string name, [FromQuery][Required] string description)
        {
            var args = new object[] { id, name, description };
            var result_before = await pluginManager.ExecuteBeforeAsync("api/Admin/EditCategories", args, User?.Identity?.Name);
            if (result_before != null)
            {
                return Ok(result_before);
            }
            var category = await postCategoriesService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            category.Name = name;
            category.Description = description;
            await postCategoriesService.UpdateCategoryAsync(id, category);
            var result_after = await pluginManager.ExecuteAfterAsync("api/Admin/EditCategories", true, User?.Identity?.Name);
            if (result_after!=null)
            {
                return Ok(result_after);
            }
            return Ok(new { success = true });
        }

        /// <summary>
        /// 删除分类
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("DeleteCategories")] // api/Admin/DeleteCategories?id=xxx
        public async Task<IActionResult> DeleteCatogory([FromQuery][Required] ulong id)
        {
            var args = new object[] { id };
            var result_before = await pluginManager.ExecuteBeforeAsync("api/Admin/DeleteCategories", args, User?.Identity?.Name);
            if (result_before != null)
            {
                return Ok(result_before);
            }
            var category = await postCategoriesService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            var posts = await postService.GetPostsByCategoryIdAsync(id,1,1);
            if (posts.Count() != 0)
            {
                return BadRequest("some posts of the category have not been deleted!");
            }
            await postCategoriesService.DeleteCategoryAsync(id);
            var result_after = await pluginManager.ExecuteAfterAsync("api/Admin/DeleteCategories", true, User?.Identity?.Name);
            if (result_after != null)
            {
                return Ok(result_after);
            }
            return Ok(new { success = true });
        }

        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("ListUsers")] // api/Admin/ListUsers?page=1&size=10
        public async Task<IActionResult> GetUserList([FromQuery] int page=1, [FromQuery] int pageSize=10)
        {
            var args = new object[] { page, pageSize };
            var result_before = await pluginManager.ExecuteBeforeAsync("api/Admin/ListUsers", args, User?.Identity?.Name);
            if (result_before != null)
            {
                return Ok(result_before);
            }
            var users = await userService.GetAllUsers(page, pageSize);
            var result_after = await pluginManager.ExecuteAfterAsync("api/Admin/ListUsers", users, User?.Identity?.Name);
            if (result_after != null)
            {
                return Ok(result_after);
            }
            return Ok(new { success = true, users = users });
        }

        /// <summary>
        /// 封禁用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("BanUser")] // api/Admin/BanUser?id=xxx
        public async Task<IActionResult> BanUser([FromQuery] ulong id)
        {
            var args = new object[] { id };
            var result_before = await pluginManager.ExecuteBeforeAsync("api/Admin/BanUser", args, User?.Identity?.Name);
            if (result_before != null)
            {
                return Ok(result_before);
            }
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
            var result_after = await pluginManager.ExecuteAfterAsync("api/Admin/BanUser", true, User?.Identity?.Name);
            if (result_after != null)
            {
                return Ok(result_after);
            }
            return Ok(new { success = true });
        }

        /// <summary>
        /// 解封用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("UnbanUser")] // api/Admin/UnbanUser?id=xxx
        public async Task<IActionResult> UnbanUser([FromQuery][Required] ulong id)
        {
            var args = new object[] { id };
            var result_before = await pluginManager.ExecuteBeforeAsync("api/Admin/UnbanUser", args, User?.Identity?.Name);
            if (result_before != null)
            {
                return Ok(result_before);
            }
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
            var result_after = await pluginManager.ExecuteAfterAsync("api/Admin/UnbanUser", true, User?.Identity?.Name);
            if (result_after != null)
            {
                return Ok(result_after);
            }
            return Ok(new { success = true });
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost("CreateUser")] // api/Admin/CreateUser
        public async Task<IActionResult> CreateUser([FromBody][Required] UserCreateRequest user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var args = new object[] { user };
            var result_before = await pluginManager.ExecuteBeforeAsync("api/Admin/CreateUser", args, User?.Identity?.Name);
            if (result_before != null)
            {
                return Ok(result_before);
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
                Username = user.Username!,
                Nickname = user.Nickname!,
                PasswordHash = user.PasswordHash!,
                Avatar = user.Avatar ?? "/images/defaultAvatar.svg",
                Email = user.Email!,
                Role = user.Role!,
                CreatedAt = DateTime.Now,
                Status = user.Status
            });
            if (result == 0)
            {
                return StatusCode(500);
            }
            var result_after = await pluginManager.ExecuteAfterAsync("api/Admin/CreateUser", true, User?.Identity?.Name);
            if (result_after != null)
            {
                return Ok(result_after);
            }
            return Ok(new { success = true });
        }

        /// <summary>
        /// 编辑用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost("EditUser")] // api/Admin/EditUser
        public async Task<IActionResult> EditUser([FromBody][Required] UserEditRequest user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var args = new object[] { user };
            var result_before = await pluginManager.ExecuteBeforeAsync("api/Admin/EditUser", args, User?.Identity?.Name);
            if (result_before != null)
            {
                return Ok(result_before);
            }
            var userCheck = await userService.GetUserByIdAsync(user.Id);
            if (userCheck == null)
            {
                return NotFound();
            }
            if(userCheck.Username == configuration["admin:Username"] && user.Role != "admin")
            {
                return BadRequest("you can't change the super admin's role");
            }
            userCheck.Nickname = user.Nickname ?? userCheck.Nickname;
            userCheck.Avatar = user.Avatar ?? userCheck.Avatar;
            userCheck.Email = user.Email ?? userCheck.Email;
            userCheck.Role = user.Role ?? userCheck.Role;
            userCheck.Status = user.Status ?? userCheck.Status;
            await userService.UpdateUserAsync(user.Id!, userCheck);
            var result_after = await pluginManager.ExecuteAfterAsync("api/Admin/EditUser", true, User?.Identity?.Name);
            if (result_after != null)
            {
                return Ok(result_after);
            }
            return Ok(new { success = true });
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("DeleteUser")] // api/Admin/DeleteUser?userId=xxx
        public async Task<IActionResult> DeleteUser([FromQuery][Required] ulong userId)
        {
            var args = new object[] { userId };
            var result_before = await pluginManager.ExecuteBeforeAsync("api/Admin/DeleteUser", args, User?.Identity?.Name);
            if (result_before != null)
            {
                return Ok(result_before);
            }
            var user = await userService.GetUserByIdAsync(userId);
            if(user == null)
            {
                return NotFound();
            }
            if(user.Role == "admin" && (await userService.GetUserByUsernameAsync(User.Identity.Name)).Username != configuration["admin:Username"])
            {
                return BadRequest("you are not super admin.");
            }
            if(user.Username == User.Identity.Name)
            {
                return BadRequest("you can't delete yourself");
            }
            var posts = await postService.GetAllPostsByUserIdAsync(userId);
            if (posts.Count() != 0)
            {
                return BadRequest("some posts of the user have not been deleted!");
            }
            // 删除用户
            var result = await userService.DeleteUserAsync(userId);
            if (result == 0)
            {
                return StatusCode(500);
            }
            var result_after = await pluginManager.ExecuteAfterAsync("api/Admin/DeleteUser", true, User?.Identity?.Name);
            if (result_after != null)
            {
                return Ok(result_after);
            }
            return Ok(new { success = true });
        }

        /// <summary>
        /// 删除用户的所有帖子
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("DeletePosts")] // api/Admin/DeletePosts?userId=xxx
        public async Task<IActionResult> DeletePosts([FromQuery][Required] ulong userId)
        {
            var args = new object[] { userId };
            var result_before = await pluginManager.ExecuteBeforeAsync("api/Admin/DeletePosts", args, User?.Identity?.Name);
            if (result_before != null)
            {
                return Ok(result_before);
            }
            var posts = await postService.GetAllPostsByUserIdAsync(userId);
            if (posts != null)
            {
                foreach (var post in posts)
                {
                    await postService.DeletePostAsync(post.Id);
                }
            }
            var result_after = await pluginManager.ExecuteAfterAsync("api/Admin/DeletePosts", true, User?.Identity?.Name);
            if (result_after != null)
            {
                return Ok(result_after);
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
        public UserCreateRequest()
        {
            Role = "user";
        }
    }

    public class UserEditRequest
    {
        [Required]
        public ulong Id { get; set; }
        public string? Nickname { get; set; }
        public string? Avatar { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public int? Status { get; set; }
    }
}
