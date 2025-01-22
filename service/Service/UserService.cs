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
        private readonly IPostService _postService;
        private readonly IMessageService _messageService;
        private readonly ILikeService _likeService;
        private readonly IFavoriteService _favoriteService;
        private readonly ICommentService _commentService;
        public UserService(Database database, IPostService postService,IMessageService _messageService, ILikeService likeService, IFavoriteService favoriteService,ICommentService commentService)
        {
            _database = database;
            _postService = postService;
            this._messageService = _messageService;
            _likeService = likeService;
            _favoriteService = favoriteService;
            _commentService = commentService;
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
                new MySqlParameter("@Username", username )
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
        public async Task<UserModels?> GetUserByEmailAsync(string email)
        {
            var query = "SELECT * FROM Users WHERE Email = @Email";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Email",email )
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
            var query = "SELECT * FROM Users ORDER BY CreatedAt DESC LIMIT @PageSize OFFSET @Offset";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Offset", (page - 1) * pageSize),
                new MySqlParameter("@PageSize", pageSize),
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
                new MySqlParameter("@Id",id )
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
                new MySqlParameter("@Id", user.Id),
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

        public async Task<int> DeleteUserAsync(string id)
        {
            await _postService.DeletePostByUserIdAsync(id);
            await _messageService.DeleteMessageByReceiverIdAsync(id);
            await _messageService.DeleteMessageBySenderIdAsync(id);
            await _likeService.DeleteLikesByUserIdAsync(id);
            await _favoriteService.DeletePostFavoriteByUserIdAsyc(id);
            await _commentService.DeleteCommentsByUserIdAsync(id);
            var query = "DELETE FROM Users WHERE Id = @Id";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id",id )
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> UpdateUserAsync(string userId, UserModels user)
        {
            var query = "UPDATE Users SET Username = @Username, Nickname = @Nickname, PasswordHash = @PasswordHash, Avatar = @Avatar, Email = @Email, Role = @Role, CreatedAt = @CreatedAt, Status = @Status WHERE Id = @Id";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", userId),
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
