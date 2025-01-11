using imarket.lib;
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
            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
            var app = builder.Build();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            Database.getInstance();
            app.Run();
        }
    }
}
