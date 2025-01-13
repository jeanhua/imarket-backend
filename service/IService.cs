using imarket.models;
namespace imarket.service
{
    public interface IService
    {
        // 帖子相关
        Task<int> GetPostNums();
        Task<IEnumerable<PostModels>> GetAllPostsAsync(int page, int pageSize);
        Task<IEnumerable<PostModels>> GetPostsByUserIdAsync(int userId);
        Task<IEnumerable<PostModels>> GetPostsByCategoryIdAsync(int categoryId,int page,int pageSize);
        Task<PostModels> GetPostByIdAsync(int id);
        Task<int> CreatePostAsync(PostModels post);
        Task<int> DeletePostAsync(int id);

        // 用户相关
        Task<int> GetUserNums();
        Task<UserModels?> GetUserByUsernameAsync(string username);
        Task<UserModels?> GetUserByEmailAsync(string email);
        Task<IEnumerable<UserModels>> GetAllUsers(int page, int pageSize);
        Task<UserModels> GetUserByIdAsync(int id);
        Task<int> CreateUserAsync(UserModels user);
        Task<int> UpdateUserAsync(int userId, UserModels user);
        Task<int> DeleteUserAsync(int id);

        // 登录相关
        Task<UserModels> LoginAsync(string username, string password);
        Task<UserModels> LogoutAsync();
        Task<UserModels> RegisterAsync(UserModels user);

        // 评论相关
        Task<IEnumerable<CommentModels>> GetCommentsByPostIdAsync(int postId);
        Task<int> CreateCommentAsync(CommentModels comment);
        Task<int> DeleteCommentAsync(int id);
        Task<CommentModels> GetCommentByIdAsync(int id);

        // 图床相关
        Task<string> UploadImageAsync(string base64);
        Task<IEnumerable<ImageModels>> GetAllImagesAsync(int page,int pageSize);
        Task<ImageModels> GetImageByIdAsync(int id);
        Task<IEnumerable<ImageModels>> GetImagesByPostId(int postId);

        // 帖子分类相关
        Task<IEnumerable<CategoryModels>> GetAllCategoriesAsync();
        Task<CategoryModels> GetCategoryByIdAsync(int id);
        Task<int> CreateCategoryAsync(CategoryModels category);
        Task<int> UpdateCategoryAsync(int categoryId, CategoryModels category);
        Task<int> DeleteCategoryAsync(int id);

        // 帖子分类关联相关
        Task<IEnumerable<PostCategoryModels>> GetPostCategoriesByPostIdAsync(int postId);
        Task<PostCategoryModels> CreatePostCategoryAsync(PostCategoryModels postCategory);
        Task<int> DeletePostCategoryAsync(PostCategoryModels postCategory);

        // 点赞相关
        Task<IEnumerable<LikeModels>> GetPostLikesByPostIdAsync(int postId);
        Task<int> GetPostLikeNumsByPostIdAsync(int postId);
        Task<int> CreatePostLikeAsync(LikeModels postLike);
        Task<int> DeletePostLikeAsync(LikeModels postLike);

        // 收藏相关
        Task<IEnumerable<int>> GetPostFavoriteByUserId(int userId,int page,int pagesize);
        Task<int> CreatePostFavoriteAsync(int postId,int userId);
    }
}
