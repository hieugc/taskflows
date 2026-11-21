using System.Net.Http.Headers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Api.Data;
using TaskFlow.Api.Domain;
using TaskFlow.Api.Security;
using Testcontainers.PostgreSql;
using Xunit;

namespace TaskFlow.Tests;

public sealed class TaskFlowApiFixture : IAsyncLifetime
{
    private const string SigningKey = "taskflow-integration-tests-signing-key-at-least-32-bytes";
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("taskflow_integration")
        .WithUsername("taskflow")
        .WithPassword("taskflow_test_password")
        .Build();
    private readonly Dictionary<string, string?> _previousEnvironment = new();
    private WebApplicationFactory<Program> _factory = null!;

    public IServiceProvider Services => _factory.Services;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        SetEnvironmentVariable("DATABASE_URL", _postgres.GetConnectionString());
        SetEnvironmentVariable("JWT_SIGNING_KEY", SigningKey);
        SetEnvironmentVariable("JWT_ISSUER", "TaskFlow.Api");
        SetEnvironmentVariable("JWT_AUDIENCE", "TaskFlow.Web");
        SetEnvironmentVariable("JWT_ACCESS_TOKEN_MINUTES", "60");
        SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        SetEnvironmentVariable("CORS_ORIGINS", "http://localhost:5173");

        _factory = new WebApplicationFactory<Program>();
        using var client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _factory.Dispose();
        await _postgres.DisposeAsync();

        foreach (var (key, value) in _previousEnvironment)
            Environment.SetEnvironmentVariable(key, value);
    }

    public HttpClient CreateClient() => _factory.CreateClient();

    public HttpClient CreateAuthenticatedClient(AppUser user)
    {
        var client = CreateClient();
        using var scope = _factory.Services.CreateScope();
        var token = scope.ServiceProvider.GetRequiredService<TokenService>().Create(user).Token;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.TaskComments.ExecuteDeleteAsync();
        await db.WorkTasks.ExecuteDeleteAsync();
        await db.ProjectMembers.ExecuteDeleteAsync();
        await db.Projects.ExecuteDeleteAsync();
        await db.Users.ExecuteDeleteAsync();
    }

    public async Task<AppUser> CreateUserAsync(string name, string email, GlobalRole role = GlobalRole.Member)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = new AppUser { Name = name, Email = email, Role = role };
        user.PasswordHash = new PasswordHasher<AppUser>().HashPassword(user, "Integration-test-password-123!");
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    public async Task<Project> CreateProjectAsync(
        AppUser owner,
        string name,
        params (AppUser User, ProjectRole Role)[] members)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var project = new Project { Name = name, OwnerId = owner.Id };
        project.Members.Add(new ProjectMember
        {
            ProjectId = project.Id,
            UserId = owner.Id,
            Role = ProjectRole.Manager
        });

        foreach (var (user, role) in members)
        {
            if (user.Id == owner.Id) continue;
            project.Members.Add(new ProjectMember
            {
                ProjectId = project.Id,
                UserId = user.Id,
                Role = role
            });
        }

        db.Projects.Add(project);
        await db.SaveChangesAsync();
        return project;
    }

    public async Task<WorkTask> CreateTaskAsync(
        Project project,
        AppUser creator,
        AppUser? assignee = null,
        TaskFlow.Api.Domain.TaskStatus status = TaskFlow.Api.Domain.TaskStatus.Todo,
        DateOnly? dueDate = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var task = new WorkTask
        {
            ProjectId = project.Id,
            CreatedById = creator.Id,
            AssigneeId = assignee?.Id,
            Title = "Integration test task",
            Status = status,
            DueDate = dueDate
        };
        db.WorkTasks.Add(task);
        await db.SaveChangesAsync();
        return task;
    }

    public async Task<WorkTask> GetTaskAsync(Guid taskId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.WorkTasks.AsNoTracking().SingleAsync(x => x.Id == taskId);
    }

    public async Task ChangeGlobalRoleAsync(Guid userId, GlobalRole role)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.SingleAsync(x => x.Id == userId);
        user.Role = role;
        await db.SaveChangesAsync();
    }

    private void SetEnvironmentVariable(string key, string? value)
    {
        if (!_previousEnvironment.ContainsKey(key))
            _previousEnvironment[key] = Environment.GetEnvironmentVariable(key);

        Environment.SetEnvironmentVariable(key, value);
    }
}
