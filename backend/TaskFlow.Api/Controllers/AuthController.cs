using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TaskFlow.Api.Contracts;
using TaskFlow.Api.Data;
using TaskFlow.Api.Domain;
using TaskFlow.Api.Security;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AppDbContext db, PasswordHasher<AppUser> passwordHasher, TokenService tokens) : ControllerBase
{
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email, ct))
            return Conflict(new ProblemDetails { Title = "Account already exists", Detail = "An account with this email already exists." });

        var user = new AppUser { Name = request.Name.Trim(), Email = email, Role = GlobalRole.Member };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        db.Users.Add(user);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return Conflict(new ProblemDetails { Title = "Account already exists", Detail = "An account with this email already exists." });
        }
        return Ok(CreateResponse(user));
    }

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email, ct);
        if (user is null || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            return Unauthorized(new ProblemDetails { Title = "Invalid credentials", Detail = "Email or password is incorrect." });
        return Ok(CreateResponse(user));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken ct)
    {
        var subject = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(subject, out var id)) return Unauthorized();
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (user is null) return Unauthorized();
        return Ok(ToUserResponse(user));
    }

    private AuthResponse CreateResponse(AppUser user)
    {
        var (token, expiresAt) = tokens.Create(user);
        return new AuthResponse(token, expiresAt, ToUserResponse(user));
    }

    private static UserResponse ToUserResponse(AppUser user) => new(user.Id, user.Name, user.Email, user.Role.ToString());
}
