using imarket.models;

namespace imarket.service.IService
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(string base64);
        Task<IEnumerable<ImageModels>> GetAllImagesAsync(int page, int pageSize);
        Task<ImageModels> GetImageByIdAsync(int id);
        Task<IEnumerable<ImageModels>> GetImagesByPostId(int postId);
    }
}
