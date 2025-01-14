using imarket.models;
using imarket.service.IService;
using imarket.utils;
using Microsoft.Data.SqlClient;
using System.Data;
namespace imarket.service.Service
{
    public class ImageService:IImageService
    {
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
    }
}
