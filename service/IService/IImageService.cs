using imarket.models;

namespace imarket.service.IService
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(string base64);
        Task<int> SaveImageAsync(ImageModels image);
        Task<IEnumerable<ImageModels>> GetAllImagesAsync(int page, int pageSize);
        Task<ImageModels?> GetImageByIdAsync(ulong id);
        Task<IEnumerable<ImageModels>> GetImagesByPostId(ulong postId);
        Task<int> DeleteImageByIdAsync(ulong id);
        Task<int> DeleteImagesByPostIdAsync(ulong postId);
    }
}
