using imarket.models;
using imarket.utils;
using Microsoft.Data.SqlClient;
using System.Data;
using imarket.service.IService;

namespace imarket.service.Service
{
    public class LoginService:ILoginService
    {
        public async Task<UserModels> LoginAsync(string username, string password)
        {
            var db = Database.getInstance();
            var query = "SELECT * FROM Users WHERE Username = @Username AND PasswordHash = @PasswordHash";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Username", SqlDbType.NVarChar) { Value = username },
                new SqlParameter("@PasswordHash", SqlDbType.NVarChar) { Value = password },
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            if (result.Rows.Count == 0)
            {
                return null;
            }
            var row = result.Rows[0];
            return new UserModels
            {
                Id = row["Id"].ToString()!,
                Username = row["Username"].ToString()!,
                Nickname = row["Nickname"].ToString()!,
                PasswordHash = row["PasswordHash"].ToString()!,
                Avatar = row["Avatar"].ToString()!,
                Email = row["Email"].ToString()!,
                Role = row["Role"].ToString()!,
                CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
            };
        }
        public async Task<UserModels> LogoutAsync()
        {
            return null;
        }
        public async Task<UserModels> RegisterAsync(UserModels user)
        {
            var db = Database.getInstance();
            var query = "INSERT INTO Users (Id, Username, Nickname, PasswordHash, Avatar, Email, Role, CreatedAt, Status) VALUES (@Id, @Username, @Nickname, @PasswordHash, @Avatar, @Email, @Role, @CreatedAt, @Status)";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", SqlDbType.Char) { Value = user.Id },
                new SqlParameter("@Username", SqlDbType.NVarChar) { Value = user.Username },
                new SqlParameter("@Nickname", SqlDbType.NVarChar) { Value = user.Nickname },
                new SqlParameter("@PasswordHash", SqlDbType.NVarChar) { Value = user.PasswordHash },
                new SqlParameter("@Avatar", SqlDbType.NVarChar) { Value = user.Avatar },
                new SqlParameter("@Email", SqlDbType.NVarChar) { Value = user.Email },
                new SqlParameter("@Role", SqlDbType.NVarChar) { Value = user.Role },
                new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = user.CreatedAt },
                new SqlParameter("@Status", SqlDbType.Int) { Value = user.Status },
            };
            await db.ExecuteNonQuery(query, CommandType.Text, parameters);
            return user;
        }
    }
}
