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

        public async Task<IEnumerable<HotRankingModels.Favorite>> GetHotRankingAsync()
        {
            var posts = new List<HotRankingModels.Favorite>();
            // 查询本周收藏数最多的10条帖子
            var query = @"
                SELECT 
                    p.Id, 
                    p.Title, 
                    p.Content, 
                    p.UserId, 
                    p.CreatedAt, 
                    COUNT(f.Id) AS FavoriteCount
                FROM 
                    Favorites f
                INNER JOIN 
                    Posts p ON f.PostId = p.Id
                WHERE 
                    f.CreatedAt >= DATE_SUB(NOW(), INTERVAL 1 WEEK)
                GROUP BY 
                    p.Id
                ORDER BY 
                    FavoriteCount DESC
                LIMIT 10";

            var result = await _database.ExecuteQuery(query, CommandType.Text);
            foreach (DataRow row in result.Rows)
            {
                posts.Add(new HotRankingModels.Favorite
                {
                    Id = row["Id"].ToString()!,
                    Title = row["Title"].ToString()!,
                    Content = row["Content"].ToString()!,
                    UserId = row["UserId"].ToString()!,
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                    FavoriteCount = Convert.ToInt32(row["FavoriteCount"]!)
                });
            }

            return posts;
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

        public async Task<int> DeletePostFavoriteByUserIdAsyc(string userId)
        {
            var query = "DELETE FROM Favorites WHERE UserId = @UserId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@UserId", userId),
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> DeletePostFavoriteByPostIdAsyc(string postId)
        {
            var query = "DELETE FROM Favorites WHERE PostId = @PostId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@PostId", postId),
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> GetFavoriteNumsByPostIdAsync(string postId)
        {
            var query = "SELECT COUNT(*) FROM Favorites WHERE PostId = @PostId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@PostId", postId),
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
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
            if (result.Rows.Count > 0)
            {
                return true;
            }
            return false;
        }
    }
}
