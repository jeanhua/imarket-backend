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
                    Id = Convert.ToInt32(row["Id"]!),
                    Name = row["Name"].ToString()!,
                    Description = row["Description"].ToString()!,
                });
            }
            return categories;
        }
        public async Task<CategoryModels> GetCategoryByIdAsync(int id)
        {
            var db = Database.getInstance();
            var query = "SELECT * FROM Categories WHERE Id = @Id";
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
            return new CategoryModels
            {
                Id = Convert.ToInt32(row["Id"]!),
                Name = row["Name"].ToString()!,
                Description = row["Description"].ToString()!,
            };
        }
        public async Task<int> CreateCategoryAsync(CategoryModels category)
        {
            var db = Database.getInstance();
            var query = "INSERT INTO Categories (Name, Description) VALUES (@Name, @Description)";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Name", SqlDbType.NVarChar) { Value = category.Name },
                new SqlParameter("@Description", SqlDbType.NVarChar) { Value = category.Description },
            };
            return await db.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
        public async Task<int> DeleteCategoryAsync(int id)
        {
            var db = Database.getInstance();
            var query = "DELETE FROM Categories WHERE Id = @Id";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", SqlDbType.Int) { Value = id }
            };
            return await db.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
        public async Task<int> UpdateCategoryAsync(int categoryId, CategoryModels category)
        {
            var db = Database.getInstance();
            var query = "UPDATE Categories SET Name = @Name, Description = @Description WHERE Id = @Id";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", SqlDbType.Int) { Value = categoryId },
                new SqlParameter("@Name", SqlDbType.NVarChar) { Value = category.Name },
                new SqlParameter("@Description", SqlDbType.NVarChar) { Value = category.Description },
            };
            return await db.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        // 帖子分类关联相关
        public async Task<IEnumerable<int>> GetPostCategoriesByPostIdAsync(int postId)
        {
            var categories = new List<int>();
            var db = Database.getInstance();
            var query = "SELECT * FROM PostCategories WHERE PostId = @PostId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@PostId", SqlDbType.Int) { Value = postId }
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                categories.Add(Convert.ToInt32(row["CategoryId"]!));
            }
            return categories;
        }
        public async Task<PostCategoryModels> CreatePostCategoryAsync(PostCategoryModels postCategory)
        {
            var db = Database.getInstance();
            var query = "INSERT INTO PostCategories (PostId, CategoryId) VALUES (@PostId, @CategoryId)";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@PostId", SqlDbType.Int) { Value = postCategory.PostId },
                new SqlParameter("@CategoryId", SqlDbType.Int) { Value = postCategory.CategoryId },
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
                new SqlParameter("@PostId", SqlDbType.Int) { Value = postCategory.PostId },
                new SqlParameter("@CategoryId", SqlDbType.Int) { Value = postCategory.CategoryId },
            };
            return await db.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
    }
}
