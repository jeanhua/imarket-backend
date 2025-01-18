using imarket.service.IService;
using imarket.utils;
using MySql.Data.MySqlClient;
using System.Data;
namespace imarket.service.Service
{
    public class FavoriteService:IFavoriteService
    {
        private readonly Database _database;
        public FavoriteService(Database database)
        {
            _database = database;
        }
        public async Task<IEnumerable<int>> GetPostFavoriteByUserId(string userId, int page, int pagesize)
        {
            var favorites = new List<int>();
            var query = "SELECT * FROM Favorites WHERE UserId = @UserId ORDER BY CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@UserId", SqlDbType.Char) { Value = userId },
                new MySqlParameter("@Offset", SqlDbType.Int) { Value = (page - 1) * pagesize },
                new MySqlParameter("@PageSize", SqlDbType.Int) { Value = pagesize },
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                favorites.Add(Convert.ToInt32(row["PostId"]!));
            }
            return favorites;
        }
        public async Task<int> CreatePostFavoriteAsync(string userId, string postId)
        {
            // 检查是否已经收藏
            var query = "SELECT * FROM Favorites WHERE UserId = @UserId AND PostId = @PostId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@UserId", SqlDbType.Char) { Value = userId },
                new MySqlParameter("@PostId", SqlDbType.Char) { Value = postId },
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            if (result.Rows.Count > 0)
            {
                return 0;
            }
            // 添加收藏记录
            query = "INSERT INTO Favorites (Id, UserId, PostId, CreatedAt) VALUES (@Id, @UserId, @PostId, @CreatedAt)";
            parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", SqlDbType.Char) { Value = Guid.NewGuid().ToString() },
                new MySqlParameter("@UserId", SqlDbType.Char) { Value = userId },
                new MySqlParameter("@PostId", SqlDbType.Char) { Value = postId },
                new MySqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = DateTime.Now },
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
    }
}
