using SolarPanel.Application.DTOs;

namespace SolarPanel.Application.Interfaces;

public interface ISystemMetricsService
{
    Task<SystemMetricsDto> GetSystemMetricsAsync();
}