using Microsoft.EntityFrameworkCore;
using SolarPanel.Core.Entities;
using SolarPanel.Core.Interfaces;
using SolarPanel.Infrastructure.Data;

namespace SolarPanel.Infrastructure.Repositories;

public class MaintenanceTaskRepository : IMaintenanceTaskRepository
{
    private readonly AppDbContext _context;

    public MaintenanceTaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MaintenanceTask?> GetByIdAsync(Guid id)
    {
        return await _context.MaintenanceTasks.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<(IEnumerable<MaintenanceTask> Items, int TotalCount)> GetPagedAsync(
        MaintenanceTask.MaintenanceStatus? status, int page, int pageSize)
    {
        var query = _context.MaintenanceTasks.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status);
        }

        var now = DateTime.UtcNow;
        query = query.Where(t =>
            t.Status != MaintenanceTask.MaintenanceStatus.Pending ||
            t.DueDate >= now ||
            t.Status == MaintenanceTask.MaintenanceStatus.Overdue);

        foreach (var task in await query
                     .Where(t => t.Status == MaintenanceTask.MaintenanceStatus.Pending && t.DueDate < now)
                     .ToListAsync())
        {
            task.Status = MaintenanceTask.MaintenanceStatus.Overdue;
        }

        await _context.SaveChangesAsync();

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(t => t.DueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<MaintenanceTask>> GetAllAsync()
    {
        var now = DateTime.UtcNow;
        var tasks = await _context.MaintenanceTasks.ToListAsync();

        foreach (var task in tasks.Where(t =>
                     t.Status == MaintenanceTask.MaintenanceStatus.Pending &&
                     t.DueDate < now))
        {
            task.Status = MaintenanceTask.MaintenanceStatus.Overdue;
        }

        await _context.SaveChangesAsync();

        return tasks;
    }

    public async Task<MaintenanceTask> CreateAsync(MaintenanceTask task)
    {
        _context.MaintenanceTasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<MaintenanceTask> UpdateAsync(MaintenanceTask task)
    {
        _context.MaintenanceTasks.Update(task);

        var now = DateTime.UtcNow;
        task.Status = task.Status switch
        {
            MaintenanceTask.MaintenanceStatus.Overdue when task.DueDate >= now => MaintenanceTask.MaintenanceStatus .Pending,
            MaintenanceTask.MaintenanceStatus.Pending when task.DueDate < now => MaintenanceTask.MaintenanceStatus .Overdue,
            _ => task.Status
        };

        await _context.SaveChangesAsync();
        return task;
    }


    public async Task DeleteAsync(Guid id)
    {
        var task = await GetByIdAsync(id);
        if (task != null)
        {
            _context.MaintenanceTasks.Remove(task);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<MaintenanceTask> CompleteAsync(Guid id, string? notes)
    {
        var task = await GetByIdAsync(id);
        if (task == null)
            throw new ArgumentException($"Maintenance task with ID {id} not found");

        task.Status = MaintenanceTask.MaintenanceStatus.Completed;
        task.CompletedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(notes))
        {
            task.Notes = string.IsNullOrEmpty(task.Notes)
                ? notes
                : $"{task.Notes}\n\nCompletion Notes: {notes}";
        }

        await _context.SaveChangesAsync();
        return task;
    }
}