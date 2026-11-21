using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.Domain;

namespace TaskFlow.Api.Services;

public sealed class ProjectAccessService(AppDbContext db)
{
    public async Task<bool> CanAccessAsync(Guid projectId, Guid userId, string globalRole, CancellationToken ct = default)
    {
        var isAdmin = globalRole == GlobalRole.Admin.ToString();
        if (isAdmin) return ProjectAccessPolicy.CanAccessProject(true, false);
        var isMember = await db.ProjectMembers.AnyAsync(x => x.ProjectId == projectId && x.UserId == userId, ct);
        return ProjectAccessPolicy.CanAccessProject(false, isMember);
    }

    public async Task<bool> CanManageAsync(Guid projectId, Guid userId, string globalRole, CancellationToken ct = default)
    {
        var isAdmin = globalRole == GlobalRole.Admin.ToString();
        if (isAdmin) return ProjectAccessPolicy.CanManageProject(true, false);
        var isProjectManager = await db.ProjectMembers.AnyAsync(x => x.ProjectId == projectId && x.UserId == userId && x.Role == ProjectRole.Manager, ct);
        return ProjectAccessPolicy.CanManageProject(false, isProjectManager);
    }

    public async Task<bool> CanEditTaskAsync(WorkTask task, Guid userId, string globalRole, CancellationToken ct = default)
    {
        var isAdmin = globalRole == GlobalRole.Admin.ToString();
        if (isAdmin) return ProjectAccessPolicy.CanEditTask(true, false, false);
        var isProjectManager = await CanManageAsync(task.ProjectId, userId, globalRole, ct);
        return ProjectAccessPolicy.CanEditTask(false, isProjectManager, task.AssigneeId == userId);
    }
}
