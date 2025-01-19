using imarket.models;

namespace imarket.service.IService
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentModels>> GetCommentsByPostIdAsync(string postId);
        Task<int> CreateCommentAsync(CommentModels comment);
        Task<int> DeleteCommentAsync(string id);
        Task<CommentModels?> GetCommentByIdAsync(string id);
        Task<int> GetCommentsNumByPostId(string postId);
    }
}
