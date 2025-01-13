using imarket.models;
using imarket.utils;
using Microsoft.Data.SqlClient;
using System.Data;

namespace imarket.service
{
    public class Service : IService
    {
        private static Service _instance;
        public static Service getInstance()
        {
            if (_instance == null)
            {
                _instance = new Service();
            }
            return _instance;
        }
        public async Task<int> GetPostNums()
        {
            var db = Database.getInstance();
            var query = "SELECT COUNT(*) FROM Posts";
            var result = await db.ExecuteQuery(query, CommandType.Text);
            return Convert.ToInt32(result.Rows[0][0]!);
        }
        public async Task<IEnumerable<PostModels>> GetAllPostsAsync(int page, int pageSize)
        {
            if(page < 1)
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

        // 用户相关
        public async Task<int> GetUserNums()
        {
            var db = Database.getInstance();
            var query = "SELECT COUNT(*) FROM Users";
            var result = await db.ExecuteQuery(query, CommandType.Text);
            return Convert.ToInt32(result.Rows[0][0]!);
        }
        public async Task<UserModels?> GetUserByUsernameAsync(string username)
        {
            var db = Database.getInstance();
            var query = "SELECT * FROM Users WHERE Username = @Username";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Username", SqlDbType.NVarChar) { Value = username }
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            if (result.Rows.Count == 0)
            {
                return null;
            }
            var row = result.Rows[0];
            return new UserModels
            {
                Id = Convert.ToInt32(row["Id"]!),
                Username = row["Username"].ToString()!,
                Nickname = row["Nickname"].ToString()!,
                PasswordHash = row["PasswordHash"].ToString()!,
                Avatar = row["Avatar"].ToString()!,
                Email = row["Email"].ToString()!,
                Role = row["Role"].ToString()!,
                CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                Status = Convert.ToInt32(row["Status"]!),
            };
        }
        public async Task<UserModels> GetUserByEmailAsync(string email)
        {
            var db = Database.getInstance();
            var query = "SELECT * FROM Users WHERE Email = @Email";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Email", SqlDbType.NVarChar) { Value = email }
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            if (result.Rows.Count == 0)
            {
                return null;
            }
            var row = result.Rows[0];
            return new UserModels
            {
                Id = Convert.ToInt32(row["Id"]!),
                Username = row["Username"].ToString()!,
                Nickname = row["Nickname"].ToString()!,
                PasswordHash = row["PasswordHash"].ToString()!,
                Avatar = row["Avatar"].ToString()!,
                Email = row["Email"].ToString()!,
                Role = row["Role"].ToString()!,
                CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                Status = Convert.ToInt32(row["Status"]!),
            };
        }
        public async Task<IEnumerable<UserModels>> GetAllUsers(int page, int pageSize)
        {
            if (page < 1)
            {
                page = 1;
            }
            if (pageSize < 1 || pageSize > 20)
            {
                pageSize = 10;
            }
            var users = new List<UserModels>();
            var db = Database.getInstance();
            var query = "SELECT * FROM Users ORDER BY CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Offset", SqlDbType.Int) { Value = (page - 1) * pageSize },
                new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize },
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                users.Add(new UserModels
                {
                    Id = Convert.ToInt32(row["Id"]!),
                    Username = row["Username"].ToString()!,
                    Nickname = row["Nickname"].ToString()!,
                    PasswordHash = row["PasswordHash"].ToString()!,
                    Avatar = row["Avatar"].ToString()!,
                    Email = row["Email"].ToString()!,
                    Role = row["Role"].ToString()!,
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                    Status = Convert.ToInt32(row["Status"]!),
                });
            }
            return users;
        }

        public async Task<UserModels> GetUserByIdAsync(int id)
        {
            var db = Database.getInstance();
            var query = "SELECT * FROM Users WHERE Id = @Id";
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
            return new UserModels
            {
                Id = Convert.ToInt32(row["Id"]!),
                Username = row["Username"].ToString()!,
                Nickname = row["Nickname"].ToString()!,
                PasswordHash = row["PasswordHash"].ToString()!,
                Avatar = row["Avatar"].ToString()!,
                Email = row["Email"].ToString()!,
                Role = row["Role"].ToString()!,
                CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                Status = Convert.ToInt32(row["Status"]!),
            };
        }
        public async Task<int> CreateUserAsync(UserModels user)
        {
            var db = Database.getInstance();
            var query = "INSERT INTO Users (Username, Nickname, PasswordHash, Avatar, Email, Role, CreatedAt, Status) VALUES (@Username, @Nickname, @PasswordHash, @Avatar, @Email, @Role, @CreatedAt, @Status)";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Username", SqlDbType.NVarChar) { Value = user.Username },
                new SqlParameter("@Nickname", SqlDbType.NVarChar) { Value = user.Nickname },
                new SqlParameter("@PasswordHash", SqlDbType.NVarChar) { Value = user.PasswordHash },
                new SqlParameter("@Avatar", SqlDbType.NVarChar) { Value = user.Avatar },
                new SqlParameter("@Email", SqlDbType.NVarChar) { Value = user.Email },
                new SqlParameter("@Role", SqlDbType.NVarChar) { Value = user.Role },
                new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = user.CreatedAt },
                new SqlParameter("@Status", SqlDbType.Int) { Value = user.Status },
            };
            return await db.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> DeleteUserAsync(int id)
        {
            var db = Database.getInstance();
            var query = "DELETE FROM Users WHERE Id = @Id";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", SqlDbType.Int) { Value = id }
            };
            return await db.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> UpdateUserAsync(int userId, UserModels user)
        {
            var db = Database.getInstance();
            var query = "UPDATE Users SET Username = @Username, Nickname = @Nickname, PasswordHash = @PasswordHash, Avatar = @Avatar, Email = @Email, Role = @Role, CreatedAt = @CreatedAt, Status = @Status WHERE Id = @Id";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", SqlDbType.Int) { Value = userId },
                new SqlParameter("@Username", SqlDbType.NVarChar) { Value = user.Username },
                new SqlParameter("@Nickname", SqlDbType.NVarChar) { Value = user.Nickname },
                new SqlParameter("@PasswordHash", SqlDbType.NVarChar) { Value = user.PasswordHash },
                new SqlParameter("@Avatar", SqlDbType.NVarChar) { Value = user.Avatar },
                new SqlParameter("@Email", SqlDbType.NVarChar) { Value = user.Email },
                new SqlParameter("@Role", SqlDbType.NVarChar) { Value = user.Role },
                new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = user.CreatedAt },
                new SqlParameter("@Status", SqlDbType.Int) { Value = user.Status },
            };
            return await db.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        // 登陆相关
        public async Task<UserModels> LoginAsync(string username, string password)
        {
            var db = Database.getInstance();
            var query = "SELECT * FROM Users WHERE Username = @Username AND PasswordHash = @PasswordHash";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Username", SqlDbType.NVarChar) { Value = username },
                new SqlParameter("@PasswordHash", SqlDbType.NVarChar) { Value = password },
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            if (result.Rows.Count == 0)
            {
                return null;
            }
            var row = result.Rows[0];
            return new UserModels
            {
                Id = Convert.ToInt32(row["Id"]!),
                Username = row["Username"].ToString()!,
                Nickname = row["Nickname"].ToString()!,
                PasswordHash = row["PasswordHash"].ToString()!,
                Avatar = row["Avatar"].ToString()!,
                Email = row["Email"].ToString()!,
                Role = row["Role"].ToString()!,
                CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
            };
        }
        public async Task<UserModels> LogoutAsync()
        {
            return null;
        }
        public async Task<UserModels> RegisterAsync(UserModels user)
        {
            var db = Database.getInstance();
            var query = "INSERT INTO Users (Username, Nickname, PasswordHash, Avatar, Email, Role, CreatedAt, Status) VALUES (@Username, @Nickname, @PasswordHash, @Avatar, @Email, @Role, @CreatedAt, @Status)";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Username", SqlDbType.NVarChar) { Value = user.Username },
                new SqlParameter("@Nickname", SqlDbType.NVarChar) { Value = user.Nickname },
                new SqlParameter("@PasswordHash", SqlDbType.NVarChar) { Value = user.PasswordHash },
                new SqlParameter("@Avatar", SqlDbType.NVarChar) { Value = user.Avatar },
                new SqlParameter("@Email", SqlDbType.NVarChar) { Value = user.Email },
                new SqlParameter("@Role", SqlDbType.NVarChar) { Value = user.Role },
                new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = user.CreatedAt },
                new SqlParameter("@Status", SqlDbType.Int) { Value = user.Status },
            };
            await db.ExecuteNonQuery(query, CommandType.Text, parameters);
            return user;
        }

        // 评论相关
        public async Task<IEnumerable<CommentModels>> GetCommentsByPostIdAsync(int postId)
        {
            var comments = new List<CommentModels>();
            var db = Database.getInstance();
            var query = "SELECT * FROM Comments WHERE PostId = @PostId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@PostId", SqlDbType.Int) { Value = postId }
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                comments.Add(new CommentModels
                {
                    Id = Convert.ToInt32(row["Id"]!),
                    PostId = Convert.ToInt32(row["PostId"]!),
                    UserId = Convert.ToInt32(row["UserId"]!),
                    Content = row["Content"].ToString()!,
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                });
            }
            return comments;
        }
        public async Task<CommentModels> GetCommentByIdAsync(int id)
        {
            var db = Database.getInstance();
            var query = "SELECT * FROM Comments WHERE Id = @Id";
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
            return new CommentModels
            {
                Id = Convert.ToInt32(row["Id"]!),
                PostId = Convert.ToInt32(row["PostId"]!),
                UserId = Convert.ToInt32(row["UserId"]!),
                Content = row["Content"].ToString()!,
                CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
            };
        }
        public async Task<int> CreateCommentAsync(CommentModels comment)
        {
            var db = Database.getInstance();
            var query = "INSERT INTO Comments (PostId, UserId, Content, CreatedAt) VALUES (@PostId, @UserId, @Content, @CreatedAt)";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@PostId", SqlDbType.Int) { Value = comment.PostId },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = comment.UserId },
                new SqlParameter("@Content", SqlDbType.NVarChar) { Value = comment.Content },
                new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = comment.CreatedAt },
            };
            return await db.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
        public async Task<int> DeleteCommentAsync(int id)
        {
            var db = Database.getInstance();
            var query = "DELETE FROM Comments WHERE Id = @Id";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", SqlDbType.Int) { Value = id }
            };
            return await db.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        // 图床相关
        public async Task<IEnumerable<ImageModels>> GetAllImagesAsync(int page, int pageSize)
        {
            if (page < 1)
            {
                page = 1;
            }
            if (pageSize < 1 || pageSize > 20)
            {
                pageSize = 10;
            }
            var images = new List<ImageModels>();
            var db = Database.getInstance();
            var query = "SELECT * FROM Images ORDER BY CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Offset", SqlDbType.Int) { Value = (page - 1) * pageSize },
                new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize },
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                images.Add(new ImageModels
                {
                    Id = Convert.ToInt32(row["Id"]!),
                    Url = row["Url"].ToString()!,
                    PostId = Convert.ToInt32(row["PostId"]!),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                });
            }
            return images;
        }
        public async Task<ImageModels> GetImageByIdAsync(int id)
        {
            var db = Database.getInstance();
            var query = "SELECT * FROM Images WHERE Id = @Id";
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
            return new ImageModels
            {
                Id = Convert.ToInt32(row["Id"]!),
                Url = row["Url"].ToString()!,
                PostId = Convert.ToInt32(row["PostId"]!),
                CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
            };
        }
        public async Task<IEnumerable<ImageModels>> GetImagesByPostId(int postId)
        {
            var images = new List<ImageModels>();
            var db = Database.getInstance();
            var query = "SELECT * FROM Images WHERE PostId = @PostId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@PostId", SqlDbType.Int) { Value = postId }
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                images.Add(new ImageModels
                {
                    Id = Convert.ToInt32(row["Id"]!),
                    Url = row["Url"].ToString()!,
                    PostId = Convert.ToInt32(row["PostId"]!),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                });
            }
            return images;
        }
        public async Task<string> UploadImageAsync(string base64)
        {
            string guid = Guid.NewGuid().ToString("N");
            var db = Database.getInstance();
            if (!Directory.Exists("wwwroot/images"))
            {
                Directory.CreateDirectory("wwwroot/images");
            }
            if (!Directory.Exists("wwwroot/images/" + DateTime.Now.ToString("yyyy/MM/dd")))
            {
                Directory.CreateDirectory("wwwroot/images/" + DateTime.Now.ToString("yyyy/MM/dd"));
            }
            var path = "wwwroot/images/" + DateTime.Now.ToString("yyyy/MM/dd") + "/" + guid + ".png";
            File.WriteAllBytes(path, Convert.FromBase64String(base64));
            var query = "INSERT INTO Images (Url, PostId, CreatedAt) VALUES (@Url, @PostId, @CreatedAt)";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Url", SqlDbType.NVarChar) { Value = path.Replace("wwwroot",string.Empty) },
                new SqlParameter("@PostId", SqlDbType.Int) { Value = 0 },
                new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = DateTime.Now },
            };
            await db.ExecuteNonQuery(query, CommandType.Text, parameters);
            return base64;
        }

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
        public async Task<IEnumerable<PostCategoryModels>> GetPostCategoriesByPostIdAsync(int postId)
        {
            var categories = new List<PostCategoryModels>();
            var db = Database.getInstance();
            var query = "SELECT * FROM PostCategories WHERE PostId = @PostId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@PostId", SqlDbType.Int) { Value = postId }
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                categories.Add(new PostCategoryModels
                {
                    PostId = Convert.ToInt32(row["PostId"]!),
                    CategoryId = Convert.ToInt32(row["CategoryId"]!),
                });
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

        // 点赞相关
        public async Task<IEnumerable<LikeModels>> GetPostLikesByPostIdAsync(int postId)
        {
            var likes = new List<LikeModels>();
            var db = Database.getInstance();
            var query = "SELECT * FROM Likes WHERE PostId = @PostId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@PostId", SqlDbType.Int) { Value = postId }
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                likes.Add(new LikeModels
                {
                    Id = Convert.ToInt32(row["Id"]!),
                    PostId = Convert.ToInt32(row["PostId"]!),
                    UserId = Convert.ToInt32(row["UserId"]!),
                    CommentId = Convert.ToInt32(row["CommentId"]!),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                });
            }
            return likes;
        }
        public async Task<int> GetPostLikeNumsByPostIdAsync(int postId)
        {
            var db = Database.getInstance();
            var query = "SELECT COUNT(*) FROM Likes WHERE PostId = @PostId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@PostId", SqlDbType.Int) { Value = postId }
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            return Convert.ToInt32(result.Rows[0][0]!);
        }
        public async Task<int> CreatePostLikeAsync(LikeModels postLike)
        {
            var db = Database.getInstance();
            // 检查是否已经点赞
            var query = "SELECT * FROM Likes WHERE PostId = @PostId AND UserId = @UserId AND CommentId = @CommentId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@PostId", SqlDbType.Int) { Value = postLike.PostId },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = postLike.UserId },
                new SqlParameter("@CommentId", SqlDbType.Int) { Value = postLike.CommentId },
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            if (result.Rows.Count > 0)
            {
                return 0;
            }
            // 添加点赞记录
            query = "INSERT INTO Likes (PostId, UserId, CommentId, CreatedAt) VALUES (@PostId, @UserId, @CommentId, @CreatedAt)";
            parameters = new SqlParameter[]
            {
                new SqlParameter("@PostId", SqlDbType.Int) { Value = postLike.PostId },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = postLike.UserId },
                new SqlParameter("@CommentId", SqlDbType.Int) { Value = postLike.CommentId },
                new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = postLike.CreatedAt },
            };
            return await db.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
        public async Task<int> DeletePostLikeAsync(LikeModels postLike)
        {
            var db = Database.getInstance();
            var query = "DELETE FROM Likes WHERE PostId = @PostId AND UserId = @UserId AND CommentId = @CommentId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@PostId", SqlDbType.Int) { Value = postLike.PostId },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = postLike.UserId },
                new SqlParameter("@CommentId", SqlDbType.Int) { Value = postLike.CommentId },
            };
            return await db.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        // 收藏相关
        public async Task<IEnumerable<int>> GetPostFavoriteByUserId(int userId, int page, int pagesize)
        {
            var favorites = new List<int>();
            var db = Database.getInstance();
            var query = "SELECT * FROM Favorites WHERE UserId = @UserId ORDER BY CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@Offset", SqlDbType.Int) { Value = (page - 1) * pagesize },
                new SqlParameter("@PageSize", SqlDbType.Int) { Value = pagesize },
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                favorites.Add(Convert.ToInt32(row["PostId"]!));
            }
            return favorites;
        }
        public async Task<int> CreatePostFavoriteAsync(int userId, int postId)
        {
            var db = Database.getInstance();
            // 检查是否已经收藏
            var query = "SELECT * FROM Favorites WHERE UserId = @UserId AND PostId = @PostId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@PostId", SqlDbType.Int) { Value = postId },
            };
            var result = await db.ExecuteQuery(query, CommandType.Text, parameters);
            if (result.Rows.Count > 0)
            {
                return 0;
            }
            // 添加收藏记录
            query = "INSERT INTO Favorites (UserId, PostId) VALUES (@UserId, @PostId)";
            parameters = new SqlParameter[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@PostId", SqlDbType.Int) { Value = postId },
            };
            return await db.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
    }
}
