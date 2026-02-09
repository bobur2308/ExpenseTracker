using ExpenseTracker.Application.Services;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using ExpenseTracker.Shared.DTOs.Auth;
using ExpenseTracker.Shared.DTOs.Tenant;
using ExpenseTracker.Shared.DTOs.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.API.Controllers.Auth;
[ApiController]
[Route("api/[controller]/[action]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPasswordHashService _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ITenantService _tenantService;

    public AuthController(
        AppDbContext context,
        ITokenService tokenService,
        ITenantService tenantService,
        IPasswordHashService passwordHasher)
    {
        _context = context;
        _tokenService = tokenService;
        _tenantService = tenantService;
        _passwordHasher = passwordHasher;
    }

    [HttpPost]
    public async Task<ActionResult<LoginResponseDto>> RegisterTenant([FromBody] RegisterTenantDto dto)
    {
        // Subdomain unique ekanini tekshirish
        if (await _context.Tenants.AnyAsync(t => t.Subdomain == dto.Subdomain.ToLower()))
        {
            return BadRequest(new { message = "Subdomain already exists" });
        }

        // Email unique ekanini tekshirish
        if (await _context.Users.AnyAsync(u => u.Email == dto.OwnerEmail.ToLower()))
        {
            return BadRequest(new { message = "Email already exists" });
        }

        // Yangi tenant yaratish
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = dto.CompanyName,
            Subdomain = dto.Subdomain.ToLower(),
            ContactEmail = dto.OwnerEmail.ToLower(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Plan = SubscriptionPlan.Free,
            MaxUsers = 5
        };

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        // Tenant context ni set qilish (bu muhim!)
        _tenantService.SetTenantId(tenant.Id);

        // Owner user yaratish
        var owner = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Email = dto.OwnerEmail.ToLower(),
            PasswordHash = _passwordHasher.HashPassword(dto.Password),
            FirstName = dto.OwnerFirstName,
            LastName = dto.OwnerLastName,
            Role = UserRole.Owner,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(owner);
        await _context.SaveChangesAsync();

        // Default categories yaratish
        var defaultCategories = new List<Category>
        {
            new() { Id = Guid.NewGuid(), TenantId = tenant.Id, Name = "Travel", ColorCode = "#3B82F6", IsActive = true },
            new() { Id = Guid.NewGuid(), TenantId = tenant.Id, Name = "Food", ColorCode = "#10B981", IsActive = true },
            new() { Id = Guid.NewGuid(), TenantId = tenant.Id, Name = "Office Supplies", ColorCode = "#F59E0B", IsActive = true },
            new() { Id = Guid.NewGuid(), TenantId = tenant.Id, Name = "Software", ColorCode = "#8B5CF6", IsActive = true },
            new() { Id = Guid.NewGuid(), TenantId = tenant.Id, Name = "Other", ColorCode = "#6B7280", IsActive = true }
        };

        _context.Categories.AddRange(defaultCategories);
        await _context.SaveChangesAsync();

        // Token generate qilish
        var token = _tokenService.GenerateToken(owner);

        return Ok(new LoginResponseDto
        {
            Token = token,
            User = new UserDto
            {
                Id = owner.Id,
                Email = owner.Email,
                FirstName = owner.FirstName,
                LastName = owner.LastName,
                Role = owner.Role.ToString()
            }
        });
    }

    [HttpPost]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto dto)
    {
        // Emailni kichik harfga o'tkazish
        var email = dto.Email.ToLower();

        // User'ni topish (tenant filter o'chirilgan holda)
        var user = await _context.Users
            .IgnoreQueryFilters() // Multi-tenancy filter'ni o'chirish
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        // Tenant active ekanini tekshirish
        if (!user.Tenant.IsActive)
        {
            return Unauthorized(new { message = "Tenant account is inactive" });
        }

        // User active ekanini tekshirish
        if (!user.IsActive)
        {
            return Unauthorized(new { message = "User account is inactive" });
        }

        // Parolni tekshirish
        if (!_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        // LastLogin ni yangilash
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Token generate qilish
        var token = _tokenService.GenerateToken(user);

        return Ok(new LoginResponseDto
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.ToString()
            }
        });
    }
}