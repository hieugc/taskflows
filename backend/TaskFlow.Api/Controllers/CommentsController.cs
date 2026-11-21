using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Contracts;
using TaskFlow.Api.Data;
using TaskFlow.Api.Domain;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks/{taskId:guid}/comments")]
public sealed class CommentsController(AppDbContext db, ICurrentUser currentUser, ProjectAccessService access) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(Guid taskId, CancellationToken ct)
    {
        var task = await db.WorkTasks.AsNoTracking().SingleOrDefaultAsync(x => x.Id == taskId, ct);
        if (task is null || !await access.CanAccessAsync(task.ProjectId, currentUser.Id, currentUser.Role, ct)) return NotFound();
        var comments = await db.TaskComments.AsNoTracking().Where(x => x.TaskId == taskId)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new CommentResponse(x.Id, x.TaskId, x.AuthorId, x.Author!.Name, x.Body, x.CreatedAt))
            .ToListAsync(ct);
        return Ok(comments);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid taskId, CreateCommentRequest request, CancellationToken ct)
    {
        var task = await db.WorkTasks.SingleOrDefaultAsync(x => x.Id == taskId, ct);
        if (task is null || !await access.CanAccessAsync(task.ProjectId, currentUser.Id, currentUser.Role, ct)) return NotFound();
        var body = request.Body.Trim();
        if (body.Length == 0) return BadRequest(new ProblemDetails { Title = "Comment cannot be empty" });
        var comment = new TaskComment { TaskId = taskId, AuthorId = currentUser.Id, Body = body };
        db.TaskComments.Add(comment);
        await db.SaveChangesAsync(ct);
        var authorName = await db.Users.Where(x => x.Id == currentUser.Id).Select(x => x.Name).SingleAsync(ct);
        return CreatedAtAction(nameof(Get), new { taskId }, new CommentResponse(comment.Id, taskId, currentUser.Id, authorName, comment.Body, comment.CreatedAt));
    }
}
