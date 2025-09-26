using Microsoft.AspNetCore.Mvc;
using SolarPanel.Application.DTOs;
using SolarPanel.Application.Interfaces;
using SolarPanel.Core.Interfaces;
using SolarPanel.Infrastructure.Services;

namespace SolarPanel.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChargeSwitchController : ControllerBase
    {
        private readonly MqttService _mqttService;
        private readonly ILogger<ChargeSwitchController> _logger;
        private readonly IModeResultRepository _modeResultRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ChargeSwitchController(MqttService mqttService, ILogger<ChargeSwitchController> logger,
            IModeResultRepository modeResultRepository, IWebHostEnvironment webHostEnvironment)
        {
            _mqttService = mqttService;
            _logger = logger;
            _modeResultRepository = modeResultRepository;
            _webHostEnvironment = webHostEnvironment;
        }


        [HttpPost("battery")]
        public async Task<IActionResult> SetBatteryChargePriority([FromQuery] string option)
        {
            if (_webHostEnvironment.IsDevelopment())
            {
                _logger.LogInformation("Development environment detected - skipping MQTT command publish.");
                return Ok(new { message = "Cannot set battery charge priority in development environment.", option });
            }
            
            var availableOptions = new[] { "PCP00", "PCP01", "PCP02", "PCP03" };

            if (!availableOptions.Contains(option))
            {
                return BadRequest(
                    $"Invalid battery charge option. Available options are: {string.Join(", ", availableOptions)}");
            }

            var commandDto = new InverterCommandDto { CommandCharge = option };

            try
            {
                await _mqttService.PublishAsync(commandDto);
                return Ok(new { message = "Battery charge priority set.", option });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to send command to inverter", error = ex.Message });
            }
        }


        [HttpPost("load")]
        public async Task<IActionResult> SetLoadSourcePriority([FromQuery] string option)
        {
            if (_webHostEnvironment.IsDevelopment())
            {
                _logger.LogInformation("Development environment detected - skipping MQTT command publish.");
                return Ok(new { message = "Cannot set load source priority in development environment.", option });
            }
            
            var availableOptions = new[] { "POP00", "POP01", "POP02" };

            if (!availableOptions.Contains(option))
            {
                return BadRequest(
                    $"Invalid load source option. Available options are: {string.Join(", ", availableOptions)}");
            }

            var commandDto = new InverterCommandDto { CommandLoad = option };

            try
            {
                await _mqttService.PublishAsync(commandDto);
                return Ok(new { message = "Load source priority set.", option });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to send command to inverter", error = ex.Message });
            }
        }

        [HttpGet("mode")]
        public async Task<IActionResult> GetCurrentMode()
        {
            try
            {
                var mode = await _modeResultRepository.GetModeResultAsync();
                return Ok(new { data = mode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current inverter mode");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}