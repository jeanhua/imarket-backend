using imarket.models;
using imarket.service.IService;
using imarket.utils;
using Microsoft.Data.SqlClient;
using System.Data;
namespace imarket.service.Service
{
    public class CommentService:ICommentService
    {
        private readonly Database _database;
        public CommentService(Database database)
        {
            _database = database;
        }
        public async Task<IEnumerable<CommentModels>> GetCommentsByPostIdAsync(string postId)
        {
            var comments = new List<CommentModels>();
            var query = "SELECT * FROM Comments WHERE PostId = @PostId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@PostId", SqlDbType.Char) { Value = postId }
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                comments.Add(new CommentModels
                {
                    Id = row["Id"].ToString()!,
                    PostId = row["PostId"].ToString()!,
                    UserId = row["UserId"].ToString()!,
                    Content = row["Content"].ToString()!,
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                });
            }
            return comments;
        }
        public async Task<CommentModels> GetCommentByIdAsync(string id)
        {
            var query = "SELECT * FROM Comments WHERE Id = @Id";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", SqlDbType.Char) { Value = id }
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            if (result.Rows.Count == 0)
            {
                return null;
            }
            var row = result.Rows[0];
            return new CommentModels
            {
                Id = row["Id"].ToString()!,
                PostId = row["PostId"].ToString()!,
                UserId = row["UserId"].ToString()!,
                Content = row["Content"].ToString()!,
                CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
            };
        }
        public async Task<int> CreateCommentAsync(CommentModels comment)
        {
            var query = "INSERT INTO Comments (Id, PostId, UserId, Content, CreatedAt) VALUES (@Id, @PostId, @UserId, @Content, @CreatedAt)";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", SqlDbType.Char) { Value = comment.Id },
                new SqlParameter("@PostId", SqlDbType.Char) { Value = comment.PostId },
                new SqlParameter("@UserId", SqlDbType.Char) { Value = comment.UserId },
                new SqlParameter("@Content", SqlDbType.NVarChar) { Value = comment.Content },
                new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = comment.CreatedAt },
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
        public async Task<int> DeleteCommentAsync(string id)
        {
            var query = "DELETE FROM Comments WHERE Id = @Id";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", SqlDbType.Char) { Value = id }
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
    }
}
