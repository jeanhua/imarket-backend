using imarket.models;
using imarket.service.IService;
using imarket.utils;
using MySql.Data.MySqlClient;
using System.Data;
namespace imarket.service.Service
{
    public class UserService:IUserService
    {
        private readonly Database _database;
        public UserService(Database database)
        {
            _database = database;
        }
        public async Task<int> GetUserNums()
        {
            var query = "SELECT COUNT(*) FROM Users";
            var result = await _database.ExecuteQuery(query, CommandType.Text);
            return Convert.ToInt32(result.Rows[0][0]!);
        }
        public async Task<UserModels?> GetUserByUsernameAsync(string? username)
        {
            if (username == null)
            {
                return null;
            }
            var query = "SELECT * FROM Users WHERE Username = @Username";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Username", SqlDbType.NVarChar) { Value = username }
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
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
                Status = Convert.ToInt32(row["Status"]!),
            };
        }
        public async Task<UserModels> GetUserByEmailAsync(string email)
        {
            var query = "SELECT * FROM Users WHERE Email = @Email";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Email", SqlDbType.NVarChar) { Value = email }
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
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
                Status = Convert.ToInt32(row["Status"]!),
            };
        }
        public async Task<IEnumerable<UserModels>> GetAllUsers(int page, int pageSize)
        {
            if (page < 1)
            {
                page = 1;
            }
            if (pageSize < 1 || pageSize > 20)
            {
                pageSize = 10;
            }
            var users = new List<UserModels>();
            var query = "SELECT * FROM Users ORDER BY CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Offset", SqlDbType.Int) { Value = (page - 1) * pageSize },
                new MySqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize },
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                users.Add(new UserModels
                {
                    Id = row["Id"].ToString()!,
                    Username = row["Username"].ToString()!,
                    Nickname = row["Nickname"].ToString()!,
                    PasswordHash = row["PasswordHash"].ToString()!,
                    Avatar = row["Avatar"].ToString()!,
                    Email = row["Email"].ToString()!,
                    Role = row["Role"].ToString()!,
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                    Status = Convert.ToInt32(row["Status"]!),
                });
            }
            return users;
        }

        public async Task<UserModels?> GetUserByIdAsync(string id)
        {
            var query = "SELECT * FROM Users WHERE Id = @Id";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", SqlDbType.Char) { Value = id }
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
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
                Status = Convert.ToInt32(row["Status"]!),
            };
        }
        public async Task<int> CreateUserAsync(UserModels user)
        {
            var query = "INSERT INTO Users (Id, Username, Nickname, PasswordHash, Avatar, Email, Role, CreatedAt, Status) VALUES (@Id, @Username, @Nickname, @PasswordHash, @Avatar, @Email, @Role, @CreatedAt, @Status)";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", SqlDbType.Char) { Value = user.Id },
                new MySqlParameter("@Username", SqlDbType.NVarChar) { Value = user.Username },
                new MySqlParameter("@Nickname", SqlDbType.NVarChar) { Value = user.Nickname },
                new MySqlParameter("@PasswordHash", SqlDbType.NVarChar) { Value = user.PasswordHash },
                new MySqlParameter("@Avatar", SqlDbType.NVarChar) { Value = user.Avatar },
                new MySqlParameter("@Email", SqlDbType.NVarChar) { Value = user.Email },
                new MySqlParameter("@Role", SqlDbType.NVarChar) { Value = user.Role },
                new MySqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = user.CreatedAt },
                new MySqlParameter("@Status", SqlDbType.Int) { Value = user.Status },
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> DeleteUserAsync(string id)
        {
            var query = "DELETE FROM Users WHERE Id = @Id";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", SqlDbType.Char) { Value = id }
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> UpdateUserAsync(string userId, UserModels user)
        {
            var query = "UPDATE Users SET Username = @Username, Nickname = @Nickname, PasswordHash = @PasswordHash, Avatar = @Avatar, Email = @Email, Role = @Role, CreatedAt = @CreatedAt, Status = @Status WHERE Id = @Id";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", SqlDbType.Char) { Value = userId },
                new MySqlParameter("@Username", SqlDbType.NVarChar) { Value = user.Username },
                new MySqlParameter("@Nickname", SqlDbType.NVarChar) { Value = user.Nickname },
                new MySqlParameter("@PasswordHash", SqlDbType.NVarChar) { Value = user.PasswordHash },
                new MySqlParameter("@Avatar", SqlDbType.NVarChar) { Value = user.Avatar },
                new MySqlParameter("@Email", SqlDbType.NVarChar) { Value = user.Email },
                new MySqlParameter("@Role", SqlDbType.NVarChar) { Value = user.Role },
                new MySqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = user.CreatedAt },
                new MySqlParameter("@Status", SqlDbType.Int) { Value = user.Status },
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
    }
}
