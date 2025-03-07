﻿using imarket.models;
using imarket.service.IService;
using imarket.utils;
using MySql.Data.MySqlClient;
using System.Data;
namespace imarket.service.Service
{
    public class PostService : IPostService
    {
        private readonly Database _database;
        private readonly ILikeService _likeService;
        private readonly ICommentService _commentService;
        private readonly IImageService _imageService;
        private readonly IFavoriteService _favoriteService;
        private readonly IPostCategoriesService _postCategoriesService;
        public PostService(Database database, ILikeService likeService, ICommentService commentService, IImageService imageService, IFavoriteService favoriteService, IPostCategoriesService postCategoriesService)
        {
            _database = database;
            _likeService = likeService;
            _commentService = commentService;
            _imageService = imageService;
            _favoriteService = favoriteService;
            _postCategoriesService = postCategoriesService;
        }
        public async Task<int> GetPostNums()
        {
            var query = "SELECT COUNT(*) FROM Posts";
            var result = await _database.ExecuteQuery(query, CommandType.Text);
            return Convert.ToInt32(result.Rows[0][0]!);
        }
        public async Task<IEnumerable<PostModels>> GetAllPostsAsync(int page, int pageSize)
        {
            if (page < 1 || pageSize < 1)
            {
                page = 1;
                pageSize = 10;
            }
            if (pageSize < 1 || pageSize > 20)
            {
                pageSize = 10;
            }
            var posts = new List<PostModels>();
            var query = "SELECT * FROM Posts ORDER BY CreatedAt DESC LIMIT @PageSize OFFSET @Offset";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Offset", (page - 1) * pageSize),
                new MySqlParameter("@PageSize", pageSize),
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                posts.Add(new PostModels
                {
                    Id = ulong.Parse(row["Id"].ToString()!),
                    Title = row["Title"].ToString()!,
                    Content = row["Content"].ToString()!,
                    UserId = ulong.Parse(row["UserId"].ToString()!),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                    Status = Convert.ToInt32(row["Status"]!),
                });
            }
            return posts;
        }
        public async Task<IEnumerable<PostModels>> GetPostsByUserIdAsync(ulong userId, int page, int pageSize)
        {
            if (page < 1 || pageSize < 1)
            {
                page = 1;
                pageSize = 10;
            }
            if (pageSize < 1 || pageSize > 20)
            {
                pageSize = 10;
            }
            var posts = new List<PostModels>();
            var query = "SELECT * FROM Posts WHERE UserId = @UserId ORDER BY CreatedAt DESC LIMIT @PageSize OFFSET @Offset";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@UserId", userId),
                new MySqlParameter("@Offset", (page - 1) * pageSize),
                new MySqlParameter("@PageSize", pageSize),
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                posts.Add(new PostModels
                {
                    Id = ulong.Parse(row["Id"].ToString()!),
                    Title = row["Title"].ToString()!,
                    Content = row["Content"].ToString()!,
                    UserId = ulong.Parse(row["UserId"].ToString()!),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                    Status = Convert.ToInt32(row["Status"]!),
                });
            }
            return posts;
        }

        public async Task<IEnumerable<PostModels>> GetAllPostsByUserIdAsync(ulong userId)
        {
            var posts = new List<PostModels>();
            var query = "SELECT * FROM Posts WHERE UserId = @UserId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@UserId", userId),
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                posts.Add(new PostModels
                {
                    Id = ulong.Parse(row["Id"].ToString()!),
                    Title = row["Title"].ToString()!,
                    Content = row["Content"].ToString()!,
                    UserId = ulong.Parse(row["UserId"].ToString()!),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                    Status = Convert.ToInt32(row["Status"]!),
                });
            }
            return posts;
        }
        public async Task<IEnumerable<PostModels>> GetPostsByCategoryIdAsync(ulong categoryId, int page, int pageSize)
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
            var query = @"
                SELECT * FROM Posts 
                WHERE Id IN (SELECT PostId FROM PostCategories WHERE CategoryId = @CategoryId) 
                ORDER BY CreatedAt DESC 
                LIMIT @PageSize OFFSET @Offset";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@CategoryId", categoryId),
                new MySqlParameter("@Offset", (page - 1) * pageSize),
                new MySqlParameter("@PageSize", pageSize),
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                posts.Add(new PostModels
                {
                    Id = ulong.Parse(row["Id"].ToString()!),
                    Title = row["Title"].ToString()!,
                    Content = row["Content"].ToString()!,
                    UserId = ulong.Parse(row["UserId"].ToString()!),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                    Status = Convert.ToInt32(row["Status"]!),
                });
            }
            return posts;
        }

        public async Task<IEnumerable<PostModels>> SearchPostsAsync(string keyWord, int page, int pageSize)
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
            var query = @"
                SELECT * FROM Posts 
                WHERE Title LIKE @KeyWord OR Content LIKE @KeyWord 
                ORDER BY CreatedAt DESC 
                LIMIT @PageSize OFFSET @Offset";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@KeyWord", $"%{keyWord}%"),
                new MySqlParameter("@Offset", (page - 1) * pageSize),
                new MySqlParameter("@PageSize", pageSize),
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                posts.Add(new PostModels
                {
                    Id = ulong.Parse(row["Id"].ToString()!),
                    Title = row["Title"].ToString()!,
                    Content = row["Content"].ToString()!,
                    UserId = ulong.Parse(row["UserId"].ToString()!),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                    Status = Convert.ToInt32(row["Status"]!),
                });
            }
            return posts;
        }


        public async Task<PostModels?> GetPostByIdAsync(ulong id)
        {
            var query = "SELECT * FROM Posts WHERE Id = @Id";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", id )
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            if (result.Rows.Count == 0)
            {
                return null;
            }
            var row = result.Rows[0];
            return new PostModels
            {
                Id = ulong.Parse(row["Id"].ToString()!),
                Status = int.Parse(row["Status"].ToString()!),
                Title = row["Title"].ToString()!,
                Content = row["Content"].ToString()!,
                UserId = ulong.Parse(row["UserId"].ToString()!),
                CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
            };
        }
        public async Task<ulong> CreatePostAsync(PostModels post)
        {
            var query = "INSERT INTO Posts (Title, Content, UserId, CreatedAt, Status) VALUES (@Title, @Content, @UserId, @CreatedAt, @Status);SELECT LAST_INSERT_ID();";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Title", post.Title),
                new MySqlParameter("@Content", post.Content),
                new MySqlParameter("@UserId", post.UserId),
                new MySqlParameter("@CreatedAt", post.CreatedAt),
                new MySqlParameter("@Status", post.Status),
            };
            return await _database.ExecuteNonQueryWithId(query, CommandType.Text, parameters);
        }

        public async Task<int> DeletePostAsync(ulong id)
        {
            await _likeService.DeleteLikesByPostIdAsync(id);
            await _commentService.DeleteCommentsByPostIdAsync(id);
            await _imageService.DeleteImagesByPostIdAsync(id);
            await _favoriteService.DeletePostFavoriteByPostIdAsyc(id);
            await _postCategoriesService.DeletePostCategoryByPostIdAsync(id);
            var query = "DELETE FROM Posts WHERE Id = @Id";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id",id)
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> DeletePostByUserIdAsync(ulong userId)
        {
            var posts = await GetAllPostsByUserIdAsync(userId);
            var result = 0;
            foreach(var post in posts)
            {
                result += await DeletePostAsync(post.Id);
            }
            return result;
        }

        public async Task<int> UpdatePostAsync(PostModels post)
        {
            var query = "UPDATE Posts SET Title = @Title, Content = @Content, UserId = @UserId, CreatedAt = @CreatedAt, Status = @Status WHERE Id = @Id";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", post.Id),
                new MySqlParameter("@Title", post.Title),
                new MySqlParameter("@Content", post.Content),
                new MySqlParameter("@UserId", post.UserId),
                new MySqlParameter("@CreatedAt", post.CreatedAt),
                new MySqlParameter("@Status", post.Status),
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
    }
}