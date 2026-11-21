namespace TaskFlow.Api.Domain;

public enum GlobalRole { Admin, Manager, Member }
public enum ProjectRole { Manager, Member }
public enum ProjectStatus { Active, Archived }
public enum TaskStatus { Todo, InProgress, InReview, Done, Blocked }
public enum TaskPriority { Low, Medium, High, Urgent }

public sealed class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public GlobalRole Role { get; set; } = GlobalRole.Member;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
}

public sealed class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public AppUser? Owner { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;
    public DateOnly? StartDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<WorkTask> Tasks { get; set; } = new List<WorkTask>();
}

public sealed class ProjectMember
{
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
    public ProjectRole Role { get; set; } = ProjectRole.Member;
    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class WorkTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public Guid? AssigneeId { get; set; }
    public AppUser? Assignee { get; set; }
    public Guid CreatedById { get; set; }
    public AppUser? CreatedBy { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
}

public sealed class TaskComment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TaskId { get; set; }
    public WorkTask? Task { get; set; }
    public Guid AuthorId { get; set; }
    public AppUser? Author { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
