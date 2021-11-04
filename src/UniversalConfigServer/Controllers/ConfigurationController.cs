using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UniversalConfigServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly ILogger<ConfigurationController> _logger;

        public ConfigurationController(ILogger<ConfigurationController> logger, IConfiguration config)
        {
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<string> GetBase()
        {
            return "{\"CloudConfigServer\": {\"version\": \"0.0.1\"}}";
        }

        [HttpGet("config/{appName}/{env}/{configName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status418ImATeapot)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetConfigAsync(string appName, string env, string configName)
        {
            if (appName is null || env is null || configName is null)
            {
                _logger.LogInformation($"One of the parameters is missing: appName: {appName}\tenv: {env}\tconfigName: {configName}");
                return BadRequest();
            }

            string response;
            string filePath = $"configs/{appName}/{env}/{configName}";
            // Load config file
            try
            {
                _logger.LogDebug($"Getting config {filePath}");
                using (StreamReader reader = new StreamReader(filePath))
                {
                    response = await reader.ReadToEndAsync();
                    // Return config file
                    return Ok(response);
                }
            }
            catch (System.IO.DirectoryNotFoundException ex)
            {
                _logger.LogWarning($"Directory {filePath} not found", ex);
                return NotFound();
            }
            catch (System.IO.FileNotFoundException)
            {
                _logger.LogInformation($"File {filePath} not found");
                return NotFound();
            }
            catch (System.Exception ex)
            {
                _logger.LogCritical("Unknown exception occuded while getting config file", ex);
                return StatusCode(418);
            }
        }
    }
}
