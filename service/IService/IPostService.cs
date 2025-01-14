using imarket.models;

namespace imarket.service.IService
{
    public interface IPostService
    {
        Task<int> GetPostNums();
        Task<IEnumerable<PostModels>> GetAllPostsAsync(int page, int pageSize);
        Task<IEnumerable<PostModels>> GetPostsByUserIdAsync(int userId);
        Task<IEnumerable<PostModels>> GetPostsByCategoryIdAsync(int categoryId, int page, int pageSize);
        Task<PostModels> GetPostByIdAsync(int id);
        Task<int> CreatePostAsync(PostModels post);
        Task<int> DeletePostAsync(int id);
    }
}
