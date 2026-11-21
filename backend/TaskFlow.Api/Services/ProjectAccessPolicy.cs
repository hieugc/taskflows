namespace TaskFlow.Api.Services;

/// <summary>Pure authorization decisions shared by project access checks and unit tests.</summary>
public static class ProjectAccessPolicy
{
    public static bool CanAccessProject(bool isAdmin, bool isProjectMember) => isAdmin || isProjectMember;
    public static bool CanManageProject(bool isAdmin, bool isProjectManager) => isAdmin || isProjectManager;
    public static bool CanEditTask(bool isAdmin, bool isProjectManager, bool isAssignee) =>
        isAdmin || isProjectManager || isAssignee;
}
