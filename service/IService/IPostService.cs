﻿using imarket.models;

namespace imarket.service.IService
{
    public interface IPostService
    {
        Task<int> GetPostNums();
        Task<IEnumerable<PostModels>> GetAllPostsAsync(int page, int pageSize);
        Task<IEnumerable<PostModels>> GetPostsByUserIdAsync(string userId, int page, int pageSize);
        Task<IEnumerable<PostModels>> GetAllPostsByUserIdAsync(string userId);
        Task<IEnumerable<PostModels>> GetPostsByCategoryIdAsync(string categoryId, int page, int pageSize);
        Task<IEnumerable<PostModels>> SearchPostsAsync(string keyWord, int page, int pageSize);
        Task<PostModels?> GetPostByIdAsync(string id);
        Task<int> CreatePostAsync(PostModels post);
        Task<int> UpdatePostAsync(PostModels post);
        Task<int> DeletePostAsync(string id);
        Task<int> DeletePostByUserIdAsync(string userId);
    }
}
