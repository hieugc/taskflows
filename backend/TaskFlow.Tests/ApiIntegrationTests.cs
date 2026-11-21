using System.Net;
using System.Net.Http.Json;
using TaskFlow.Api.Contracts;
using TaskFlow.Api.Domain;
using Xunit;

namespace TaskFlow.Tests;

public sealed class ApiIntegrationTests(TaskFlowApiFixture fixture) : IClassFixture<TaskFlowApiFixture>
{
    [Fact]
    public async Task Registration_always_assigns_member_role_and_login_returns_a_valid_identity()
    {
        await fixture.ResetDatabaseAsync();
        using var client = fixture.CreateClient();

        var register = await client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "New User",
            email = "new.user@example.com",
            password = "Integration-test-password-123!"
        });

        Assert.Equal(HttpStatusCode.OK, register.StatusCode);
        var registered = await register.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(registered);
        Assert.Equal("Member", registered.User.Role);

        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "NEW.USER@example.com",
            password = "Integration-test-password-123!"
        });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var loggedIn = await login.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(loggedIn);
        Assert.Equal(registered.User.Id, loggedIn.User.Id);
    }

    [Fact]
    public async Task Project_member_only_sees_projects_in_which_they_are_members()
    {
        await fixture.ResetDatabaseAsync();
        var owner = await fixture.CreateUserAsync("Project Owner", "owner@example.com", GlobalRole.Manager);
        var member = await fixture.CreateUserAsync("Project Member", "member@example.com");
        var outsider = await fixture.CreateUserAsync("Outsider", "outsider@example.com");
        var visibleProject = await fixture.CreateProjectAsync(owner, "Visible project", (member, ProjectRole.Member));
        var hiddenProject = await fixture.CreateProjectAsync(outsider, "Private project");

        using var client = fixture.CreateAuthenticatedClient(member);
        var response = await client.GetAsync("/api/projects");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var projects = await response.Content.ReadFromJsonAsync<List<ProjectResponse>>();
        Assert.NotNull(projects);
        Assert.Equal(new[] { visibleProject.Id }, projects.Select(x => x.Id));

        var hiddenResponse = await client.GetAsync($"/api/projects/{hiddenProject.Id}");
        Assert.Equal(HttpStatusCode.NotFound, hiddenResponse.StatusCode);
    }

    [Fact]
    public async Task Assigned_member_can_change_task_status_but_cannot_edit_task_fields()
    {
        await fixture.ResetDatabaseAsync();
        var manager = await fixture.CreateUserAsync("Project Manager", "manager@example.com", GlobalRole.Manager);
        var assignee = await fixture.CreateUserAsync("Assigned Member", "assignee@example.com");
        var project = await fixture.CreateProjectAsync(manager, "Delivery project", (assignee, ProjectRole.Member));
        var task = await fixture.CreateTaskAsync(project, manager, assignee);
        using var client = fixture.CreateAuthenticatedClient(assignee);

        var statusResponse = await client.PatchAsJsonAsync($"/api/tasks/{task.Id}/status", new { status = "InProgress" });
        Assert.Equal(HttpStatusCode.NoContent, statusResponse.StatusCode);

        var editResponse = await client.PutAsJsonAsync($"/api/tasks/{task.Id}", new
        {
            title = "Unauthorized title change",
            description = "This should not be persisted",
            status = "InProgress",
            priority = "High",
            assigneeId = assignee.Id,
            dueDate = (DateOnly?)null
        });

        Assert.Equal(HttpStatusCode.Forbidden, editResponse.StatusCode);
        var persisted = await fixture.GetTaskAsync(task.Id);
        Assert.Equal("Integration test task", persisted.Title);
        Assert.Equal(TaskFlow.Api.Domain.TaskStatus.InProgress, persisted.Status);
    }

    [Fact]
    public async Task Only_project_managers_can_create_tasks()
    {
        await fixture.ResetDatabaseAsync();
        var manager = await fixture.CreateUserAsync("Project Manager", "task-manager@example.com", GlobalRole.Manager);
        var member = await fixture.CreateUserAsync("Project Member", "task-member@example.com");
        var project = await fixture.CreateProjectAsync(manager, "Task creation project", (member, ProjectRole.Member));

        using var memberClient = fixture.CreateAuthenticatedClient(member);
        var denied = await memberClient.PostAsJsonAsync($"/api/projects/{project.Id}/tasks", new
        {
            title = "Member-created task",
            description = "",
            priority = "Medium"
        });
        Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);

        using var managerClient = fixture.CreateAuthenticatedClient(manager);
        var allowed = await managerClient.PostAsJsonAsync($"/api/projects/{project.Id}/tasks", new
        {
            title = "Manager-created task",
            description = "",
            priority = "High"
        });
        Assert.Equal(HttpStatusCode.Created, allowed.StatusCode);
    }

    [Fact]
    public async Task Task_comments_are_available_to_project_members_and_include_the_author()
    {
        await fixture.ResetDatabaseAsync();
        var manager = await fixture.CreateUserAsync("Project Manager", "comments-manager@example.com", GlobalRole.Manager);
        var member = await fixture.CreateUserAsync("Comment Author", "comment-author@example.com");
        var outsider = await fixture.CreateUserAsync("Comment Outsider", "comment-outsider@example.com");
        var project = await fixture.CreateProjectAsync(manager, "Comment project", (member, ProjectRole.Member));
        var task = await fixture.CreateTaskAsync(project, manager);

        using var memberClient = fixture.CreateAuthenticatedClient(member);
        var created = await memberClient.PostAsJsonAsync($"/api/tasks/{task.Id}/comments", new
        {
            body = "  Please review the latest update.  "
        });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var comment = await created.Content.ReadFromJsonAsync<CommentResponse>();
        Assert.NotNull(comment);
        Assert.Equal(member.Id, comment.AuthorId);
        Assert.Equal("Please review the latest update.", comment.Body);

        var comments = await memberClient.GetFromJsonAsync<List<CommentResponse>>($"/api/tasks/{task.Id}/comments");
        Assert.NotNull(comments);
        Assert.Single(comments);
        Assert.Equal(member.Name, comments[0].AuthorName);

        using var outsiderClient = fixture.CreateAuthenticatedClient(outsider);
        var hidden = await outsiderClient.GetAsync($"/api/tasks/{task.Id}/comments");
        Assert.Equal(HttpStatusCode.NotFound, hidden.StatusCode);
    }

    [Fact]
    public async Task Dashboard_analytics_only_include_projects_visible_to_the_current_member()
    {
        await fixture.ResetDatabaseAsync();
        var owner = await fixture.CreateUserAsync("Dashboard Owner", "dashboard-owner@example.com", GlobalRole.Manager);
        var member = await fixture.CreateUserAsync("Dashboard Member", "dashboard-member@example.com");
        var outsider = await fixture.CreateUserAsync("Dashboard Outsider", "dashboard-outsider@example.com");
        var visibleProject = await fixture.CreateProjectAsync(owner, "Visible analytics", (member, ProjectRole.Member));
        var hiddenProject = await fixture.CreateProjectAsync(outsider, "Hidden analytics");
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);

        var openTask = await fixture.CreateTaskAsync(visibleProject, owner, status: TaskFlow.Api.Domain.TaskStatus.Todo, dueDate: yesterday);
        var completedTask = await fixture.CreateTaskAsync(visibleProject, owner, status: TaskFlow.Api.Domain.TaskStatus.Done, dueDate: yesterday);
        await fixture.CreateTaskAsync(hiddenProject, outsider, status: TaskFlow.Api.Domain.TaskStatus.Todo, dueDate: yesterday);

        using var client = fixture.CreateAuthenticatedClient(member);
        var dashboard = await client.GetFromJsonAsync<DashboardResponse>("/api/dashboard/summary");

        Assert.NotNull(dashboard);
        Assert.Equal(1, dashboard.ProjectCount);
        Assert.Equal(1, dashboard.OpenTaskCount);
        Assert.Equal(1, dashboard.CompletedTaskCount);
        Assert.Equal(1, dashboard.OverdueTaskCount);
        Assert.Contains(dashboard.StatusBreakdown, item => item.Name == "Todo" && item.Count == 1);
        Assert.Contains(dashboard.StatusBreakdown, item => item.Name == "Done" && item.Count == 1);
        Assert.Equal(new[] { openTask.Id, completedTask.Id }.Order(), dashboard.RecentTasks.Select(x => x.Id).Order());
        Assert.DoesNotContain(dashboard.RecentTasks, item => item.ProjectId == hiddenProject.Id);
    }

    [Fact]
    public async Task Changing_a_users_global_role_invalidates_existing_access_tokens()
    {
        await fixture.ResetDatabaseAsync();
        var user = await fixture.CreateUserAsync("Role Change User", "role-change@example.com", GlobalRole.Manager);
        using var client = fixture.CreateAuthenticatedClient(user);

        await fixture.ChangeGlobalRoleAsync(user.Id, GlobalRole.Member);
        var response = await client.GetAsync("/api/projects");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    [Fact]
    public async Task Api_responses_include_security_headers()
    {
        using var client = fixture.CreateClient();
        var response = await client.GetAsync("/health");

        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("no-referrer", response.Headers.GetValues("Referrer-Policy").Single());
        Assert.Equal("default-src 'none'; frame-ancestors 'none'; base-uri 'none'",
            response.Headers.GetValues("Content-Security-Policy").Single());
    }

}
