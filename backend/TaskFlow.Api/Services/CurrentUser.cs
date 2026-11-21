using System.Security.Claims;

namespace TaskFlow.Api.Services;

public interface ICurrentUser
{
    Guid Id { get; }
    string Role { get; }
}

public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid Id => Guid.TryParse(accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? accessor.HttpContext?.User.FindFirstValue("sub"), out var id) ? id : Guid.Empty;
    public string Role => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role) ?? "";
}
