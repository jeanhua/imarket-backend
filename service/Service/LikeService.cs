using imarket.models;
using imarket.service.IService;
using imarket.utils;
using MySql.Data.MySqlClient;
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
        public async Task<IEnumerable<LikeModels>> GetPostLikesByPostIdAsync(ulong postId)
        {
            var likes = new List<LikeModels>();
            var query = "SELECT * FROM Likes WHERE PostId = @PostId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@PostId", postId)
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                likes.Add(new LikeModels
                {
                    Id = ulong.Parse(row["Id"].ToString()!),
                    PostId = ulong.Parse(row["PostId"].ToString()!),
                    UserId = ulong.Parse(row["UserId"].ToString()!),
                    CommentId = ulong.Parse(row["CommentId"].ToString()!),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                });
            }
            return likes;
        }
        public async Task<IEnumerable<LikeModels>> GetCommentLikesByCommentIdAsync(ulong commentId)
        {
            var likes = new List<LikeModels>();
            var query = "SELECT * FROM Likes WHERE CommentId = @CommentId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@CommentId", commentId)
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                likes.Add(new LikeModels
                {
                    Id = ulong.Parse(row["Id"].ToString()!),
                    PostId = ulong.Parse(row["PostId"].ToString()!),
                    UserId = ulong.Parse(row["UserId"].ToString()!),
                    CommentId = ulong.Parse(row["CommentId"].ToString()!),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                });
            }
            return likes;
        }

        public async Task<IEnumerable<HotRankingModels.Post>> GetHotRankingAsync()
        {
            var posts = new List<HotRankingModels.Post>();
            // 查询最近一周内点赞数最多的10条帖子
            var query = @"
                SELECT 
                    p.Id, 
                    p.Title, 
                    p.Content, 
                    p.UserId, 
                    p.CreatedAt, 
                    COUNT(l.Id) AS LikeCount
                FROM 
                    Likes l
                INNER JOIN 
                    Posts p ON l.PostId = p.Id
                WHERE 
                    l.CreatedAt >= DATE_SUB(NOW(), INTERVAL 1 WEEK)
                    AND l.PostId IS NOT NULL
                GROUP BY 
                    p.Id
                ORDER BY 
                    LikeCount DESC
                LIMIT 10";

            var result = await _database.ExecuteQuery(query, CommandType.Text);
            foreach (DataRow row in result.Rows)
            {
                posts.Add(new HotRankingModels.Post
                {
                    Id = ulong.Parse(row["Id"].ToString()!),
                    Title = row["Title"].ToString()!,
                    Content = row["Content"].ToString()!,
                    UserId = ulong.Parse(row["UserId"].ToString()!),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                    LikeCount = Convert.ToInt32(row["LikeCount"]!)
                });
            }
            return posts;
        }

        public async Task<int> GetPostLikeNumsByPostIdAsync(ulong postId)
        {
            var query = "SELECT COUNT(*) FROM Likes WHERE PostId = @PostId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@PostId", postId)
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            return Convert.ToInt32(result.Rows[0][0]!);
        }
        public async Task<int> GetCommentLikeNumsByCommentIdAsync(ulong commentId)
        {
            var query = "SELECT COUNT(*) FROM Likes WHERE CommentId = @CommentId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@CommentId", commentId)
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            return Convert.ToInt32(result.Rows[0][0]!);
        }
        public async Task<int> CreateLikeAsync(LikeModels like)
        {
            string query;
            MySqlParameter[] parameters;
            if (like.CommentId == null)
            {
                query = "SELECT * FROM Likes WHERE PostId = @PostId AND UserId = @UserId AND CommentId IS NULL";
                parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@PostId", like.PostId),
                    new MySqlParameter("@UserId", like.UserId),
                };
                var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
                if (result.Rows.Count > 0)
                {
                    return 0;
                }
            }
            else
            {
                query = "SELECT * FROM Likes WHERE PostId IS NULL AND UserId = @UserId AND CommentId = @CommentId";
                parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@UserId", like.UserId),
                    new MySqlParameter("@CommentId", like.CommentId),
                };
                var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
                if (result.Rows.Count > 0)
                {
                    return 0;
                }
            }
            if (like.CommentId == null)
            {
                query = "INSERT INTO Likes (PostId, UserId, CommentId, CreatedAt) VALUES (@PostId, @UserId, NULL, @CreatedAt)";
            }
            else
            {
                query = "INSERT INTO Likes (PostId, UserId, CommentId, CreatedAt) VALUES (NULL, @UserId, @CommentId, @CreatedAt)";
            }
            parameters = new MySqlParameter[]
            {
                new MySqlParameter("@PostId", like.PostId),
                new MySqlParameter("@UserId", like.UserId),
                new MySqlParameter("@CommentId", like.CommentId),
                new MySqlParameter("@CreatedAt", like.CreatedAt),
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
        public async Task<int> DeleteLikeAsync(LikeModels like)
        {
            if (like.CommentId == null)
            {
                var query = "DELETE FROM Likes WHERE PostId = @PostId AND UserId = @UserId AND CommentId IS NULL";
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@PostId", like.PostId),
                    new MySqlParameter("@UserId", like.UserId),
                };
                return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
            }
            else
            {
                var query = "DELETE FROM Likes WHERE PostId IS NULL AND UserId = @UserId AND CommentId = @CommentId";
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@UserId", like.UserId),
                    new MySqlParameter("@CommentId", like.CommentId),
                };
                return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
            }
        }

        public async Task<int> DeleteLikesByPostIdAsync(ulong postId)
        {
            var query = "DELETE FROM Likes WHERE PostId = @PostId AND CommentId IS NULL";
            var parameters = new MySqlParameter[]
            {
                    new MySqlParameter("@PostId", postId),
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> DeleteLikesByUserIdAsync(ulong userId)
        {
            var query = "DELETE FROM Likes WHERE UserId = @UserId";
            var parameters = new MySqlParameter[]
            {
                    new MySqlParameter("@UserId", userId),
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> DeleteLikesByCommentIdAsync(ulong commentId)
        {
            var query = "DELETE FROM Likes WHERE PostId IS NULL AND CommentId = @CommentId";
            var parameters = new MySqlParameter[]
            {
                    new MySqlParameter("@CommentId", commentId),
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<bool> CheckUserLikeCommentAsync(ulong userId, ulong commentId)
        {
            var query = "SELECT * FROM Likes WHERE UserId = @UserId AND CommentId = @CommentId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@UserId", userId),
                new MySqlParameter("@CommentId", commentId)
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            if (result.Rows.Count == 0)
            {
                return false;
            }
            return true;
        }
        public async Task<bool> CheckUserLikePostAsync(ulong userId, ulong postId)
        {
            var query = "SELECT * FROM Likes WHERE UserId = @UserId AND PostId = @PostId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@UserId", userId),
                new MySqlParameter("@PostId", postId)
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
