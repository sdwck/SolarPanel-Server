using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SolarPanel.Application.DTOs;

namespace SolarPanel.Infrastructure.Services
{
    public interface IWeatherService
    {
        Task<SolarRadiationForecastDto> GetBlendedDailySolarForecastAsync(double latitude, double longitude);
        Task<List<SolarRadiationForecastDto>> GetBlendedWeeklySolarForecastAsync(double latitude, double longitude);
        Task<List<SolarRadiationForecastDto>> GetBlendedMonthlySolarForecastAsync(double latitude, double longitude);
    }
}

