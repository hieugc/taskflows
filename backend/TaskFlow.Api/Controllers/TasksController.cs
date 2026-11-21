using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Contracts;
using TaskFlow.Api.Data;
using TaskFlow.Api.Domain;
using TaskStatus = TaskFlow.Api.Domain.TaskStatus;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public sealed class TasksController(AppDbContext db, ICurrentUser currentUser, ProjectAccessService access) : ControllerBase
{
    [HttpGet("projects/{projectId:guid}/tasks")]
    public async Task<IActionResult> GetProjectTasks(Guid projectId, [FromQuery] TaskStatus? status,
        [FromQuery] Guid? assigneeId, CancellationToken ct)
    {
        if (!await access.CanAccessAsync(projectId, currentUser.Id, currentUser.Role, ct)) return NotFound();
        var query = db.WorkTasks.AsNoTracking().Where(x => x.ProjectId == projectId);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        if (assigneeId.HasValue) query = query.Where(x => x.AssigneeId == assigneeId.Value);
        var rows = await query.OrderBy(x => x.DueDate).ThenByDescending(x => x.Priority)
            .Select(x => new { Task = x, AssigneeName = x.Assignee == null ? null : x.Assignee.Name, CommentCount = x.Comments.Count })
            .ToListAsync(ct);
        return Ok(rows.Select(x => ToResponse(x.Task, x.AssigneeName, x.CommentCount)).ToList());
    }

    [HttpPost("projects/{projectId:guid}/tasks")]
    public async Task<IActionResult> Create(Guid projectId, CreateTaskRequest request, CancellationToken ct)
    {
        if (!await access.CanAccessAsync(projectId, currentUser.Id, currentUser.Role, ct)) return NotFound();
        if (!await access.CanManageAsync(projectId, currentUser.Id, currentUser.Role, ct)) return Forbid();
        if (request.Priority is null || !Enum.IsDefined(request.Priority.Value)) return BadRequest(new ProblemDetails { Title = "A valid task priority is required" });
        if (!await db.Projects.AnyAsync(x => x.Id == projectId && x.Status == ProjectStatus.Active, ct)) return NotFound();
        if (request.AssigneeId.HasValue && !await access.CanAccessAsync(projectId, request.AssigneeId.Value, "Member", ct))
            return BadRequest(new ProblemDetails { Title = "Invalid assignee", Detail = "Assignee must be a member of this project." });
        var task = new WorkTask
        {
            ProjectId = projectId, Title = request.Title.Trim(), Description = request.Description?.Trim() ?? string.Empty,
            Priority = request.Priority.Value, AssigneeId = request.AssigneeId, DueDate = request.DueDate, CreatedById = currentUser.Id
        };
        db.WorkTasks.Add(task);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, ToResponse(task, null, 0));
    }

    [HttpGet("tasks/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var task = await db.WorkTasks.AsNoTracking().Include(x => x.Assignee).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (task is null) return NotFound();
        if (!await access.CanAccessAsync(task.ProjectId, currentUser.Id, currentUser.Role, ct)) return NotFound();
        return Ok(ToResponse(task, task.Assignee?.Name, await db.TaskComments.CountAsync(x => x.TaskId == id, ct)));
    }

    [HttpPut("tasks/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateTaskRequest request, CancellationToken ct)
    {
        var task = await db.WorkTasks.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (task is null) return NotFound();
        if (!await access.CanAccessAsync(task.ProjectId, currentUser.Id, currentUser.Role, ct)) return NotFound();
        if (!await access.CanManageAsync(task.ProjectId, currentUser.Id, currentUser.Role, ct)) return Forbid();
        if (request.Status is null || request.Priority is null || !Enum.IsDefined(request.Status.Value) || !Enum.IsDefined(request.Priority.Value)) return BadRequest(new ProblemDetails { Title = "A valid task status and priority are required" });
        if (request.AssigneeId.HasValue && !await access.CanAccessAsync(task.ProjectId, request.AssigneeId.Value, "Member", ct))
            return BadRequest(new ProblemDetails { Title = "Invalid assignee", Detail = "Assignee must be a member of this project." });
        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim() ?? string.Empty;
        task.Status = request.Status.Value;
        task.Priority = request.Priority.Value;
        task.AssigneeId = request.AssigneeId;
        task.DueDate = request.DueDate;
        task.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("tasks/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var task = await db.WorkTasks.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (task is null) return NotFound();
        if (!await access.CanAccessAsync(task.ProjectId, currentUser.Id, currentUser.Role, ct)) return NotFound();
        if (!await access.CanManageAsync(task.ProjectId, currentUser.Id, currentUser.Role, ct)) return Forbid();
        db.WorkTasks.Remove(task);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPatch("tasks/{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateTaskStatusRequest request, CancellationToken ct)
    {
        var task = await db.WorkTasks.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (task is null) return NotFound();
        if (!await access.CanAccessAsync(task.ProjectId, currentUser.Id, currentUser.Role, ct)) return NotFound();
        if (!await access.CanEditTaskAsync(task, currentUser.Id, currentUser.Role, ct)) return Forbid();
        if (request.Status is null || !Enum.IsDefined(request.Status.Value)) return BadRequest(new ProblemDetails { Title = "A valid task status is required" });
        task.Status = request.Status.Value;
        task.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static TaskResponse ToResponse(WorkTask task, string? assigneeName, int commentCount) =>
        new(task.Id, task.ProjectId, task.Title, task.Description, task.Status.ToString(), task.Priority.ToString(),
            task.AssigneeId, assigneeName, task.CreatedById, task.DueDate, task.CreatedAt, task.UpdatedAt, commentCount);
}
