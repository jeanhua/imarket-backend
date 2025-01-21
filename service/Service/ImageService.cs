using imarket.models;
using imarket.service.IService;
using imarket.utils;
using MySql.Data.MySqlClient;
using System.Data;
namespace imarket.service.Service
{
    public class ImageService : IImageService
    {
        private readonly Database _database;
        private readonly ILogger<ImageService> _logger;
        public ImageService(Database database, ILogger<ImageService> _logger)
        {
            _database = database;
            this._logger = _logger;
        }
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
            int offset = (page - 1) * pageSize;
            var query = @"
                SELECT * FROM Images 
                ORDER BY CreatedAt DESC 
                LIMIT @PageSize OFFSET @Offset;
            ";
            var parameters = new[]
            {
                new MySqlParameter("@Offset", offset),
                new MySqlParameter("@PageSize", pageSize)
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            var images = new List<ImageModels>();
            foreach (DataRow row in result.Rows)
            {
                images.Add(new ImageModels
                {
                    Id = row["Id"].ToString()!,
                    Url = row["Url"].ToString()!,
                    PostId = row["PostId"].ToString()!,
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                });
            }
            return images;
        }
        public async Task<ImageModels?> GetImageByIdAsync(string id)
        {
            var query = "SELECT * FROM Images WHERE Id = @Id";
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
            return new ImageModels
            {
                Id = row["Id"].ToString()!,
                Url = row["Url"].ToString()!,
                PostId = row["PostId"].ToString()!,
                CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
            };
        }
        public async Task<IEnumerable<ImageModels>> GetImagesByPostId(string postId)
        {
            var images = new List<ImageModels>();
            var query = "SELECT * FROM Images WHERE PostId = @PostId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@PostId", postId)
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                images.Add(new ImageModels
                {
                    Id = row["Id"].ToString()!,
                    Url = row["Url"].ToString()!,
                    PostId = row["PostId"].ToString()!,
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                });
            }
            return images;
        }
        public async Task<string> UploadImageAsync(string base64)
        {
            string guid = Guid.NewGuid().ToString("N");
            if (!Directory.Exists("wwwroot/images"))
            {
                Directory.CreateDirectory("wwwroot/images");
            }
            if (!Directory.Exists("wwwroot/images/" + DateTime.Now.ToString("yyyy/MM/dd")))
            {
                Directory.CreateDirectory("wwwroot/images/" + DateTime.Now.ToString("yyyy/MM/dd"));
            }
            var path = "wwwroot/images/" + DateTime.Now.ToString("yyyy/MM/dd") + "/" + guid + ".png";
            if (File.Exists(path))
            {
                guid = Guid.NewGuid().ToString("N");
                path = "wwwroot/images/" + DateTime.Now.ToString("yyyy/MM/dd") + "/" + guid + ".png";
            }
            await File.WriteAllBytesAsync(path, Convert.FromBase64String(base64));
            return path.Replace("wwwroot", string.Empty);
        }

        public async Task<int> SaveImageAsync(ImageModels image)
        {
            var query = "INSERT INTO Images (Id, Url, PostId, CreatedAt) VALUES (@Id, @Url, @PostId, @CreatedAt)";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", image.Id),
                new MySqlParameter("@Url", image.Url),
                new MySqlParameter("@PostId", image.PostId),
                new MySqlParameter("@CreatedAt", image.CreatedAt),
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> DeleteImageByIdAsync(string id)
        {
            var query = "DELETE FROM Images WHERE Id = @Id";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", id)
            };
            var image = await GetImageByIdAsync(id);
            try
            {
                File.Delete($"wwwroot{image.Url}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteImageByIdAsync {image.Url} error");
                System.IO.File.AppendAllText("log.txt", $"DeleteImageByIdAsync {image.Url} error");
            }
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
        public async Task<int> DeleteImagesByPostIdAsync(string postId)
        {
            var images = await GetImagesByPostId(postId);
            foreach (var image in images)
            {
                try
                {
                    File.Delete($"wwwroot{image.Url}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"DeleteImageByIdAsync {image.Url} error");
                    System.IO.File.AppendAllText("log.txt", $"DeleteImageByIdAsync {image.Url} error");
                }
            }
            var query = "DELETE FROM Images WHERE PostId = @PostId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@PostId", postId)
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
    }
}
