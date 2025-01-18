using imarket.models;
using imarket.service.IService;
using imarket.utils;
using MySql.Data.MySqlClient;
using System.Data;

namespace imarket.service.Service
{
    public class PostCategoriesService:IPostCategoriesService
    {
        private readonly Database _database;
        public PostCategoriesService(Database database)
        {
            _database = database;
        }
        // 帖子分类相关
        public async Task<IEnumerable<CategoryModels>> GetAllCategoriesAsync()
        {
            var categories = new List<CategoryModels>();
            var query = "SELECT * FROM Categories";
            var result = await _database.ExecuteQuery(query, CommandType.Text);
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
        public async Task<CategoryModels?> GetCategoryByIdAsync(string id)
        {
            var query = "SELECT * FROM Categories WHERE Id = @Id";
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
            return new CategoryModels
            {
                Id = row["Id"].ToString()!,
                Name = row["Name"].ToString()!,
                Description = row["Description"].ToString()!,
            };
        }
        public async Task<int> CreateCategoryAsync(CategoryModels category)
        {
            var query = "INSERT INTO Categories (Id, Name, Description) VALUES (@Id, @Name, @Description)";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", SqlDbType.Char) { Value = category.Id },
                new MySqlParameter("@Name", SqlDbType.NVarChar) { Value = category.Name },
                new MySqlParameter("@Description", SqlDbType.NVarChar) { Value = category.Description },
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
        public async Task<int> DeleteCategoryAsync(string id)
        {
            var query = "DELETE FROM Categories WHERE Id = @Id";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", SqlDbType.Char) { Value = id }
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
        public async Task<int> UpdateCategoryAsync(string categoryId, CategoryModels category)
        {
            var query = "UPDATE Categories SET Name = @Name, Description = @Description WHERE Id = @Id";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", SqlDbType.Char) { Value = categoryId },
                new MySqlParameter("@Name", SqlDbType.NVarChar) { Value = category.Name },
                new MySqlParameter("@Description", SqlDbType.NVarChar) { Value = category.Description },
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        // 帖子分类关联相关
        public async Task<IEnumerable<string>> GetPostCategoriesByPostIdAsync(string postId)
        {
            var categories = new List<string>();
            var query = "SELECT * FROM PostCategories WHERE PostId = @PostId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@PostId", SqlDbType.Char) { Value = postId }
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                categories.Add(row["CategoryId"].ToString()!);
            }
            return categories;
        }
        public async Task<int> CreatePostCategoryAsync(PostCategoryModels postCategory)
        {
            var query = "INSERT INTO PostCategories (PostId, CategoryId) VALUES (@PostId, @CategoryId)";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@PostId", SqlDbType.Char) { Value = postCategory.PostId },
                new MySqlParameter("@CategoryId", SqlDbType.Char) { Value = postCategory.CategoryId },
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
        public async Task<int> DeletePostCategoryAsync(PostCategoryModels postCategory)
        {
            var query = "DELETE FROM PostCategories WHERE PostId = @PostId AND CategoryId = @CategoryId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@PostId", SqlDbType.Char) { Value = postCategory.PostId },
                new MySqlParameter("@CategoryId", SqlDbType.Char) { Value = postCategory.CategoryId },
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
    }
}
