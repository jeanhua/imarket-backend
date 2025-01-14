using imarket.models;

namespace imarket.service.IService
{
    public interface IPostCategoriesService
    {
        // 帖子分类相关
        Task<IEnumerable<CategoryModels>> GetAllCategoriesAsync();
        Task<CategoryModels> GetCategoryByIdAsync(int id);
        Task<int> CreateCategoryAsync(CategoryModels category);
        Task<int> UpdateCategoryAsync(int categoryId, CategoryModels category);
        Task<int> DeleteCategoryAsync(int id);

        // 帖子分类关联相关
        Task<IEnumerable<int>> GetPostCategoriesByPostIdAsync(int postId);
        Task<PostCategoryModels> CreatePostCategoryAsync(PostCategoryModels postCategory);
        Task<int> DeletePostCategoryAsync(PostCategoryModels postCategory);
    }
}
