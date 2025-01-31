using imarket.models;

namespace imarket.service.IService
{
    public interface IPostService
    {
        Task<int> GetPostNums();
        Task<IEnumerable<PostModels>> GetAllPostsAsync(int page, int pageSize);
        Task<IEnumerable<PostModels>> GetPostsByUserIdAsync(ulong userId, int page, int pageSize);
        Task<IEnumerable<PostModels>> GetAllPostsByUserIdAsync(ulong userId);
        Task<IEnumerable<PostModels>> GetPostsByCategoryIdAsync(ulong categoryId, int page, int pageSize);
        Task<IEnumerable<PostModels>> SearchPostsAsync(string keyWord, int page, int pageSize);
        Task<PostModels?> GetPostByIdAsync(ulong id);
        Task<ulong> CreatePostAsync(PostModels post);
        Task<int> UpdatePostAsync(PostModels post);
        Task<int> DeletePostAsync(ulong id);
        Task<int> DeletePostByUserIdAsync(ulong userId);
    }
}
