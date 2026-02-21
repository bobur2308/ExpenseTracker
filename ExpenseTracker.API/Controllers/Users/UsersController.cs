using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using ExpenseTracker.Shared.DTOs;
using ExpenseTracker.Shared.DTOs.User;

namespace ExpenseTracker.API.Controllers.Users;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPasswordHashService _passwordHasher;

    public UsersController(AppDbContext context, IPasswordHashService passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    private string GetCurrentUserRole() =>
        User.FindFirst(ClaimTypes.Role)!.Value;

    // GET: api/Users
    [HttpGet]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role.ToString(),
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt
            })
            .ToListAsync();

        return Ok(users);
    }

    // GET: api/Users/me
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound();

        return Ok(new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        });
    }

    // POST: api/Users/invite
    [HttpPost("invite")]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<ActionResult<UserDto>> InviteUser([FromBody] InviteUserDto dto)
    {
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == dto.Email.ToLower());

        if (emailExists)
            return BadRequest(new { message = "Email already exists in this tenant" });

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = dto.Email.ToLower(),
            PasswordHash = _passwordHasher.HashPassword(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Role = dto.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        });
    }

    // PUT: api/Users/{id}/role
    [HttpPut("{id}/role")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleDto dto)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        // Owner rolini o'zgartirib bo'lmaydi
        if (user.Role == UserRole.Owner)
            return BadRequest(new { message = "Cannot change Owner role" });

        user.Role = dto.Role;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PUT: api/Users/{id}/deactivate
    [HttpPut("{id}/deactivate")]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        if (user.Role == UserRole.Owner)
            return BadRequest(new { message = "Cannot deactivate Owner" });

        user.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PUT: api/Users/change-password
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = GetCurrentUserId();
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound();

        if (!_passwordHasher.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
            return BadRequest(new { message = "Current password is incorrect" });

        user.PasswordHash = _passwordHasher.HashPassword(dto.NewPassword);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Password changed successfully" });
    }
}