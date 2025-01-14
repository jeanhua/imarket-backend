namespace imarket.service.IService
{
    public interface IFavoriteService
    {
        Task<IEnumerable<int>> GetPostFavoriteByUserId(int userId, int page, int pagesize);
        Task<int> CreatePostFavoriteAsync(int postId, int userId);
    }
}
