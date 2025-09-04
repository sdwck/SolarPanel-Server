using Microsoft.AspNetCore.Mvc;
using SolarPanel.Application.DTOs;
using SolarPanel.Application.Interfaces;

namespace SolarPanel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SolarDataController : ControllerBase
{
    private readonly ISolarDataService _solarDataService;
    private readonly ILogger<SolarDataController> _logger;

    public SolarDataController(ISolarDataService solarDataService, ILogger<SolarDataController> logger)
    {
        _solarDataService = solarDataService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<SolarDataResponseDto>> GetAll([FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize is < 1 or > 100) pageSize = 50;

            var result = await _solarDataService.GetAllAsync(page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting solar data");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("latest")]
    public async Task<ActionResult<SolarDataDto>> GetLatest()
    {
        try
        {
            var result = await _solarDataService.GetLatestDataAsync();

            if (result == null)
                return NotFound("No solar data found");

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest solar data");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SolarDataDto>> GetById(int id)
    {
        try
        {
            var result = await _solarDataService.GetByIdAsync(id);

            if (result == null)
                return NotFound($"Solar data with ID {id} not found");

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting solar data by ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("range")]
    public async Task<ActionResult<IEnumerable<SolarDataDto>>> GetByDateRange(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        try
        {
            if (from > to)
                return BadRequest("'From' date cannot be greater than 'To' date");

            var result = await _solarDataService.GetByDateRangeAsync(from, to);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting solar data by date range");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id:int}/battery")]
    public async Task<ActionResult<BatteryDataDto>> GetBatteryData(int id)
    {
        try
        {
            var solarData = await _solarDataService.GetByIdAsync(id);

            if (solarData?.BatteryData == null)
                return NotFound($"Battery data for solar data ID {id} not found");

            return Ok(solarData.BatteryData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting battery data for solar data ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}