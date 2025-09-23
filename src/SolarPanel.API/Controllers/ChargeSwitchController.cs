using Microsoft.AspNetCore.Mvc;
using SolarPanel.Application.DTOs;
using SolarPanel.Infrastructure.Services;

namespace SolarPanel.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChargeSwitchController : ControllerBase
    {
        private readonly MqttService _mqttService;
        private readonly ILogger<ChargeSwitchController> _logger;

        public ChargeSwitchController(MqttService mqttService, ILogger<ChargeSwitchController> logger)
        {
            _mqttService = mqttService;
            _logger = logger;
        }


        [HttpPost("battery")]
        public async Task<IActionResult> SetBatteryChargePriority([FromQuery] string option)
        {
 
            string command;

            switch (option)
            {
                case "PCP00":
                    command = "PCP00"; 
                    break;
                case "PCP03":
                    command = "PCP03"; 
                    break;
                default:
                    return BadRequest("Invalid battery charge option. Use PCP00 or PCP03.");
            }

           
            var commandDto = new InverterCommandDto { CommandCharge = command };
            
            try
            {
                await _mqttService.PublishAsync(commandDto);
                _logger.LogInformation("Sent battery charge command: {Command}", command);
                return Ok(new { message = "Battery charge priority set.", command });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending battery charge command: {Command}", command);
                return StatusCode(500, new { message = "Failed to send command to inverter", error = ex.Message });
            }
        }


        [HttpPost("load")]
        public async Task<IActionResult> SetLoadSourcePriority([FromQuery] string option)
        {

            string command;

            switch (option)
            {
                case "POP00":
                    command = "POP00";
                    break;
                case "POP01":
                    command = "POP01";
                    break;
                case "POP02":
                    command = "POP02";
                    break;
                default:
                    return BadRequest("Invalid load source option. Use POP00, POP01, or POP02.");
            }


            var commandDto = new InverterCommandDto { CommandLoad = command };
            
            try
            {
                await _mqttService.PublishAsync(commandDto);
                _logger.LogInformation("Sent load source command: {Command}", command);
                return Ok(new { message = "Load source priority set.", command });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending load source command: {Command}", command);
                return StatusCode(500, new { message = "Failed to send command to inverter", error = ex.Message });
            }
        }

        [HttpGet("mode")]
        public async Task<IActionResult> GetCurrentMode()
        {
            try
            {
                var mode = await _mqttService.GetCurrentModeAsync();
                if (mode == null)
                    return NotFound(new { success = false, error = "Не удалось получить режим работы инвертора" });
                return Ok(new { success = true, data = mode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении режима работы инвертора");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}