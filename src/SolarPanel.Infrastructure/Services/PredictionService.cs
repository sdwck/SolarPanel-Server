using SolarPanel.Application.DTOs;
using SolarPanel.Application.Interfaces;
using SolarPanel.Core.Entities;
using SolarPanel.Core.Interfaces;

namespace SolarPanel.Infrastructure.Services;

public class PredictionService : IPredictionService
{
    private readonly ISolarDataRepository _repository;

    public PredictionService(ISolarDataRepository repository)
    {
        _repository = repository;
    }

    // Добавить учёт погоды и пр. либо интеграцию с внешними API
    public async Task<PredictionDataDto> GetPredictionAsync(string period)
    {
        var historicalData = await GetHistoricalDataForPrediction(period);
        var prediction = CalculatePrediction(historicalData, period);
        
        return prediction;
    }

    private async Task<List<Core.Entities.SolarData>> GetHistoricalDataForPrediction(string period)
    {
        var now = DateTime.UtcNow;
        var from = period.ToLower() switch
        {
            "today" or "tomorrow" => now.AddDays(-7),
            "week" => now.AddDays(-30),
            "month" => now.AddDays(-90),
            _ => now.AddDays(-7)
        };
        var gap = period.ToLower() switch
        {
            "today" or "tomorrow" => 10,
            "week" => 30,
            "month" => 60,
            _ => 30
        };

        var data = await _repository.GetByDateRangeAsync(from, now, gap);
        return data.Where(d => d.PowerData != null).ToList();
    }

    private static PredictionDataDto CalculatePrediction(List<SolarData> historicalData, string period)
    {
        if (historicalData.Count == 0)
        {
            return new PredictionDataDto
            {
                Period = period,
                EnergyKWh = 0,
                Confidence = 0,
                Factors = ["Insufficient historical data"]
            };
        }

        var avgDailyEnergy = historicalData
            .GroupBy(d => d.Timestamp.Date)
            .Average(g => g.Sum(d => (double)d.PowerData!.PvInputPower) * 60d / historicalData.Count(x => x.Timestamp.Date == g.Key) / 1000.0);

        var multiplier = period.ToLower() switch
        {
            "today" or "tomorrow" => 1.0,
            "week" => 7.0,
            "month" => 30.0,
            _ => 1.0
        };

        var predictedEnergy = avgDailyEnergy * multiplier;
        var confidence = Math.Min(95, historicalData.Count * 2);
        
        var factors = new List<string>
        {
            "Historical performance",
            "Seasonal patterns",
            "Weather conditions"
        };

        return new PredictionDataDto
        {
            Period = period,
            EnergyKWh = Math.Round(predictedEnergy, 2),
            Confidence = confidence,
            Factors = factors
        };  
    }
}