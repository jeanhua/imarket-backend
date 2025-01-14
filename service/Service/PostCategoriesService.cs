using imarket.models;
using imarket.service.IService;
using imarket.utils;
using Microsoft.Data.SqlClient;
using System.Data;

namespace imarket.service.Service
{
    public class PostCategoriesService:IPostCategoriesService
    {
        // 帖子分类相关
        public async Task<IEnumerable<CategoryModels>> GetAllCategoriesAsync()
        {
            var categories = new List<CategoryModels>();
            var db = Database.getInstance();
            var query = "SELECT * FROM Categories";
            var result = await db.ExecuteQuery(query, CommandType.Text);
            foreach (DataRow row in result.Rows)
            {
                categories.Add(new CategoryModels
                {
                    Id = row["Id"].ToString()!,
                    Name = row["Name"].ToString()!,
                    Description = row["Description"].ToString()!,
                });
            }
            return categories;
        }
        public async Task<CategoryModels> GetCategoryByIdAsync(string id)
        {
            var db = Database.getInstance();
            var query = "SELECT * FROM Categories WHERE Id = @Id";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", SqlDbType.Char) { Value = id }
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            if (result.Rows.Count == 0)
            {
                return null;
            }
            var row = result.Rows[0];
            return new CategoryModels
            {
                Id = row["Id"].ToString()!,
                Name = row["Name"].ToString()!,
                Description = row["Description"].ToString()!,
            };
        }
        public async Task<int> CreateCategoryAsync(CategoryModels category)
        {
            var db = Database.getInstance();
            var query = "INSERT INTO Categories (Id, Name, Description) VALUES (@Id, @Name, @Description)";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", SqlDbType.Char) { Value = category.Id },
                new SqlParameter("@Name", SqlDbType.NVarChar) { Value = category.Name },
                new SqlParameter("@Description", SqlDbType.NVarChar) { Value = category.Description },
            };
            return await db.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
        public async Task<int> DeleteCategoryAsync(string id)
        {
            var db = Database.getInstance();
            var query = "DELETE FROM Categories WHERE Id = @Id";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", SqlDbType.Char) { Value = id }
            };
            return await db.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
        public async Task<int> UpdateCategoryAsync(string categoryId, CategoryModels category)
        {
            var db = Database.getInstance();
            var query = "UPDATE Categories SET Name = @Name, Description = @Description WHERE Id = @Id";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", SqlDbType.Char) { Value = categoryId },
                new SqlParameter("@Name", SqlDbType.NVarChar) { Value = category.Name },
                new SqlParameter("@Description", SqlDbType.NVarChar) { Value = category.Description },
            };
            return await db.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        // 帖子分类关联相关
        public async Task<IEnumerable<string>> GetPostCategoriesByPostIdAsync(string postId)
        {
            var categories = new List<string>();
            var db = Database.getInstance();
            var query = "SELECT * FROM PostCategories WHERE PostId = @PostId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@PostId", SqlDbType.Char) { Value = postId }
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                categories.Add(row["CategoryId"].ToString()!);
            }
            return categories;
        }
        public async Task<PostCategoryModels> CreatePostCategoryAsync(PostCategoryModels postCategory)
        {
            var db = Database.getInstance();
            var query = "INSERT INTO PostCategories (PostId, CategoryId) VALUES (@PostId, @CategoryId)";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@PostId", SqlDbType.Char) { Value = postCategory.PostId },
                new SqlParameter("@CategoryId", SqlDbType.Char) { Value = postCategory.CategoryId },
            };
            await db.ExecuteNonQuery(query, CommandType.Text, parameters);
            return postCategory;
        }
        public async Task<int> DeletePostCategoryAsync(PostCategoryModels postCategory)
        {
            var db = Database.getInstance();
            var query = "DELETE FROM PostCategories WHERE PostId = @PostId AND CategoryId = @CategoryId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@PostId", SqlDbType.Char) { Value = postCategory.PostId },
                new SqlParameter("@CategoryId", SqlDbType.Char) { Value = postCategory.CategoryId },
            };
            return await db.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
    }
}
