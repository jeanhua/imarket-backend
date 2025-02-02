using imarket.plugin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace imarket.Controllers.admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class PluginController : ControllerBase
    {
        private readonly PluginManager pluginManager;
        public PluginController(PluginManager pluginManager)
        {
            this.pluginManager = pluginManager;
        }

        [HttpGet("List")] // api/Plugin/List
        public IActionResult GetPlugins()
        {
            var plugins = pluginManager.GetLoadedPlugins();
            var response = new
            {
                success = true,
                plugins
            };
            return Ok(response);
        }

        [HttpGet("Directory")] // api/Plugin/Directory
        public IActionResult GetPluginDirectory()
        {
            var plugins = pluginManager.GetPluginDirectory();
            var response = new
            {
                success = true,
                plugins
            };
            return Ok(response);
        }
    }
}
