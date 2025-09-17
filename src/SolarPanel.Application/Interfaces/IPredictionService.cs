using SolarPanel.Application.DTOs;

namespace SolarPanel.Application.Interfaces;

public interface IPredictionService
{
    Task<PredictionDataDto> GetPredictionAsync(string period);
}