using BlazorDynamic.Models;
using BlazorDynamic.Server;
using Microsoft.AspNetCore.Mvc;

namespace BlazorWebAssemblyWithAspNet.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ModuleController : ControllerBase
    {
        private readonly ILogger<ModuleController> _logger;
        private readonly ModuleManager _moduleManager;

        public ModuleController(ILogger<ModuleController> logger, ModuleManager moduleManager )
        {
            _logger = logger;
            _moduleManager = moduleManager;
        }

        [HttpGet("plugins/{name}")]
        public async Task<ActionResult<List<AssemblyRawData>>> GetAsync(string name)
        {
            try
            {
                return await _moduleManager.GetModules(name);
            } catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return NotFound(ex.StackTrace);
            }
            
        }
    }
}