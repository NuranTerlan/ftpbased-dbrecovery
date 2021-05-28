using System.Threading.Tasks;
using FTPBasedSystem.API.Contracts;
using FTPBasedSystem.SERVICES.Abstraction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FTPBasedSystem.API.Controllers
{
    public class ActionLoggingController : ControllerBase
    {
        private readonly ICustomLoggingService _actionLoggingService;
        private readonly ILogger<ActionLoggingController> _logger;

        public ActionLoggingController(ICustomLoggingService actionLoggingService, ILogger<ActionLoggingController> logger)
        {
            _actionLoggingService = actionLoggingService;
            _logger = logger;
        }

        [HttpGet(DistributionRoutes.ActionLog.Fetch)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _actionLoggingService.GetAllActions();
            _logger.LogInformation(result.Message);
            return Ok(result);
        }
    }
}