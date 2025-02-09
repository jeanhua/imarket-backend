namespace imarket.plugin.template
{
    [PluginTag(Name = "template", Description = "A plugin", Author = "Your Name", Enable = true)]
    public class Template:IPluginInterceptor
    {
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
        }
    }
}
