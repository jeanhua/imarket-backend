using System.Reflection;


namespace imarket.plugin
{
    public class PluginManager
    {
        private readonly List<IPluginInterceptor> _interceptors = new();
        private readonly Dictionary<string, Assembly> _loadedAssemblies = new();
        private readonly Dictionary<string, PluginRecord> _pluginRecords = new();
        private readonly ILogger<PluginManager> _logger;

        public PluginManager(ILogger<PluginManager> logger)
        {
            _logger = logger;
        }

        public IEnumerable<string> GetPluginDirectory()
        {
            var dirs = new List<string>();
            foreach(var directory in Directory.GetDirectories("plugin"))
            {
                dirs.Add(directory);
            }
            return dirs;
        }
        public bool LoadPlugins(string pluginDirectory)
        {
            if (!Directory.Exists(pluginDirectory))
                return false;

            foreach (var dllPath in Directory.GetFiles(pluginDirectory, "*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(dllPath);
                    var pluginTypes = assembly.GetTypes()
                        .Where(t => typeof(IPluginInterceptor).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                    foreach (var type in pluginTypes)
                    {
                        if (Activator.CreateInstance(type) is IPluginInterceptor plugin)
                        {
                            _interceptors.Add(plugin);
                            _loadedAssemblies[dllPath] = assembly;

                            var tag = type.GetCustomAttribute<PluginTag>();
                            if (tag != null)
                            {
                                var pluginRecord = new PluginRecord
                                {
                                    Name = tag.Name,
                                    Description = tag.Description,
                                    Author = tag.Author
                                };
                                _pluginRecords[dllPath] = pluginRecord;
                            }
                        }
                    }
                    _logger.LogInformation($"插件加载成功: {dllPath}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"插件加载失败: {dllPath}");
                    return false;
                }
            }
            return true;
        }

        public bool UnloadPlugin(string pluginPath)
        {
            if (_loadedAssemblies.ContainsKey(pluginPath))
            {
                var assembly = _loadedAssemblies[pluginPath];
                _interceptors.RemoveAll(p => p.GetType().Assembly == assembly);
                _loadedAssemblies.Remove(pluginPath);
                _pluginRecords.Remove(pluginPath);
                _logger.LogInformation($"插件卸载成功: {pluginPath}");
                return true;
            }
            return false;
        }

        public IEnumerable<PluginRecord> GetLoadedPlugins()
        {
            return _pluginRecords.Values;
        }

        

        public async Task<object?> ExecuteBeforeAsync(string methodName, object?[] args)
        {
            foreach (var interceptor in _interceptors)
            {
                var result = await interceptor.OnBeforeExecutionAsync(methodName, args);
                if (result != null) return result;
            }
            return null;
        }

        public async Task<object?> ExecuteAfterAsync(string methodName, object? result)
        {
            foreach (var interceptor in _interceptors)
            {
                var modifiedResult = await interceptor.OnAfterExecutionAsync(methodName, result);
                if (modifiedResult != null) return modifiedResult;
            }
            return result;
        }
    }
}
