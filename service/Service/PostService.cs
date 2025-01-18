using imarket.models;
using imarket.service.IService;
using imarket.utils;
using MySql.Data.MySqlClient;
using System.Data;
namespace imarket.service.Service
{
    public class PostService : IPostService
    {
        private readonly Database _database;
        public PostService(Database database)
        {
            _database = database;
        }
        public async Task<int> GetPostNums()
        {
            var query = "SELECT COUNT(*) FROM Posts";
            var result = await _database.ExecuteQuery(query, CommandType.Text);
            return Convert.ToInt32(result.Rows[0][0]!);
        }
        public async Task<IEnumerable<PostModels>> GetAllPostsAsync(int page, int pageSize)
        {
            if (page < 1)
            {
                page = 1;
            }
            if (pageSize < 1 || pageSize > 20)
            {
                pageSize = 10;
            }
            var posts = new List<PostModels>();
            var query = "SELECT * FROM Posts ORDER BY CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Offset", SqlDbType.Int) { Value = (page - 1) * pageSize },
                new MySqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize },
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                posts.Add(new PostModels
                {
                    Id = row["Id"].ToString()!,
                    Title = row["Title"].ToString()!,
                    Content = row["Content"].ToString()!,
                    UserId = row["UserId"].ToString()!,
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                    Status = Convert.ToInt32(row["Status"]!),
                });
            }
            return posts;
        }
        public async Task<IEnumerable<PostModels>> GetPostsByUserIdAsync(string userId)
        {
            var posts = new List<PostModels>();
            var query = "SELECT * FROM Posts WHERE UserId = @UserId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@UserId", SqlDbType.Char) { Value = userId }
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                posts.Add(new PostModels
                {
                    Id = row["Id"].ToString()!,
                    Title = row["Title"].ToString()!,
                    Content = row["Content"].ToString()!,
                    UserId = row["UserId"].ToString()!,
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                    Status = Convert.ToInt32(row["Status"]!),
                });
            }
            return posts;
        }
        public async Task<IEnumerable<PostModels>> GetPostsByCategoryIdAsync(string categoryId, int page, int pageSize)
        {
            if (page < 1)
            {
                page = 1;
            }
            if (pageSize < 1 || pageSize > 20)
            {
                pageSize = 10;
            }
            var posts = new List<PostModels>();
            var query = "SELECT * FROM Posts WHERE Id IN (SELECT PostId FROM PostCategories WHERE CategoryId = @CategoryId) ORDER BY CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@CategoryId", SqlDbType.Char) { Value = categoryId },
                new MySqlParameter("@Offset", SqlDbType.Int) { Value = (page - 1) * pageSize },
                new MySqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize },
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                posts.Add(new PostModels
                {
                    Id = row["Id"].ToString()!,
                    Title = row["Title"].ToString()!,
                    Content = row["Content"].ToString()!,
                    UserId = row["UserId"].ToString()!,
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                    Status = Convert.ToInt32(row["Status"]!),
                });
            }
            return posts;
        }

        public async Task<PostModels?> GetPostByIdAsync(string id)
        {
            var query = "SELECT * FROM Posts WHERE Id = @Id";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", SqlDbType.Char) { Value = id }
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            if (result.Rows.Count == 0)
            {
                return null;
            }
            var row = result.Rows[0];
            return new PostModels
            {
                Id = row["Id"].ToString()!,
                Title = row["Title"].ToString()!,
                Content = row["Content"].ToString()!,
                UserId = row["UserId"].ToString()!,
                CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
            };
        }
        public async Task<int> CreatePostAsync(PostModels post)
        {
            var query = "INSERT INTO Posts (Id, Title, Content, UserId, CreatedAt, Status) VALUES (@Id, @Title, @Content, @UserId, @CreatedAt, @Status)";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", SqlDbType.Char) { Value = post.Id },
                new MySqlParameter("@Title", SqlDbType.NVarChar) { Value = post.Title },
                new MySqlParameter("@Content", SqlDbType.NVarChar) { Value = post.Content },
                new MySqlParameter("@UserId", SqlDbType.Char) { Value = post.UserId },
                new MySqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = post.CreatedAt },
                new MySqlParameter("@Status", SqlDbType.Int) { Value = post.Status },
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> DeletePostAsync(string id)
        {
            var query = "DELETE FROM Posts WHERE Id = @Id";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", SqlDbType.Char) { Value = id }
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> UpdatePostAsync(PostModels post)
        {
            var query = "UPDATE Posts SET Title = @Title, Content = @Content, UserId = @UserId , CreatedAt = @CreatedAt , Status = @Status WHERE Id = @Id";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", SqlDbType.Char) { Value = post.Id },
                new MySqlParameter("@Title", SqlDbType.NVarChar) { Value = post.Title },
                new MySqlParameter("@Content", SqlDbType.NVarChar) { Value = post.Content },
                new MySqlParameter("@UserId", SqlDbType.Char) { Value = post.UserId },
                new MySqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = post.CreatedAt },
                new MySqlParameter("@Status", SqlDbType.Int) { Value = post.Status },
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
    }
}