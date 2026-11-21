using System.ComponentModel.DataAnnotations;
using TaskFlow.Api.Domain;
using TaskStatus = TaskFlow.Api.Domain.TaskStatus;

namespace TaskFlow.Api.Contracts;

public sealed record RegisterRequest([Required, StringLength(120, MinimumLength = 2)] string Name,
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required, MinLength(10), StringLength(128)] string Password);
public sealed record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);
public sealed record UserResponse(Guid Id, string Name, string Email, string Role);
public sealed record AuthResponse(string AccessToken, DateTimeOffset ExpiresAt, UserResponse User);
public sealed record CreateProjectRequest([Required, StringLength(160, MinimumLength = 2)] string Name,
    [StringLength(4000)] string? Description, DateOnly? StartDate, DateOnly? DueDate);
public sealed record UpdateProjectRequest([Required, StringLength(160, MinimumLength = 2)] string Name,
    [StringLength(4000)] string? Description, DateOnly? StartDate, DateOnly? DueDate, ProjectStatus? Status);
public sealed record AddProjectMemberRequest([Required, EmailAddress] string Email, ProjectRole? Role);
public sealed record CreateTaskRequest([Required, StringLength(200, MinimumLength = 2)] string Title,
    [StringLength(8000)] string? Description, TaskPriority? Priority, Guid? AssigneeId, DateOnly? DueDate);
public sealed record UpdateTaskRequest([Required, StringLength(200, MinimumLength = 2)] string Title,
    [StringLength(8000)] string? Description, TaskStatus? Status, TaskPriority? Priority, Guid? AssigneeId, DateOnly? DueDate);
public sealed record UpdateTaskStatusRequest(TaskStatus? Status);
public sealed record CreateCommentRequest([Required, StringLength(4000, MinimumLength = 1)] string Body);
public sealed record ProjectResponse(Guid Id, string Name, string Description, string Status, DateOnly? StartDate,
    DateOnly? DueDate, DateTimeOffset CreatedAt, int MemberCount, int TaskCount, int CompletedTaskCount, bool CanManage);
public sealed record TaskResponse(Guid Id, Guid ProjectId, string Title, string Description, string Status,
    string Priority, Guid? AssigneeId, string? AssigneeName, Guid CreatedById, DateOnly? DueDate,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, int CommentCount);
public sealed record CommentResponse(Guid Id, Guid TaskId, Guid AuthorId, string AuthorName, string Body, DateTimeOffset CreatedAt);
public sealed record BreakdownItem(string Name, int Count);
public sealed record DashboardResponse(int ProjectCount, int OpenTaskCount, int CompletedTaskCount,
    int OverdueTaskCount, IReadOnlyList<BreakdownItem> StatusBreakdown,
    IReadOnlyList<BreakdownItem> PriorityBreakdown, IReadOnlyList<TaskResponse> RecentTasks);

public sealed record UpdateUserRoleRequest(GlobalRole? Role);
public sealed record BootstrapAdminRequest([Required, StringLength(256, MinimumLength = 16)] string BootstrapToken, [Required, StringLength(120, MinimumLength = 2)] string Name, [Required, EmailAddress, StringLength(254)] string Email, [Required, MinLength(12), StringLength(128)] string Password);
