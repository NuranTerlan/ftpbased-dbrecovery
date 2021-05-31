using System.Threading.Tasks;
using FTPBasedSystem.API.Contracts;
using FTPBasedSystem.DOMAINENTITIES.DTOs;
using FTPBasedSystem.SERVICES.Abstraction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FTPBasedSystem.API.Controllers
{
    public class DistributionController : ControllerBase
    {
        private readonly INumericService _numericService;
        private readonly ITextService _textService;
        private readonly IDateService _dateService;
        private readonly ILogger<DistributionController> _logger;

        public DistributionController(INumericService numericService, ITextService textService, IDateService dateService, ILogger<DistributionController> logger)
        {
            _numericService = numericService;
            _textService = textService;
            _dateService = dateService;
            _logger = logger;
        }

        [HttpPost(DistributionRoutes.Numeric.Create)]
        public async Task<IActionResult> AddNumber([FromBody] NumberDto model)
        {
            var record = await _numericService.Create(model);
            _logger.LogInformation(record.Message);
            return Ok(record);
        }

        [HttpPost(DistributionRoutes.Date.Create)]
        public async Task<IActionResult> AddDate([FromBody] DateDto model)
        {
            var record = await _dateService.Create(model);
            _logger.LogInformation(record.Message);
            return Ok(record);
        }

        [HttpPost(DistributionRoutes.Text.Create)]
        public async Task<IActionResult> AddText([FromBody] TextDto model)
        {
            var record = await _textService.Create(model);
            _logger.LogInformation(record.Message);
            return Ok(record);
        }
    }
}