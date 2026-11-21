using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Contracts;
using TaskFlow.Api.Data;
using TaskFlow.Api.Domain;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/users")]
public sealed class AdminController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetUsers(CancellationToken ct)
    {
        var rows = await db.Users.AsNoTracking().OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.Email, x.Role }).ToListAsync(ct);
        return Ok(rows.Select(x => new UserResponse(x.Id, x.Name, x.Email, x.Role.ToString())));
    }

    [HttpPatch("{id:guid}/role")]
    public async Task<IActionResult> UpdateRole(Guid id, UpdateUserRoleRequest request, CancellationToken ct)
    {
        if (request.Role is null || !Enum.IsDefined(request.Role.Value)) return BadRequest(new ProblemDetails { Title = "A valid global role is required" });
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var user = await db.Users.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (user is null) return NotFound();
        if (user.Role == GlobalRole.Admin && request.Role.Value != GlobalRole.Admin)
        {
            var adminCount = await db.Users.CountAsync(x => x.Role == GlobalRole.Admin, ct);
            if (adminCount <= 1)
                return Conflict(new ProblemDetails { Title = "Cannot remove the last Admin", Detail = "At least one global Admin must remain." });
        }
        user.Role = request.Role.Value;
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return NoContent();
    }
}
