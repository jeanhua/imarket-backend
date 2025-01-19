using imarket.models;
using imarket.service.IService;
using imarket.utils;
using MySql.Data.MySqlClient;
using System.Data;
namespace imarket.service.Service
{
    public class CommentService : ICommentService
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
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@PostId", postId)
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
        public async Task<CommentModels?> GetCommentByIdAsync(string id)
        {
            var query = "SELECT * FROM Comments WHERE Id = @Id";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", id)
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
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", comment.Id),
                new MySqlParameter("@PostId", comment.PostId),
                new MySqlParameter("@UserId", comment.UserId),
                new MySqlParameter("@Content", comment.Content),
                new MySqlParameter("@CreatedAt", comment.CreatedAt),
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
        public async Task<int> DeleteCommentAsync(string id)
        {
            var query = "DELETE FROM Comments WHERE Id = @Id";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", id)
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> GetCommentsNumByPostId(string postId)
        {
            var query = "SELECT COUNT(*) FROM Comments WHERE PostId = @PostId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@PostId",postId)
            };
            var num = await _database.ExecuteQuery(query,CommandType.Text,parameters);
            if (num == null || num.Rows.Count == 0)
            {
                return 0;
            }
            return Convert.ToInt32(num.Rows[0][0]);
        }
    }
}
