using imarket.models;

namespace imarket.service.IService
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(string base64);
        Task<int> SaveImageAsync(ImageModels image);
        Task<IEnumerable<ImageModels>> GetAllImagesAsync(int page, int pageSize);
        Task<ImageModels?> GetImageByIdAsync(string id);
        Task<IEnumerable<ImageModels>> GetImagesByPostId(string postId);
        Task<int> DeleteImageByIdAsync(string id);
        Task<int> DeleteImagesByPostIdAsync(string postId);
    }
}
