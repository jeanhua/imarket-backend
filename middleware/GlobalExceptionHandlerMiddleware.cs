using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace imarket.middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public GlobalExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlerMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // 记录日志
                _logger.LogError(ex, "Internal Server Error");
                // 返回错误响应
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    message = "Internal Server Error",
                    detail = _env.IsDevelopment() ? ex.Message : null // 仅在开发环境中返回详细错误信息
                };
                System.IO.File.AppendAllText("error.log", DateTime.Now.ToString() + "\t" + ex.ToString() + "\n");
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
