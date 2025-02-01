namespace imarket.plugin
{
    public interface IPluginInterceptor
    {
        Task<object?> OnBeforeExecutionAsync(string route, object?[] args);
        Task<object?> OnAfterExecutionAsync(string route, object? result);
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
