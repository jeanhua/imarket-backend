using imarket.service.IService;
using imarket.service.Service;
using imarket.utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
namespace imarket
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            int port = 5001;
            foreach (var arg in args)
            {
                if (arg.StartsWith("--port="))
                {
                    try
                    {
                        port = int.Parse(arg.Substring(7));
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Invalid port number, using default port 5001");
                        port = 5001;
                    }
                }
            }
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(port);
            });
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]))
                };
            });
            builder.Services.AddMemoryCache();
            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
            // “¿¿µ◊¢»Î
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IPostService, PostService>();
            builder.Services.AddScoped<ILikeService, LikeService>();
            builder.Services.AddScoped<IPostCategoriesService, PostCategoriesService>();
            builder.Services.AddScoped<ILoginService, LoginService>();
            builder.Services.AddScoped<ICommentService, CommentService>();
            builder.Services.AddScoped<IImageService, ImageService>();
            builder.Services.AddScoped<IFavoriteService,FavoriteService>();

            var app = builder.Build();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.MapControllers();
            Database.getInstance();
            app.Run();
        }
    }
}
