using imarket.lib;
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
        [Route("info")]
        [HttpGet]
        IActionResult getinfo()
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
            return Ok(user.Rows[0]);
        }
    }
}
