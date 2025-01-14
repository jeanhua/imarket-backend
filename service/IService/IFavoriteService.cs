namespace imarket.service.IService
{
    public interface IFavoriteService
    {
        Task<IEnumerable<int>> GetPostFavoriteByUserId(string userId, int page, int pagesize);
        Task<int> CreatePostFavoriteAsync(string postId, string userId);
    }
}
