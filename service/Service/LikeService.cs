using imarket.models;
using imarket.service.IService;
using imarket.utils;
using Microsoft.Data.SqlClient;
using System.Data;
namespace imarket.service.Service
{
    public class LikeService : ILikeService
    {
        private readonly Database _database;
        public LikeService(Database database)
        {
            _database = database;
        }
        public async Task<IEnumerable<LikeModels>> GetPostLikesByPostIdAsync(string postId)
        {
            var likes = new List<LikeModels>();
            var query = "SELECT * FROM Likes WHERE PostId = @PostId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@PostId", SqlDbType.Char) { Value = postId }
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                likes.Add(new LikeModels
                {
                    Id = row["Id"].ToString()!,
                    PostId = row["PostId"].ToString()!,
                    UserId = row["UserId"].ToString()!,
                    CommentId = row["CommentId"].ToString()!,
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                });
            }
            return likes;
        }
        public async Task<IEnumerable<LikeModels>> GetCommentLikesByCommentIdAsync(string commentId)
        {
            var likes = new List<LikeModels>();
            var query = "SELECT * FROM Likes WHERE CommentId = @CommentId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@CommentId", SqlDbType.Char) { Value = commentId }
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                likes.Add(new LikeModels
                {
                    Id = row["Id"].ToString()!,
                    PostId = row["PostId"].ToString()!,
                    UserId = row["UserId"].ToString()!,
                    CommentId = row["CommentId"].ToString()!,
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                });
            }
            return likes;
        }
        public async Task<int> GetPostLikeNumsByPostIdAsync(string postId)
        {
            var query = "SELECT COUNT(*) FROM Likes WHERE PostId = @PostId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@PostId", SqlDbType.Char) { Value = postId }
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            return Convert.ToInt32(result.Rows[0][0]!);
        }
        public async Task<int> GetCommentLikeNumsByCommentIdAsync(string commentId)
        {
            var query = "SELECT COUNT(*) FROM Likes WHERE CommentId = @CommentId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@CommentId", SqlDbType.Char) { Value = commentId }
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            return Convert.ToInt32(result.Rows[0][0]!);
        }
        public async Task<int> CreateLikeAsync(LikeModels like)
        {
            string query;
            SqlParameter[] parameters;
            // 检查是帖子点赞还是评论点赞
            if (like.CommentId == null)
            {
                // 检查是否已经点赞
                query = "SELECT * FROM Likes WHERE PostId = @PostId AND UserId = @UserId AND CommentId IS NULL";
                parameters = new SqlParameter[]
                {
                    new SqlParameter("@PostId", SqlDbType.Char) { Value = like.PostId },
                    new SqlParameter("@UserId", SqlDbType.Char) { Value = like.UserId },
                };
                var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
                if (result.Rows.Count > 0)
                {
                    return 0;
                }
            }
            else
            {
                // 检查是否已经点赞
                query = "SELECT * FROM Likes WHERE PostId IS NULL AND UserId = @UserId AND CommentId = @CommentId";
                parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", SqlDbType.Char) { Value = like.UserId },
                    new SqlParameter("@CommentId", SqlDbType.Char) { Value = like.CommentId },
                };
                var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
                if (result.Rows.Count > 0)
                {
                    return 0;
                }
            }
            // 添加点赞记录
            if (like.CommentId == null)
            {
                query = "INSERT INTO Likes (Id, PostId, UserId, CommentId, CreatedAt) VALUES (@Id, @PostId, @UserId, NULL, @CreatedAt)";
            }
            else
            {
                query = "INSERT INTO Likes (Id, PostId, UserId, CommentId, CreatedAt) VALUES (@Id, NULL, @UserId, @CommentId, @CreatedAt)";
            }
            parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", SqlDbType.Char) { Value = like.Id },
                new SqlParameter("@PostId", SqlDbType.Char) { Value = like.PostId },
                new SqlParameter("@UserId", SqlDbType.Char) { Value = like.UserId },
                new SqlParameter("@CommentId", SqlDbType.Char) { Value = like.CommentId },
                new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = like.CreatedAt },
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
        public async Task<int> DeleteLikeAsync(LikeModels like)
        {
            if (like.CommentId == null)
            {
                var query = "DELETE FROM Likes WHERE PostId = @PostId AND UserId = @UserId AND CommentId IS NULL";
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@PostId", SqlDbType.Char) { Value = like.PostId },
                    new SqlParameter("@UserId", SqlDbType.Char) { Value = like.UserId },
                };
                return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
            }
            else
            {
                var query = "DELETE FROM Likes WHERE PostId IS NULL AND UserId = @UserId AND CommentId = @CommentId";
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", SqlDbType.Char) { Value = like.UserId },
                    new SqlParameter("@CommentId", SqlDbType.Char) { Value = like.CommentId },
                };
                return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
            }
        }

        public async Task<bool> CheckUserLikeCommentAsync(string userId, string commentId)
        {
            var query = "SELECT * FROM Likes WHERE UserId = @UserId AND CommentId = @CommentId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserId", SqlDbType.Char) { Value = userId },
                new SqlParameter("@CommentId", SqlDbType.Char) { Value = commentId }
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            if (result.Rows.Count == 0)
            {
                return false;
            }
            return true;
        }
        public async Task<bool> CheckUserLikePostAsync(string userId, string postId)
        {
            var query = "SELECT * FROM Likes WHERE UserId = @UserId AND PostId = @PostId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserId", SqlDbType.Char) { Value = userId },
                new SqlParameter("@PostId", SqlDbType.Char) { Value = postId }
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            if (result.Rows.Count == 0)
            {
                return false;
            }
            return true;
        }
    }
}
