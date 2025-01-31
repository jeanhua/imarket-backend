using imarket.models;

namespace imarket.service.IService
{
    public interface IPostCategoriesService
    {
        // 帖子分类相关
        Task<IEnumerable<CategoryModels>> GetAllCategoriesAsync();
        Task<CategoryModels?> GetCategoryByIdAsync(ulong id);
        Task<int> CreateCategoryAsync(CategoryModels category);
        Task<int> UpdateCategoryAsync(ulong categoryId, CategoryModels category);
        Task<int> DeleteCategoryAsync(ulong id);

        // 帖子分类关联相关
        Task<string?> GetPostCategoriesByPostIdAsync(ulong postId);
        Task<int> CreatePostCategoryAsync(PostCategoryModels postCategory);
        Task<int> DeletePostCategoryAsync(PostCategoryModels postCategory);
        Task<int> DeletePostCategoryByPostIdAsync(ulong postId);
    }
}
