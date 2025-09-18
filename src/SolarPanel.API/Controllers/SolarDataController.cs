using Microsoft.AspNetCore.Mvc;
using SolarPanel.Application.DTOs;
using SolarPanel.Application.Interfaces;

namespace SolarPanel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SolarDataController : ControllerBase
{
    private readonly ISolarDataService _solarDataService;
    private readonly IAnalyticsService _analyticsService;
    private readonly IHistoryService _historyService;
    private readonly IPredictionService _predictionService;
    private readonly ISystemMetricsService _systemMetricsService;
    private readonly ILogger<SolarDataController> _logger;

    public SolarDataController(ISolarDataService solarDataService, ILogger<SolarDataController> logger,
        IAnalyticsService analyticsService, IHistoryService historyService, IPredictionService predictionService,
        ISystemMetricsService systemMetricsService)
    {
        _solarDataService = solarDataService;
        _logger = logger;
        _analyticsService = analyticsService;
        _historyService = historyService;
        _predictionService = predictionService;
        _systemMetricsService = systemMetricsService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<SolarDataDto>>> GetAll([FromQuery] int page = 1,
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
        [FromQuery] DateTime to,
        [FromQuery] int? gap)
    {
        try
        {
            if (from > to)
                return BadRequest("'From' date cannot be greater than 'To' date");
            
            if (gap is < 1 or > 9999)
                return BadRequest("'Gap' must be between 1 and 1000");

            var result = await _solarDataService.GetByDateRangeAsync(from, to, gap);
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

    [HttpGet("energy")]
    public async Task<ActionResult<EnergyResponseDto>> GetEnergy(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] string source = "pv")
    {
        try
        {
            if (from > to)
                return BadRequest("'from' cannot be greater than 'to'");

            var result = await _solarDataService.GetEnergyProducedAsync(from, to, source);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating energy for range {From} - {To}", from, to);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<AnalyticsDataDto>> GetAnalytics([FromQuery] string timeRange = "week")
    {
        try
        {
            var analytics = await _analyticsService.GetAnalyticsDataAsync(timeRange);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<HistoryDataDto>>> GetHistory(
        [FromQuery] string timeRange = "today",
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        try
        {
            var history = await _historyService.GetHistoryDataAsync(timeRange, from, to);
            return Ok(history);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("prediction")]
    public async Task<ActionResult<PredictionDataDto>> GetPrediction([FromQuery] string period = "today")
    {
        try
        {
            var prediction = await _predictionService.GetPredictionAsync(period);
            return Ok(prediction);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("metrics")]
    public async Task<ActionResult<SystemMetricsDto>> GetSystemMetrics()
    {
        try
        {
            var metrics = await _systemMetricsService.GetSystemMetricsAsync();
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}