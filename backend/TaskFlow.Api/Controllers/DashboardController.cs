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
[Route("api/dashboard")]
public sealed class DashboardController(AppDbContext db, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardResponse>> Summary(CancellationToken ct)
    {
        var projectIds = db.Projects.AsNoTracking().Where(x => x.Status == ProjectStatus.Active);
        if (currentUser.Role != GlobalRole.Admin.ToString())
            projectIds = projectIds.Where(x => x.Members.Any(m => m.UserId == currentUser.Id));
        var ids = await projectIds.Select(x => x.Id).ToListAsync(ct);
        var tasks = db.WorkTasks.AsNoTracking().Where(x => ids.Contains(x.ProjectId));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var openCount = await tasks.CountAsync(x => x.Status != TaskStatus.Done, ct);
        var completedCount = await tasks.CountAsync(x => x.Status == TaskStatus.Done, ct);
        var overdueCount = await tasks.CountAsync(x => x.Status != TaskStatus.Done && x.DueDate.HasValue && x.DueDate < today, ct);
        var statusCounts = await tasks.GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() }).ToListAsync(ct);
        var statusBreakdown = statusCounts.Select(x => new BreakdownItem(x.Status.ToString(), x.Count)).ToList();
        var priorityCounts = await tasks.GroupBy(x => x.Priority)
            .Select(g => new { Priority = g.Key, Count = g.Count() }).ToListAsync(ct);
        var priorityBreakdown = priorityCounts.Select(x => new BreakdownItem(x.Priority.ToString(), x.Count)).ToList();
        var recentRows = await tasks.OrderByDescending(x => x.UpdatedAt).Take(8)
            .Select(x => new { Task = x, AssigneeName = x.Assignee == null ? null : x.Assignee.Name, CommentCount = x.Comments.Count })
            .ToListAsync(ct);
        var recent = recentRows.Select(x => new TaskResponse(x.Task.Id, x.Task.ProjectId, x.Task.Title, x.Task.Description,
            x.Task.Status.ToString(), x.Task.Priority.ToString(), x.Task.AssigneeId, x.AssigneeName, x.Task.CreatedById,
            x.Task.DueDate, x.Task.CreatedAt, x.Task.UpdatedAt, x.CommentCount)).ToList();
        return Ok(new DashboardResponse(ids.Count, openCount, completedCount, overdueCount, statusBreakdown, priorityBreakdown, recent));
    }
}
