using imarket.models;

namespace imarket.service.IService
{
    public interface IPostCategoriesService
    {
        // 帖子分类相关
        Task<IEnumerable<CategoryModels>> GetAllCategoriesAsync();
        Task<CategoryModels> GetCategoryByIdAsync(string id);
        Task<int> CreateCategoryAsync(CategoryModels category);
        Task<int> UpdateCategoryAsync(string categoryId, CategoryModels category);
        Task<int> DeleteCategoryAsync(string id);

        // 帖子分类关联相关
        Task<IEnumerable<string>> GetPostCategoriesByPostIdAsync(string postId);
        Task<PostCategoryModels> CreatePostCategoryAsync(PostCategoryModels postCategory);
        Task<int> DeletePostCategoryAsync(PostCategoryModels postCategory);
    }
}
