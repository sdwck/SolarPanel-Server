using SolarPanel.Application.DTOs;
using SolarPanel.Core.Entities;

namespace SolarPanel.Application.Interfaces;

public interface IMaintenanceTaskService
{
    Task<MaintenanceTaskDto?> GetByIdAsync(Guid id);
    Task<PaginatedResponse<MaintenanceTaskDto>> GetPagedAsync(
        string? filter, int page, int pageSize);
    Task<MaintenanceTaskDto> CreateAsync(CreateMaintenanceTaskDto request);
    Task<MaintenanceTaskDto> UpdateAsync(Guid id, UpdateMaintenanceTaskDto request);
    Task DeleteAsync(Guid id);
    Task<MaintenanceTaskDto> CompleteAsync(Guid id, CompleteMaintenanceTaskDto request);
    Task<MaintenanceTaskStatsDto> GetStatsAsync();
}