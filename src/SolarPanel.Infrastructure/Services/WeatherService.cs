using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SolarPanel.Application.DTOs;
using System.Linq;

namespace SolarPanel.Infrastructure.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private const int PanelCount = 12;
        private const double PanelPower = 330; 
        private const double InverterLimit = 3000; 
        private const double PanelArea = 1.6;
        private const double Efficiency = 0.018; 
        private const double MeasurementEfficiency = 0.95; 

        public WeatherService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(180)
            };
        }

      
        public async Task<SolarRadiationForecastDto> GetDailySolarForecastAsync(double latitude, double longitude, double? inverterLimitOverride = null)
        {
            var today = DateTime.UtcNow.Date;
            var url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&daily=shortwave_radiation_sum&timezone=UTC&start_date={today:yyyy-MM-dd}&end_date={today:yyyy-MM-dd}";
            var response = await _httpClient.GetStringAsync(url);
            var json = JObject.Parse(response);
            var radiationArr = json["daily"]?["shortwave_radiation_sum"];
            if (radiationArr == null || !radiationArr.Any()) return null;
            double radiation = radiationArr.First.Value<double>(); 
            double totalRadiation = radiation * PanelArea * PanelCount * Efficiency * MeasurementEfficiency;   
            double maxDaylightHours = 10; 
            double inverterLimit = inverterLimitOverride ?? InverterLimit;
            double inverterMaxEnergy = inverterLimit / 1000 * maxDaylightHours;
            double estimatedEnergy = Math.Min(totalRadiation, inverterMaxEnergy);
            
            return new SolarRadiationForecastDto
            {
                Date = today,
                Radiation = radiation,
                EstimatedEnergy = estimatedEnergy
            };
        }


        public async Task<List<SolarRadiationForecastDto>> GetWeeklySolarForecastAsync(double latitude, double longitude)
        {
            var start = DateTime.UtcNow.Date;
            var end = start.AddDays(6);
            var url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&daily=shortwave_radiation_sum&timezone=UTC&start_date={start:yyyy-MM-dd}&end_date={end:yyyy-MM-dd}";
            var response = await _httpClient.GetStringAsync(url);
            var json = JObject.Parse(response);
            var dates = json["daily"]?["time"];
            var radiationArr = json["daily"]?["shortwave_radiation_sum"];
            var result = new List<SolarRadiationForecastDto>();
            if (dates != null && radiationArr != null)
            {
                for (int i = 0; i < Math.Min(7, dates.Count()); i++)
                {
                    double radiation = radiationArr[i].Value<double>();
                    double totalRadiation = radiation * PanelArea * PanelCount * Efficiency * MeasurementEfficiency;
                    
                    double maxDaylightHours = 10; 
                    double inverterMaxEnergy = InverterLimit / 1000 * maxDaylightHours; 
                    double estimatedEnergy = Math.Min(totalRadiation, inverterMaxEnergy);
                    result.Add(new SolarRadiationForecastDto
                    {
                        Date = DateTime.Parse(dates[i].Value<string>()),
                        Radiation = radiation,
                        EstimatedEnergy = estimatedEnergy
                    });
                }
            }
            return result;
        }

      
        public async Task<List<SolarRadiationForecastDto>> GetMonthlySolarForecastAsync(double latitude, double longitude)
        {
            var start = DateTime.UtcNow.Date;
            var end = start.AddDays(15);
            var url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&daily=shortwave_radiation_sum&timezone=UTC&start_date={start:yyyy-MM-dd}&end_date={end:yyyy-MM-dd}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new List<SolarRadiationForecastDto>();
            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);
            var dates = json["daily"]?["time"];
            var radiationArr = json["daily"]?["shortwave_radiation_sum"];
            var result = new List<SolarRadiationForecastDto>();
            if (dates != null && radiationArr != null)
            {
                for (int i = 0; i < Math.Min(16, dates.Count()); i++)
                {
                    double radiation = radiationArr[i].Value<double>();
                    double totalRadiation = radiation * PanelArea * PanelCount * Efficiency * MeasurementEfficiency;
                    double maxDaylightHours = 10;
                    double inverterMaxEnergy = InverterLimit / 1000 * maxDaylightHours;
                    double estimatedEnergy = Math.Min(totalRadiation, inverterMaxEnergy);
                    result.Add(new SolarRadiationForecastDto
                    {
                        Date = DateTime.Parse(dates[i].Value<string>()),
                        Radiation = radiation,
                        EstimatedEnergy = estimatedEnergy
                    });
                }
            }
            return result;
        }

    
        public async Task<SolarRadiationForecastDto> GetHistoricalSolarForecastAsync(double latitude, double longitude, DateTime targetDate)
        {
            var historicalDate = targetDate.AddYears(-1);
            var url = $"https://archive-api.open-meteo.com/v1/archive?latitude={latitude}&longitude={longitude}&start_date={historicalDate:yyyy-MM-dd}&end_date={historicalDate:yyyy-MM-dd}&daily=shortwave_radiation_sum&timezone=UTC";
            var response = await _httpClient.GetStringAsync(url);
            var json = JObject.Parse(response);
            var radiationArr = json["daily"]?["shortwave_radiation_sum"];
            if (radiationArr == null || !radiationArr.Any()) return null;
            double radiation = radiationArr.First.Value<double>();
            double totalRadiation = radiation * PanelArea * PanelCount * Efficiency * MeasurementEfficiency;  

            double maxDaylightHours = 10; 
            double inverterMaxEnergy = InverterLimit / 1000 * maxDaylightHours; 
            double estimatedEnergy = Math.Min(totalRadiation, inverterMaxEnergy);
            return new SolarRadiationForecastDto
            {
                Date = historicalDate,
                Radiation = radiation,
                EstimatedEnergy = estimatedEnergy
            };
        }

       
        public async Task<List<SolarRadiationForecastDto>> GetHistoricalWeeklySolarForecastAsync(double latitude, double longitude, DateTime targetStartDate)
        {
            var historicalStart = targetStartDate.AddYears(-1);
            var historicalEnd = historicalStart.AddDays(6);
            var url = $"https://archive-api.open-meteo.com/v1/archive?latitude={latitude}&longitude={longitude}&start_date={historicalStart:yyyy-MM-dd}&end_date={historicalEnd:yyyy-MM-dd}&daily=shortwave_radiation_sum&timezone=UTC";
            var response = await _httpClient.GetStringAsync(url);
            var json = JObject.Parse(response);
            var dates = json["daily"]?["time"];
            var radiationArr = json["daily"]?["shortwave_radiation_sum"];
            var result = new List<SolarRadiationForecastDto>();
            if (dates != null && radiationArr != null)
            {
                for (int i = 0; i < Math.Min(7, dates.Count()); i++)
                {
                    double radiation = radiationArr[i].Value<double>();
                    double totalRadiation = radiation * PanelArea * PanelCount * Efficiency * MeasurementEfficiency;
                    
                    double maxDaylightHours = 10; 
                    double inverterMaxEnergy = InverterLimit / 1000 * maxDaylightHours; 
                    double estimatedEnergy = Math.Min(totalRadiation, inverterMaxEnergy);
                    result.Add(new SolarRadiationForecastDto
                    {
                        Date = DateTime.Parse(dates[i].Value<string>()),
                        Radiation = radiation,
                        EstimatedEnergy = estimatedEnergy
                    });
                }
            }
            return result;
        }

        
        public async Task<List<SolarRadiationForecastDto>> GetHistoricalMonthlySolarForecastAsync(double latitude, double longitude, DateTime targetStartDate)
        {
            var historicalStart = targetStartDate.AddYears(-1);
            var historicalEnd = historicalStart.AddDays(29);
            var url = $"https://archive-api.open-meteo.com/v1/archive?latitude={latitude}&longitude={longitude}&start_date={historicalStart:yyyy-MM-dd}&end_date={historicalEnd:yyyy-MM-dd}&daily=shortwave_radiation_sum&timezone=UTC";
            var response = await _httpClient.GetStringAsync(url);
            var json = JObject.Parse(response);
            var dates = json["daily"]?["time"];
            var radiationArr = json["daily"]?["shortwave_radiation_sum"];
            var result = new List<SolarRadiationForecastDto>();
            if (dates != null && radiationArr != null)
            {
                for (int i = 0; i < Math.Min(30, dates.Count()); i++)
                {
                    var radiation = radiationArr[i].Value<double>();
                    var totalRadiation = radiation * PanelArea * PanelCount * Efficiency * MeasurementEfficiency;
                   
                    double maxDaylightHours = 10; 
                    var inverterMaxEnergy = InverterLimit / 1000 * maxDaylightHours; 
                    var estimatedEnergy = Math.Min(totalRadiation, inverterMaxEnergy);
                    result.Add(new SolarRadiationForecastDto
                    {
                        Date = DateTime.Parse(dates[i].Value<string>()),
                        Radiation = radiation,
                        EstimatedEnergy = estimatedEnergy
                    });
                }
            }
            return result;
        }

       
        public async Task<SolarRadiationForecastDto> GetBlendedDailySolarForecastAsync(double latitude, double longitude)
        {
            var today = DateTime.UtcNow.Date;
            var forecast = await GetDailySolarForecastAsync(latitude, longitude);
            var history = await GetHistoricalSolarForecastAsync(latitude, longitude, today);
            var blendedEnergy = 0.6 * forecast.EstimatedEnergy + 0.4 * history.EstimatedEnergy;
            return new SolarRadiationForecastDto
            {
                Date = today,
                Radiation = forecast.Radiation,
                EstimatedEnergy = blendedEnergy
            };
        }

       
        public async Task<List<SolarRadiationForecastDto>> GetBlendedWeeklySolarForecastAsync(double latitude, double longitude)
        {
            var start = DateTime.UtcNow.Date;
            var forecastList = await GetWeeklySolarForecastAsync(latitude, longitude);
            var historyList = await GetHistoricalWeeklySolarForecastAsync(latitude, longitude, start);
            var result = new List<SolarRadiationForecastDto>();
            for (int i = 0; i < 7; i++)
            {
                var forecast = forecastList != null && forecastList.Count > i ? forecastList[i] : null;
                var history = historyList != null && historyList.Count > i ? historyList[i] : null;
                double blendedEnergy = 0;
                if (forecast != null && history != null)
                    blendedEnergy = 0.4 * forecast.EstimatedEnergy + 0.6 * history.EstimatedEnergy;
                else if (forecast != null)
                    blendedEnergy = forecast.EstimatedEnergy;
                else if (history != null)
                    blendedEnergy = history.EstimatedEnergy;
                result.Add(new SolarRadiationForecastDto
                {
                    Date = forecast?.Date ?? history?.Date ?? start.AddDays(i),
                    Radiation = forecast?.Radiation ?? history?.Radiation ?? 0,
                    EstimatedEnergy = blendedEnergy
                });
            }
            return result;
        }

       
        public async Task<List<SolarRadiationForecastDto>> GetBlendedMonthlySolarForecastAsync(double latitude, double longitude)
        {
            var start = DateTime.UtcNow.Date;
            var forecastList = await GetMonthlySolarForecastAsync(latitude, longitude);
            var historyList = await GetHistoricalMonthlySolarForecastAsync(latitude, longitude, start);
            var result = new List<SolarRadiationForecastDto>();
            for (int i = 0; i < 30; i++)
            {
                var forecast = forecastList != null && forecastList.Count > i ? forecastList[i] : null;
                var history = historyList != null && historyList.Count > i ? historyList[i] : null;
                double blendedEnergy = 0;
                if (forecast != null && history != null)
                    blendedEnergy = 0.2 * forecast.EstimatedEnergy + 0.8 * history.EstimatedEnergy;
                else if (forecast != null)
                    blendedEnergy = forecast.EstimatedEnergy;
                else if (history != null)
                    blendedEnergy = history.EstimatedEnergy;
                result.Add(new SolarRadiationForecastDto
                {
                    Date = forecast?.Date ?? history?.Date ?? start.AddDays(i),
                    Radiation = forecast?.Radiation ?? history?.Radiation ?? 0,
                    EstimatedEnergy = blendedEnergy
                });
            }
            return result;
        }
    }
}
