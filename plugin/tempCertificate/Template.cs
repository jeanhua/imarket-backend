using imarket.service.IService;
using System.Text;
using System.Text.Unicode;

namespace imarket.plugin.tempCertificate
{
    [PluginTag(Name = "tempCertificate", Description = "临时认证脚本", Author = "jeanhua", Enable = true)]
    public class TempCertificate : IPluginInterceptor
    {
        private readonly IUserService userService;
        public TempCertificate(IUserService userService)
        {
            this.userService = userService;
        }
        public async Task<(bool op, object? result)> OnBeforeExecutionAsync(string route, object?[] args, string? username = null)
        {
            return (false, null);
        }
        public async Task<(bool op, object? result)> OnAfterExecutionAsync(string route, object? result, string? username = null)
        {
            return (false, null);
        }

        public void RegisterRoutes(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("Certificate", (context) =>
            {
                // 返回简单html页面
                return context.Response.WriteAsync(@"<html>
<head>
    <meta charset=""UTF-8"">
    <title>认证</title>
</head>
<body>
    <form action='/api/Certificate' method='get'>
        <input type='text' name='username' placeholder='用户名'>
        <input type='text' name='code' placeholder='认证码'>
        <input type='submit' value='提交'>
    </form>
</body>
</html>");

            });
            endpoints.MapGet("/api/Certificate", async (context) =>
            {
                var code = context.Request.Query["code"];
                var username = context.Request.Query["username"];
                var user = await userService.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    await context.Response.WriteAsync("user not found");
                    return;
                }
                if (code == "imarket114514")
                {
                    user.Status = 1;
                    await userService.UpdateUserAsync(user.Id, user);
                    await context.Response.WriteAsync("success");
                }
            });
        }
    }
}
