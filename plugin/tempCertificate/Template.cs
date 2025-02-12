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
                return context.Response.WriteAsync(@"<!DOCTYPE html>
<html lang=""zh-CN"">
<head>
    <meta charset=""UTF-8"">
    <title>用户认证</title>
    <style>
        body {
            font-family: 'Arial', sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
        }
        .container {
            background-color: #fff;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
            width: 300px;
        }
        form {
            display: flex;
            flex-direction: column;
        }
        input[type='text'] {
            padding: 10px;
            margin: 10px 0;
            border: 1px solid #ddd;
            border-radius: 4px;
        }
        input[type='submit'] {
            padding: 10px;
            background-color: #5cb85c;
            color: white;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            margin-top: 20px;
        }
        input[type='submit']:hover {
            background-color: #4cae4c;
        }
    </style>
</head>
<body>
    <div class=""container"">
        <h2>用户认证</h2>
        <form action='/api/Certificate' method='get'>
            <input type='text' name='username' placeholder='请输入用户名' required>
            <input type='text' name='code' placeholder='请输入认证码' required>
            <input type='submit' value='提交认证'>
        </form>
    </div>
</body>
</html>
");

            });
            endpoints.MapGet("/api/Certificate", async (context) =>
            {
                var code = context.Request.Query["code"];
                var username = context.Request.Query["username"];
                var user = await userService.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    await context.Response.WriteAsync("<h1>user not found,please register!</h1>");
                    return;
                }
                if (code == "imarket114514")
                {
                    user.Status = 1;
                    await userService.UpdateUserAsync(user.Id, user);
                    await context.Response.WriteAsync("success");
                }
                else
                {
                    await context.Response.WriteAsync("<h1>error code</h1>");
                }
            });
        }
    }
}
