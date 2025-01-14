using imarket.models;
using imarket.service.IService;
using imarket.utils;
using Microsoft.Data.SqlClient;
using System.Data;
namespace imarket.service.Service
{
    public class PostService:IPostService
    {
        public async Task<int> GetPostNums()
        {
            var db = Database.getInstance();
            var query = "SELECT COUNT(*) FROM Posts";
            var result = await db.ExecuteQuery(query, CommandType.Text);
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
            var db = Database.getInstance();
            var query = "SELECT * FROM Posts ORDER BY CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Offset", SqlDbType.Int) { Value = (page - 1) * pageSize },
                new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize },
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                posts.Add(new PostModels
                {
                    Id = Convert.ToInt32(row["Id"]!),
                    Title = row["Title"].ToString()!,
                    Content = row["Content"].ToString()!,
                    UserId = Convert.ToInt32(row["UserId"]!),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                    Status = Convert.ToInt32(row["Status"]!),
                });
            }
            return posts;
        }
        public async Task<IEnumerable<PostModels>> GetPostsByUserIdAsync(int userId)
        {
            var posts = new List<PostModels>();
            var db = Database.getInstance();
            var query = "SELECT * FROM Posts WHERE UserId = @UserId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                posts.Add(new PostModels
                {
                    Id = Convert.ToInt32(row["Id"]!),
                    Title = row["Title"].ToString()!,
                    Content = row["Content"].ToString()!,
                    UserId = Convert.ToInt32(row["UserId"]!),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                    Status = Convert.ToInt32(row["Status"]!),
                });
            }
            return posts;
        }
        public async Task<IEnumerable<PostModels>> GetPostsByCategoryIdAsync(int categoryId, int page, int pageSize)
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
            var db = Database.getInstance();
            var query = "SELECT * FROM Posts WHERE Id IN (SELECT PostId FROM PostCategories WHERE CategoryId = @CategoryId) ORDER BY CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId },
                new SqlParameter("@Offset", SqlDbType.Int) { Value = (page - 1) * pageSize },
                new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize },
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                posts.Add(new PostModels
                {
                    Id = Convert.ToInt32(row["Id"]!),
                    Title = row["Title"].ToString()!,
                    Content = row["Content"].ToString()!,
                    UserId = Convert.ToInt32(row["UserId"]!),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                    Status = Convert.ToInt32(row["Status"]!),
                });
            }
            return posts;
        }

        public async Task<PostModels> GetPostByIdAsync(int id)
        {
            var db = Database.getInstance();
            var query = "SELECT * FROM Posts WHERE Id = @Id";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", SqlDbType.Int) { Value = id }
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            if (result.Rows.Count == 0)
            {
                return null;
            }
            var row = result.Rows[0];
            return new PostModels
            {
                Id = Convert.ToInt32(row["Id"]!),
                Title = row["Title"].ToString()!,
                Content = row["Content"].ToString()!,
                UserId = Convert.ToInt32(row["UserId"]!),
                CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
            };
        }
        public async Task<int> CreatePostAsync(PostModels post)
        {
            var db = Database.getInstance();
            var query = "INSERT INTO Posts (Title, Content, UserId, CreatedAt) VALUES (@Title, @Content, @UserId, @CreatedAt)";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Title", SqlDbType.NVarChar) { Value = post.Title },
                new SqlParameter("@Content", SqlDbType.NVarChar) { Value = post.Content },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = post.UserId },
                new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = post.CreatedAt },
            };
            return await db.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> DeletePostAsync(int id)
        {
            var db = Database.getInstance();
            var query = "DELETE FROM Posts WHERE Id = @Id";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", SqlDbType.Int) { Value = id }
            };
            return await db.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
    }
}
