using imarket.models;
using imarket.service.IService;
using imarket.utils;
using MySql.Data.MySqlClient;
using System.Data;
namespace imarket.service.Service
{
    public class FavoriteService : IFavoriteService
    {
        private readonly Database _database;
        public FavoriteService(Database database)
        {
            _database = database;
        }
        public async Task<IEnumerable<FavoriteModels>> GetPostFavoriteByUserId(string userId, int page, int pagesize)
        {
            var favorites = new List<FavoriteModels>();
            int offset = (page - 1) * pagesize;
            string query = @"
                SELECT * FROM Favorites 
                WHERE UserId = @UserId 
                ORDER BY CreatedAt DESC 
                LIMIT @PageSize OFFSET @Offset;
            ";
            var parameters = new[]
            {
                new MySqlParameter("@UserId", userId),
                new MySqlParameter("@Offset", offset),
                new MySqlParameter("@PageSize", pagesize)
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                favorites.Add(new FavoriteModels
                {
                    Id = row["Id"].ToString()!,
                    UserId = row["UserId"].ToString()!,
                    PostId = row["PostId"].ToString()!,
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                });
            }
            return favorites;
        }
        public async Task<int> CreatePostFavoriteAsync(string postId, string userId)
        {
            // 检查是否已经收藏
            var query = "SELECT * FROM Favorites WHERE UserId = @UserId AND PostId = @PostId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@UserId", userId),
                new MySqlParameter("@PostId", postId),
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            if (result.Rows.Count > 0)
            {
                return 0;
            }
            // 创建收藏
            query = "INSERT INTO Favorites (Id, UserId, PostId, CreatedAt) VALUES (@Id, @UserId, @PostId, @CreatedAt)";
            parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", Guid.NewGuid().ToString()),
                new MySqlParameter("@UserId", userId),
                new MySqlParameter("@PostId", postId),
                new MySqlParameter("@CreatedAt", DateTime.Now),
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> DeletePostFavoriteAsync(string postId, string userId)
        {
            var query = "DELETE FROM Favorites WHERE UserId = @UserId AND PostId = @PostId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@UserId", userId),
                new MySqlParameter("@PostId", postId),
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> GetFavoriteNumsByPostId(string postId)
        {
            var query = "SELECT COUNT(*) FROM Favorites WHERE PostId = @PostId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@PostId", postId),
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            if (result.Rows.Count == 0 || result == null)
            {
                return 0;
            }
            return Convert.ToInt32(result.Rows[0][0]);
        }

        public async Task<bool> CheckIsFavorite(string userId, string postId)
        {
            var query = "SELECT * FROM Favorites WHERE UserId = @UserId AND PostId = @PostId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@UserId",userId),
                new MySqlParameter("@PostId",postId)
            };
            var result = await _database.ExecuteQuery(query,CommandType.Text, parameters);
            if (Convert.ToInt32(result.Rows[0][0]) == 0)
            {
                return false;
            }
            return true;
        }
    }
}
