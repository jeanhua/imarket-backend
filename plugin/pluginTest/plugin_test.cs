using imarket.plugin;
// 引入依赖接口
using imarket.service.IService;
namespace plugin_test
{
    [PluginTag(Name = "plugin_test", Description = "A plugin for testing", Author = "Your Name")]
    public class plugin_test: IPluginInterceptor
    {
        private readonly IUserService service;
        // 在构造函数中注入依赖
        public plugin_test(IUserService service)
        {
            this.service = service;
        }
        public async Task<(bool op, object? result)> OnBeforeExecutionAsync(string route, object?[] args, string? username = null)
        {
            // 如果请求的路由是获取用户信息的路由
            if (route == "/api/Account/Info" && !string.IsNullOrEmpty(username))
            {
                var user = await service.GetUserByUsernameAsync(username);
                var response = new
                {
                    success = true,
                    account = new
                    {
                        username = user.Username,
                        nickname = user.Nickname,
                        avatar = user.Avatar,
                        email = user.Email,
                        status = user.Status,
                        from = "plugin_test"
                    }
                };
                return (true,new
                {
                    success = true,
                    info = response
                });
            }
            return (false,null);
        }
        public async Task<(bool op, object? result)> OnAfterExecutionAsync(string route, object? result, string? username = null)
        {
            return (false, null);
        }
    }
}
