using imarket.service.IService;
using imarket.utils;
using Microsoft.Data.SqlClient;
using System.Data;
namespace imarket.service.Service
{
    public class FavoriteService:IFavoriteService
    {
        public async Task<IEnumerable<int>> GetPostFavoriteByUserId(int userId, int page, int pagesize)
        {
            var favorites = new List<int>();
            var db = Database.getInstance();
            var query = "SELECT * FROM Favorites WHERE UserId = @UserId ORDER BY CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@Offset", SqlDbType.Int) { Value = (page - 1) * pagesize },
                new SqlParameter("@PageSize", SqlDbType.Int) { Value = pagesize },
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                favorites.Add(Convert.ToInt32(row["PostId"]!));
            }
            return favorites;
        }
        public async Task<int> CreatePostFavoriteAsync(int userId, int postId)
        {
            var db = Database.getInstance();
            // 检查是否已经收藏
            var query = "SELECT * FROM Favorites WHERE UserId = @UserId AND PostId = @PostId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@PostId", SqlDbType.Int) { Value = postId },
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            if (result.Rows.Count > 0)
            {
                return 0;
            }
            // 添加收藏记录
            query = "INSERT INTO Favorites (UserId, PostId) VALUES (@UserId, @PostId)";
            parameters = new SqlParameter[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@PostId", SqlDbType.Int) { Value = postId },
            };
            return await db.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
    }
}
