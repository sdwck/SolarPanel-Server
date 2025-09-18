using SolarPanel.Application.DTOs;
using SolarPanel.Application.Interfaces;
using SolarPanel.Core.Entities;
using SolarPanel.Core.Interfaces;

namespace SolarPanel.Infrastructure.Services;

public class MaintenanceTaskService : IMaintenanceTaskService
{
    private readonly IMaintenanceTaskRepository _repository;

    public MaintenanceTaskService(IMaintenanceTaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<MaintenanceTaskDto?> GetByIdAsync(Guid id)
    {
        var task = await _repository.GetByIdAsync(id);
        return task == null ? null : MapToDto(task);
    }

    public async Task<PaginatedResponse<MaintenanceTaskDto>> GetPagedAsync(
        string? filter, int page, int pageSize)
    {
        MaintenanceTask.MaintenanceStatus? status = filter?.ToLower() switch
        {
            "pending" => MaintenanceTask.MaintenanceStatus.Pending,
            "overdue" => MaintenanceTask.MaintenanceStatus.Overdue,
            "completed" => MaintenanceTask.MaintenanceStatus.Completed,
            _ => null
        };
        
        var (items, totalCount) = await _repository.GetPagedAsync(status, page, pageSize);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PaginatedResponse<MaintenanceTaskDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    public async Task<MaintenanceTaskDto> CreateAsync(CreateMaintenanceTaskDto request)
    {
        var task = new MaintenanceTask
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            DueDate = request.DueDate,
            Category = request.Category,
            EstimatedDuration = request.EstimatedDuration,
            AssignedTo = request.AssignedTo,
            Notes = request.Notes,
            Tags = request.Tags
        };

        var createdTask = await _repository.CreateAsync(task);
        return MapToDto(createdTask);
    }

    public async Task<MaintenanceTaskDto> UpdateAsync(Guid id, UpdateMaintenanceTaskDto request)
    {
        var existingTask = await _repository.GetByIdAsync(id);
        if (existingTask == null)
            throw new ArgumentException($"Maintenance task with ID {id} not found");

        if (request.Title != null) existingTask.Title = request.Title;
        if (request.Description != null) existingTask.Description = request.Description;
        if (request.Priority.HasValue) existingTask.Priority = request.Priority.Value;
        if (request.Status.HasValue) existingTask.Status = request.Status.Value;
        if (request.DueDate != null) existingTask.DueDate = request.DueDate.Value;
        if (request.Category.HasValue) existingTask.Category = request.Category.Value;
        if (request.EstimatedDuration.HasValue) existingTask.EstimatedDuration = request.EstimatedDuration;
        if (request.AssignedTo != null) existingTask.AssignedTo = request.AssignedTo;
        if (request.Notes != null) existingTask.Notes = request.Notes;
        if (request.Tags != null) existingTask.Tags = request.Tags;

        var updatedTask = await _repository.UpdateAsync(existingTask);
        return MapToDto(updatedTask);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task<MaintenanceTaskDto> CompleteAsync(Guid id, CompleteMaintenanceTaskDto request)
    {
        var completedTask = await _repository.CompleteAsync(id, request.Notes);
        return MapToDto(completedTask);
    }

    public async Task<MaintenanceTaskStatsDto> GetStatsAsync()
    {
        var allTasks = (await _repository.GetAllAsync()).ToList();
        var now = DateTime.UtcNow;

        var stats = new MaintenanceTaskStatsDto
        {
            Total = allTasks.Count,
            Pending = allTasks.Count(t => t.Status == MaintenanceTask.MaintenanceStatus.Pending),
            InProgress = allTasks.Count(t => t.Status == MaintenanceTask.MaintenanceStatus.InProgress),
            Completed = allTasks.Count(t => t.Status == MaintenanceTask.MaintenanceStatus.Completed),
            Overdue = allTasks.Count(t => t.Status == MaintenanceTask.MaintenanceStatus.Pending && t.DueDate < now)
        };

        stats.CompletionRate = stats.Total > 0 
            ? Math.Round((decimal)stats.Completed / stats.Total * 100, 1)
            : 0;

        return stats;
    }

    private static MaintenanceTaskDto MapToDto(MaintenanceTask task)
    {
        return new MaintenanceTaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Priority = task.Priority,
            Status = task.Status,
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt,
            CompletedAt = task.CompletedAt,
            Category = task.Category,
            EstimatedDuration = task.EstimatedDuration,
            AssignedTo = task.AssignedTo,
            Notes = task.Notes,
            Tags = task.Tags
        };
    }
}