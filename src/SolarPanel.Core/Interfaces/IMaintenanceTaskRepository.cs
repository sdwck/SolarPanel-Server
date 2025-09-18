using SolarPanel.Core.Entities;

namespace SolarPanel.Core.Interfaces;

public interface IMaintenanceTaskRepository
{
    Task<MaintenanceTask?> GetByIdAsync(Guid id);
    Task<(IEnumerable<MaintenanceTask> Items, int TotalCount)> GetPagedAsync(
        MaintenanceTask.MaintenanceStatus? status, int page, int pageSize);
    Task<IEnumerable<MaintenanceTask>> GetAllAsync();
    Task<MaintenanceTask> CreateAsync(MaintenanceTask task);
    Task<MaintenanceTask> UpdateAsync(MaintenanceTask task);
    Task DeleteAsync(Guid id);
    Task<MaintenanceTask> CompleteAsync(Guid id, string? notes);
}