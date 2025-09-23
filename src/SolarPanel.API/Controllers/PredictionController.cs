using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarPanel.Infrastructure.Services;
using SolarPanel.Application.DTOs;

namespace SolarPanel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PredictionController : ControllerBase
{
    private readonly IWeatherService _weatherService;

    public PredictionController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [HttpGet("day")]
    [Authorize]
    public async Task<ActionResult<SolarRadiationForecastDto>> GetDayPrediction(double latitude, double longitude)
    {
        var result = await _weatherService.GetBlendedDailySolarForecastAsync(latitude, longitude);
        return Ok(result);
    }

    [HttpGet("week")]
    [Authorize]
    public async Task<ActionResult<List<SolarRadiationForecastDto>>> GetWeekPrediction(double latitude,
        double longitude)
    {
        var result = await _weatherService.GetBlendedWeeklySolarForecastAsync(latitude, longitude);
        if (result.Count == 0) return NotFound();
        return Ok(result);
    }

    [HttpGet("month")]
    [Authorize]
    public async Task<ActionResult<List<SolarRadiationForecastDto>>> GetMonthPrediction(double latitude,
        double longitude)
    {
        var result = await _weatherService.GetBlendedMonthlySolarForecastAsync(latitude, longitude);
        if (result.Count == 0) return NotFound();
        return Ok(result);
    }
}