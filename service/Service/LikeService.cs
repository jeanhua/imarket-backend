using imarket.models;
using imarket.service.IService;
using imarket.utils;
using Microsoft.Data.SqlClient;
using System.Data;
namespace imarket.service.Service
{
    public class LikeService : ILikeService
    {
        public async Task<IEnumerable<LikeModels>> GetPostLikesByPostIdAsync(int postId)
        {
            var likes = new List<LikeModels>();
            var db = Database.getInstance();
            var query = "SELECT * FROM Likes WHERE PostId = @PostId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@PostId", SqlDbType.Int) { Value = postId }
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                likes.Add(new LikeModels
                {
                    Id = Convert.ToInt32(row["Id"]!),
                    PostId = Convert.ToInt32(row["PostId"]!),
                    UserId = Convert.ToInt32(row["UserId"]!),
                    CommentId = Convert.ToInt32(row["CommentId"]!),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                });
            }
            return likes;
        }
        public async Task<int> GetPostLikeNumsByPostIdAsync(int postId)
        {
            var db = Database.getInstance();
            var query = "SELECT COUNT(*) FROM Likes WHERE PostId = @PostId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@PostId", SqlDbType.Int) { Value = postId }
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            return Convert.ToInt32(result.Rows[0][0]!);
        }
        public async Task<int> CreatePostLikeAsync(LikeModels postLike)
        {
            var db = Database.getInstance();
            // 检查是否已经点赞
            var query = "SELECT * FROM Likes WHERE PostId = @PostId AND UserId = @UserId AND CommentId = @CommentId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@PostId", SqlDbType.Int) { Value = postLike.PostId },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = postLike.UserId },
                new SqlParameter("@CommentId", SqlDbType.Int) { Value = postLike.CommentId },
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            if (result.Rows.Count > 0)
            {
                return 0;
            }
            // 添加点赞记录
            query = "INSERT INTO Likes (PostId, UserId, CommentId, CreatedAt) VALUES (@PostId, @UserId, @CommentId, @CreatedAt)";
            parameters = new SqlParameter[]
            {
                new SqlParameter("@PostId", SqlDbType.Int) { Value = postLike.PostId },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = postLike.UserId },
                new SqlParameter("@CommentId", SqlDbType.Int) { Value = postLike.CommentId },
                new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = postLike.CreatedAt },
            };
            return await db.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
        public async Task<int> DeletePostLikeAsync(LikeModels postLike)
        {
            var db = Database.getInstance();
            var query = "DELETE FROM Likes WHERE PostId = @PostId AND UserId = @UserId AND CommentId = @CommentId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@PostId", SqlDbType.Int) { Value = postLike.PostId },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = postLike.UserId },
                new SqlParameter("@CommentId", SqlDbType.Int) { Value = postLike.CommentId },
            };
            return await db.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
    }
}
