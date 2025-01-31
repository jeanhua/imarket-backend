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
                    Id = ulong.Parse(row["Id"].ToString()!),
                    Name = row["Name"].ToString()!,
                    Description = row["Description"].ToString()!,
                });
            }
            return categories;
        }
        public async Task<CategoryModels?> GetCategoryByIdAsync(ulong id)
        {
            var query = "SELECT * FROM Categories WHERE Id = @Id";
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
            return new CategoryModels
            {
                Id = ulong.Parse(row["Id"].ToString()!),
                Name = row["Name"].ToString()!,
                Description = row["Description"].ToString()!,
            };
        }
        public async Task<int> CreateCategoryAsync(CategoryModels category)
        {
            var query = "INSERT INTO Categories (Id, Name, Description) VALUES (@Id, @Name, @Description)";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", category.Id),
                new MySqlParameter("@Name", category.Name),
                new MySqlParameter("@Description", category.Description),
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
        public async Task<int> DeleteCategoryAsync(ulong id)
        {
            var query = "DELETE FROM Categories WHERE Id = @Id";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id",id )
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
        public async Task<int> UpdateCategoryAsync(ulong categoryId, CategoryModels category)
        {
            var query = "UPDATE Categories SET Name = @Name, Description = @Description WHERE Id = @Id";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", categoryId),
                new MySqlParameter("@Name", category.Name),
                new MySqlParameter("@Description", category.Description),
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        // 帖子分类关联相关
        public async Task<string?> GetPostCategoriesByPostIdAsync(ulong postId)
        {
            var categories = new List<string>();
            var query = "SELECT * FROM PostCategories WHERE PostId = @PostId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@PostId", postId )
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            if(result.Rows.Count == 0)
            {
                return null;
            }
            return result.Rows[0]["CategoryId"].ToString();
        }
        public async Task<int> CreatePostCategoryAsync(PostCategoryModels postCategory)
        {
            var query = "INSERT INTO PostCategories (PostId, CategoryId) VALUES (@PostId, @CategoryId)";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@PostId",postCategory.PostId ),
                new MySqlParameter("@CategoryId", postCategory.CategoryId ),
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
        public async Task<int> DeletePostCategoryAsync(PostCategoryModels postCategory)
        {
            var query = "DELETE FROM PostCategories WHERE PostId = @PostId AND CategoryId = @CategoryId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@PostId",postCategory.PostId ),
                new MySqlParameter("@CategoryId",postCategory.CategoryId ),
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> DeletePostCategoryByPostIdAsync(ulong postId)
        {
            var query = "DELETE FROM PostCategories WHERE PostId = @PostId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@PostId",postId),
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
    }
}
