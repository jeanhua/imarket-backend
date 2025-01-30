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
                //���Jwt��֤����,�������ͷ��Ϣ
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
                //���ýӿ�Auth��Ȩ��ť
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Value Bearer {token}",
                    Name = "Authorization",//jwtĬ�ϵĲ�������
                    In = ParameterLocation.Header,//jwtĬ�ϴ��Authorization��Ϣ��λ��(����ͷ��)
                    Type = SecuritySchemeType.ApiKey
                });
            });
            // ����ע��
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
            // ע�����ݿ�
            builder.Services.AddSingleton<Database>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var logger = provider.GetRequiredService<ILogger<Database>>();
                return new Database(configuration, logger);
            });
            // ע����������
            builder.Services.AddSingleton<PluginManager>();
            // ע��token������
            builder.Services.AddSingleton<JwtTokenGenerator>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var logger = provider.GetRequiredService<ILogger<JwtTokenGenerator>>();
                return new JwtTokenGenerator(configuration, logger);
            });
            // ���ݿ��ʼ��
            var database = builder.Services.BuildServiceProvider().GetService<Database>();
            database.InitDatabase();
            // �����־����
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
                app.UseMiddleware<GlobalExceptionHandlerMiddleware>(); // ȫ���쳣�����м��
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
