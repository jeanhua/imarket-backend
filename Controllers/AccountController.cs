using imarket.utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace imarket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        [HttpGet("info")]
        public IActionResult getinfo()
        {
            var username = User.Identity?.Name;
            if (username == null)
            {
                return Unauthorized();
            }
            // 查询用户信息
            var query = "SELECT * FROM Users WHERE Username = @Username";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Username", SqlDbType.NVarChar) { Value = username }
            };
            var user = Database.getInstance().ExecuteQuery(query, CommandType.Text, parameters);
            if (user.Rows.Count == 0)
            {
                return NotFound();
            }
            DataRow row = user.Rows[0];
            return Ok(new
            {
                Username = row["Username"].ToString(),
                Nickname = row["Nickname"].ToString(),
                Avatar = row["Avatar"].ToString(),
                Email = row["Email"].ToString()
            });
        }

        [HttpPost("edit")]
        public IActionResult edit([FromBody] Usermodel user)
        {
            var username = User.Identity?.Name;
            if (username == null)
            {
                return Unauthorized();
            }
            // 更新用户信息
            var query = "UPDATE Users SET Nickname = @Nickname, Email = @Email, Avatar = @Avatar WHERE Username = @Username";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Username", SqlDbType.NVarChar) { Value = username },
                new SqlParameter("@Nickname", SqlDbType.NVarChar) { Value = user.Nickname },
                new SqlParameter("@Email", SqlDbType.NVarChar) { Value = user.Email },
                new SqlParameter("@Avatar", SqlDbType.NVarChar) { Value = user.Avatar }
            };
            int rowsAffected = Database.getInstance().ExecuteNonQuery(query, CommandType.Text, parameters);
            if (rowsAffected == 0)
            {
                return NotFound();
            }
            return Ok();
        }

        public class Usermodel
        {
            public string Nickname { get; set; }
            public string Email { get; set; }
            public string Avatar { get; set; }
        }
    }
}
