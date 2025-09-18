using Microsoft.AspNetCore.Mvc;
using SolarPanel.Application.DTOs;
using SolarPanel.Application.Interfaces;

namespace SolarPanel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceTaskService _maintenanceService;

    public MaintenanceController(IMaintenanceTaskService maintenanceService)
    {
        _maintenanceService = maintenanceService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<MaintenanceTaskDto>>> GetTasks(
        [FromQuery] string? filter,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _maintenanceService.GetPagedAsync(filter, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MaintenanceTaskDto>> GetTask(Guid id)
    {
        var task = await _maintenanceService.GetByIdAsync(id);
        if (task == null)
            return NotFound();

        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<MaintenanceTaskDto>> CreateTask(
        [FromBody] CreateMaintenanceTaskDto request)
    {
        var task = await _maintenanceService.CreateAsync(request);
        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<MaintenanceTaskDto>> UpdateTask(
        Guid id, [FromBody] UpdateMaintenanceTaskDto request)
    {
        try
        {
            var task = await _maintenanceService.UpdateAsync(id, request);
            return Ok(task);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        await _maintenanceService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPatch("{id:guid}/complete")]
    public async Task<ActionResult<MaintenanceTaskDto>> CompleteTask(
        Guid id, [FromBody] CompleteMaintenanceTaskDto request)
    {
        try
        {
            var task = await _maintenanceService.CompleteAsync(id, request);
            return Ok(task);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpGet("stats")]
    public async Task<ActionResult<MaintenanceTaskStatsDto>> GetStats()
    {
        var stats = await _maintenanceService.GetStatsAsync();
        return Ok(stats);
    }
}