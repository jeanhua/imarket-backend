using System.Reflection;

namespace imarket.plugin
{
    public class PluginManager
    {
        private static List<IPluginInterceptor> _interceptors = new();
        private static Dictionary<string, PluginRecord> _pluginRecords = new();
        private readonly ILogger<PluginManager> _logger;
        private readonly IServiceProvider _serviceProvider;

        public PluginManager(ILogger<PluginManager> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public IEnumerable<string> GetPluginDirectory()
        {
            var dirs = new List<string>();
            foreach (var directory in Directory.GetDirectories("plugin"))
            {
                dirs.Add(directory);
            }
            return dirs;
        }

        public void LoadPlugins(IServiceCollection services)
        {
            try
            {
                var assembly = Assembly.GetAssembly(typeof(IPluginInterceptor));
                var allTypes = assembly?.GetTypes();
                if (allTypes == null)
                {
                    return;
                }
                int count = 0;
                foreach (var type in allTypes)
                {
                    if (typeof(IPluginInterceptor).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                    {
                        services.AddScoped(type);
                        var instance = services.BuildServiceProvider().GetService(type) as IPluginInterceptor;
                        if (instance != null)
                        {
                            count++;
                            _interceptors.Add(instance);

                            var tag = type.GetCustomAttribute<PluginTag>();
                            if (tag == null)
                            {
                                tag = new PluginTag();
                                tag.Author = "Unknown";
                                tag.Description = "No description";
                                tag.Name = "Unknown";
                            }

                            var pluginRecord = new PluginRecord
                            {
                                Name = tag.Name,
                                Description = tag.Description,
                                Author = tag.Author
                            };
                            _pluginRecords[type.FullName] = pluginRecord;
                        }
                    }
                }

                if (count > 0)
                {
                    _logger.LogInformation($"插件加载成功,加载类 {count}个");
                }
                else
                {
                    _logger.LogWarning($"插件加载失败,没有找到合适的类");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"插件加载失败");
            }
        }

        public IEnumerable<PluginRecord> GetLoadedPlugins()
        {
            return _pluginRecords.Values;
        }

        public async Task<object?> ExecuteBeforeAsync(string route, object?[] args, string? username = null)
        {
            foreach (var interceptor in _interceptors)
            {
                if (interceptor == null) { continue; }
                var result = await interceptor.OnBeforeExecutionAsync(route, args, username);
                if (result.op == true) return result;
            }
            return null;
        }

        public async Task<object?> ExecuteAfterAsync(string route, object? result, string? username = null)
        {
            foreach (var interceptor in _interceptors)
            {
                if (interceptor == null) { continue; }
                var modifiedResult = await interceptor.OnAfterExecutionAsync(route, result, username);
                if (modifiedResult.op == true) return modifiedResult;
            }
            return null;
        }
    }
}
