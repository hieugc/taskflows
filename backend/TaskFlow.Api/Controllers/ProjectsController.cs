using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Contracts;
using TaskFlow.Api.Data;
using TaskFlow.Api.Domain;
using TaskFlow.Api.Services;
using TaskStatus = TaskFlow.Api.Domain.TaskStatus;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projects")]
public sealed class ProjectsController(AppDbContext db, ICurrentUser currentUser, ProjectAccessService access) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectResponse>>> GetAll(CancellationToken ct)
    {
        var query = db.Projects.AsNoTracking().Where(x => x.Status == ProjectStatus.Active);
        if (currentUser.Role != GlobalRole.Admin.ToString())
            query = query.Where(x => x.Members.Any(m => m.UserId == currentUser.Id));
        var rows = await query.OrderByDescending(x => x.UpdatedAt)
            .Select(x => new
            {
                Project = x,
                MemberCount = x.Members.Count,
                TaskCount = x.Tasks.Count,
                CompletedTaskCount = x.Tasks.Count(t => t.Status == TaskStatus.Done),
                CanManage = currentUser.Role == GlobalRole.Admin.ToString() || x.Members.Any(m => m.UserId == currentUser.Id && m.Role == ProjectRole.Manager)
            }).ToListAsync(ct);
        var projects = rows.Select(x => new ProjectResponse(x.Project.Id, x.Project.Name, x.Project.Description,
            x.Project.Status.ToString(), x.Project.StartDate, x.Project.DueDate, x.Project.CreatedAt,
            x.MemberCount, x.TaskCount, x.CompletedTaskCount, x.CanManage)).ToList();
        return Ok(projects);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectResponse>> Create(CreateProjectRequest request, CancellationToken ct)
    {
        if (currentUser.Role == GlobalRole.Member.ToString()) return Forbid();
        if (request.DueDate.HasValue && request.StartDate.HasValue && request.DueDate < request.StartDate)
            return BadRequest(new ProblemDetails { Title = "Invalid project dates", Detail = "Due date must be on or after start date." });

        var project = new Project
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            OwnerId = currentUser.Id,
            StartDate = request.StartDate,
            DueDate = request.DueDate
        };
        project.Members.Add(new ProjectMember { ProjectId = project.Id, UserId = currentUser.Id, Role = ProjectRole.Manager });
        db.Projects.Add(project);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, ToResponse(project, 1, 0, 0, true));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectResponse>> GetById(Guid id, CancellationToken ct)
    {
        if (!await access.CanAccessAsync(id, currentUser.Id, currentUser.Role, ct)) return NotFound();
        var row = await db.Projects.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new
            {
                Project = x,
                MemberCount = x.Members.Count,
                TaskCount = x.Tasks.Count,
                CompletedTaskCount = x.Tasks.Count(t => t.Status == TaskStatus.Done),
                CanManage = currentUser.Role == GlobalRole.Admin.ToString() || x.Members.Any(m => m.UserId == currentUser.Id && m.Role == ProjectRole.Manager)
            }).SingleOrDefaultAsync(ct);
        if (row is null) return NotFound();
        var project = new ProjectResponse(row.Project.Id, row.Project.Name, row.Project.Description,
            row.Project.Status.ToString(), row.Project.StartDate, row.Project.DueDate, row.Project.CreatedAt,
            row.MemberCount, row.TaskCount, row.CompletedTaskCount, row.CanManage);
        return Ok(project);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProjectRequest request, CancellationToken ct)
    {
        if (!await access.CanAccessAsync(id, currentUser.Id, currentUser.Role, ct)) return NotFound();
        if (!await access.CanManageAsync(id, currentUser.Id, currentUser.Role, ct)) return Forbid();
        if (request.Status is null || !Enum.IsDefined(request.Status.Value)) return BadRequest(new ProblemDetails { Title = "A valid project status is required" });
        if (request.DueDate.HasValue && request.StartDate.HasValue && request.DueDate < request.StartDate)
            return BadRequest(new ProblemDetails { Title = "Invalid project dates", Detail = "Due date must be on or after start date." });
        var project = await db.Projects.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (project is null) return NotFound();
        project.Name = request.Name.Trim();
        project.Description = request.Description?.Trim() ?? string.Empty;
        project.StartDate = request.StartDate;
        project.DueDate = request.DueDate;
        project.Status = request.Status.Value;
        project.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/members")]
    public async Task<IActionResult> GetMembers(Guid id, CancellationToken ct)
    {
        if (!await access.CanAccessAsync(id, currentUser.Id, currentUser.Role, ct)) return NotFound();
        if (!await db.Projects.AnyAsync(x => x.Id == id, ct)) return NotFound();
        var rows = await db.ProjectMembers.AsNoTracking().Where(x => x.ProjectId == id)
            .Select(x => new { x.UserId, Name = x.User!.Name, Email = x.User.Email, x.Role, x.JoinedAt })
            .OrderBy(x => x.Name).ToListAsync(ct);
        return Ok(rows.Select(x => new { x.UserId, x.Name, x.Email, Role = x.Role.ToString(), x.JoinedAt }));
    }

    [HttpPost("{id:guid}/members")]
    public async Task<IActionResult> AddMember(Guid id, AddProjectMemberRequest request, CancellationToken ct)
    {
        if (!await access.CanAccessAsync(id, currentUser.Id, currentUser.Role, ct)) return NotFound();
        if (!await access.CanManageAsync(id, currentUser.Id, currentUser.Role, ct)) return Forbid();
        if (!await db.Projects.AnyAsync(x => x.Id == id, ct)) return NotFound();
        if (request.Role is null || !Enum.IsDefined(request.Role.Value)) return BadRequest(new ProblemDetails { Title = "A valid project role is required" });
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email, ct);
        if (user is null) return NotFound(new ProblemDetails { Title = "User not found", Detail = "Register the user before adding them to a project." });
        var membership = await db.ProjectMembers.SingleOrDefaultAsync(x => x.ProjectId == id && x.UserId == user.Id, ct);
        if (membership is null)
            db.ProjectMembers.Add(new ProjectMember { ProjectId = id, UserId = user.Id, Role = request.Role.Value });
        else
            membership.Role = request.Role.Value;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static ProjectResponse ToResponse(Project p, int memberCount, int taskCount, int completedTaskCount, bool canManage) =>
        new(p.Id, p.Name, p.Description, p.Status.ToString(), p.StartDate, p.DueDate, p.CreatedAt,
            memberCount, taskCount, completedTaskCount, canManage);
}
