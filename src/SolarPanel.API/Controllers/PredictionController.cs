using Microsoft.AspNetCore.Mvc;
using SolarPanel.Infrastructure.Services;
using SolarPanel.Application.DTOs;

namespace SolarPanel.API.Controllers
{
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
        public async Task<ActionResult<SolarRadiationForecastDto>> GetDayPrediction(double latitude, double longitude)
        {
            var result = await _weatherService.GetBlendedDailySolarForecastAsync(latitude, longitude);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("week")]
        public async Task<ActionResult<List<SolarRadiationForecastDto>>> GetWeekPrediction(double latitude, double longitude)
        {
            var result = await _weatherService.GetBlendedWeeklySolarForecastAsync(latitude, longitude);
            if (result == null || result.Count == 0) return NotFound();
            return Ok(result);
        }

        [HttpGet("month")]
        public async Task<ActionResult<List<SolarRadiationForecastDto>>> GetMonthPrediction(double latitude, double longitude)
        {
            var result = await _weatherService.GetBlendedMonthlySolarForecastAsync(latitude, longitude);
            if (result == null || result.Count == 0) return NotFound();
            return Ok(result);
        }
    }
}

