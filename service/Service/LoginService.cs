using imarket.models;
using imarket.utils;
using System.Data;
using imarket.service.IService;
using MySql.Data.MySqlClient;

namespace imarket.service.Service
{
    public class LoginService:ILoginService
    {
        private readonly Database _database;
        public LoginService(Database database)
        {
            _database = database;
        }
        public async Task<UserModels?> LoginAsync(string username, string password)
        {
            var query = "SELECT * FROM Users WHERE Username = @Username AND PasswordHash = @PasswordHash";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Username", username),
                new MySqlParameter("@PasswordHash", password),
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            if (result.Rows.Count == 0)
            {
                return null;
            }
            var row = result.Rows[0];
            return new UserModels
            {
                Id = ulong.Parse(row["Id"].ToString()!),
                Username = row["Username"].ToString()!,
                Nickname = row["Nickname"].ToString()!,
                PasswordHash = row["PasswordHash"].ToString()!,
                Avatar = row["Avatar"].ToString()!,
                Email = row["Email"].ToString()!,
                Role = row["Role"].ToString()!,
                CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
            };
        }
        public async Task<UserModels?> LogoutAsync()
        {
            return null;
        }
        public async Task<int> RegisterAsync(UserModels user)
        {
            var query = "INSERT INTO Users (Username, Nickname, PasswordHash, Avatar, Email, Role, CreatedAt, Status) VALUES (@Username, @Nickname, @PasswordHash, @Avatar, @Email, @Role, @CreatedAt, @Status)";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Username", user.Username),
                new MySqlParameter("@Nickname", user.Nickname),
                new MySqlParameter("@PasswordHash", user.PasswordHash),
                new MySqlParameter("@Avatar", user.Avatar),
                new MySqlParameter("@Email", user.Email),
                new MySqlParameter("@Role", user.Role),
                new MySqlParameter("@CreatedAt", user.CreatedAt),
                new MySqlParameter("@Status", user.Status),
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
    }
}
