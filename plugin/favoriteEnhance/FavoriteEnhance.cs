using imarket.Controllers.open;
using imarket.models;
using imarket.service.IService;
using imarket.service.Service;

namespace imarket.plugin.favoriteEnhance
{
    [PluginTag(Name ="favoriteEnhance plugin",Description ="用于增强用户获取收藏的帖子",Enable = true,Author ="jeanhua")]
    public class FavoriteEnhance:IPluginInterceptor
    {
        private readonly IPostService postService;
        private readonly IUserService userService;
        private readonly IFavoriteService favoriteService;
        private readonly ILikeService likeService;
        public FavoriteEnhance(IPostService postService, IUserService userService,IFavoriteService favoriteService,ILikeService likeService)
        {
            this.postService = postService;
            this.userService = userService;
            this.favoriteService = favoriteService;
            this.likeService = likeService;
        }
        public async Task<(bool op, object? result)> OnBeforeExecutionAsync(string route, object?[] args, string? username = null)
        {
            return (false, null);
        }
        public async Task<(bool op, object? result)> OnAfterExecutionAsync(string route, object? result, string? username = null)
        {
            if(route== "/api/Post/GetFavorites")
            {
                var favorite = (result as dynamic).favorite as List<dynamic>;
                var postList = new List<PostsResponse>();
                foreach(var post in favorite)
                {
                    var postId =Convert.ToUInt64(post.PostId);
                    var postModel = (await postService.GetPostByIdAsync(postId)) as PostModels;
                    var user = await userService.GetUserByIdAsync(postModel.UserId);
                    postList.Add(new PostsResponse
                    {
                        Id = postModel.Id,
                        Content = postModel.Content,
                        Title = postModel.Title,
                        CreatedAt = postModel.CreatedAt,
                        Avatar = user.Avatar,
                        FavoriteNums = await favoriteService.GetFavoriteNumsByPostIdAsync(postModel.Id),
                        LikeNums = await likeService.GetPostLikeNumsByPostIdAsync(postModel.Id),
                        Nickname = user.Nickname
                    });
                }
                return (true, new
                {
                    success = true,
                    posts = postList
                });
            }
            return (false, null);
        }

        public void RegisterRoutes(IEndpointRouteBuilder endpoints)
        {
        }
    }
}
