using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Contracts;
using TaskFlow.Api.Data;
using TaskFlow.Api.Domain;
using TaskFlow.Api.Security;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/setup")]
public sealed class SetupController(AppDbContext db, PasswordHasher<AppUser> passwordHasher, TokenService tokens, IConfiguration configuration) : ControllerBase
{
    /// <summary>One-time bootstrap. Disabled unless BOOTSTRAP_ADMIN_TOKEN is configured.</summary>
    [HttpPost("bootstrap-admin")]
    public async Task<IActionResult> BootstrapAdmin(BootstrapAdminRequest request, CancellationToken ct)
    {
        var configuredToken = configuration["Bootstrap:AdminToken"] ?? Environment.GetEnvironmentVariable("BOOTSTRAP_ADMIN_TOKEN");
        if (string.IsNullOrWhiteSpace(configuredToken)) return NotFound();

        var expected = Encoding.UTF8.GetBytes(configuredToken);
        var supplied = Encoding.UTF8.GetBytes(request.BootstrapToken);
        if (expected.Length != supplied.Length || !CryptographicOperations.FixedTimeEquals(expected, supplied))
            return Unauthorized();

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        if (await db.Users.AnyAsync(x => x.Role == GlobalRole.Admin, ct))
            return Conflict(new ProblemDetails { Title = "Admin already provisioned", Detail = "The one-time admin bootstrap has already been used." });

        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email, ct))
            return Conflict(new ProblemDetails { Title = "Account already exists" });

        var admin = new AppUser { Name = request.Name.Trim(), Email = email, Role = GlobalRole.Admin };
        admin.PasswordHash = passwordHasher.HashPassword(admin, request.Password);
        db.Users.Add(admin);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        var (token, expiresAt) = tokens.Create(admin);
        return Ok(new AuthResponse(token, expiresAt, new UserResponse(admin.Id, admin.Name, admin.Email, admin.Role.ToString())));
    }
}
