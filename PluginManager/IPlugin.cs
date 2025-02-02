namespace imarket.plugin
{
    public interface IPluginInterceptor
    {
        Task<(bool op,object? result)> OnBeforeExecutionAsync(string route, object?[] args, string? username = null);
        Task<(bool op, object? result)> OnAfterExecutionAsync(string route, object? result, string? username = null);
    }

    [System.AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class PluginTag : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
    }

    public record PluginRecord
    {
        public string Name { get; init; }
        public string Description { get; init; }
        public string Author { get; init; }
    }
}
