using imarket.middleware;
using imarket.plugin;
using imarket.service.IService;
using imarket.service.Service;
using imarket.utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
                        if (port < 1 || port > 65535)
                        {
                            throw new Exception();
                        }
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
                options.ListenAnyIP(port);
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
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                //添加Jwt验证设置,添加请求头信息
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        new List<string>()
                    }
                });
                //放置接口Auth授权按钮
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Value Bearer {token}",
                    Name = "Authorization",//jwt默认的参数名称
                    In = ParameterLocation.Header,//jwt默认存放Authorization信息的位置(请求头中)
                    Type = SecuritySchemeType.ApiKey
                });
            });
            // 依赖注入
            builder.Services.AddScoped<ILikeService, LikeService>();
            builder.Services.AddScoped<IPostCategoriesService, PostCategoriesService>();
            builder.Services.AddScoped<ICommentService, CommentService>();
            builder.Services.AddScoped<IImageService, ImageService>();
            builder.Services.AddScoped<IFavoriteService, FavoriteService>();
            builder.Services.AddScoped<IPostService, PostService>();
            builder.Services.AddScoped<IMessageService, MessageService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ILoginService, LoginService>();   
            builder.Services.AddScoped<IMailService, MailService>();
            // 注入数据库
            builder.Services.AddSingleton<Database>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var logger = provider.GetRequiredService<ILogger<Database>>();
                return new Database(configuration, logger);
            });
            // 注入插件管理器
            builder.Services.AddSingleton<PluginManager>();
            // 注入token生成器
            builder.Services.AddSingleton<JwtTokenGenerator>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var logger = provider.GetRequiredService<ILogger<JwtTokenGenerator>>();
                return new JwtTokenGenerator(configuration, logger);
            });
            // 数据库初始化
            var database = builder.Services.BuildServiceProvider().GetService<Database>();
            database.InitDatabase();
            // 添加日志服务
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
            builder.Logging.AddEventSourceLogger();
            var app = builder.Build();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseMiddleware<GlobalExceptionHandlerMiddleware>(); // 全局异常处理中间件
            }
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.MapControllers();
            app.Run();
        }
    }
}
