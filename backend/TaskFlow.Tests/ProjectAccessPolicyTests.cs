using Xunit;
using TaskFlow.Api.Services;

namespace TaskFlow.Tests;

public sealed class ProjectAccessPolicyTests
{
    [Theory]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(false, false, false)]
    public void Project_access_is_admin_or_membership(bool isAdmin, bool isMember, bool expected) =>
        Assert.Equal(expected, ProjectAccessPolicy.CanAccessProject(isAdmin, isMember));

    [Theory]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(false, false, false)]
    public void Project_management_is_admin_or_project_manager(bool isAdmin, bool isProjectManager, bool expected) =>
        Assert.Equal(expected, ProjectAccessPolicy.CanManageProject(isAdmin, isProjectManager));

    [Theory]
    [InlineData(true, false, false, true)]
    [InlineData(false, true, false, true)]
    [InlineData(false, false, true, true)]
    [InlineData(false, false, false, false)]
    public void Task_edit_is_admin_manager_or_assignee(bool isAdmin, bool isProjectManager, bool isAssignee, bool expected) =>
        Assert.Equal(expected, ProjectAccessPolicy.CanEditTask(isAdmin, isProjectManager, isAssignee));
}
