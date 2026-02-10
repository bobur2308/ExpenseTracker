using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Application.Services;

namespace ExpenseTracker.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ITenantService tenantService) : base(options)
    {
        _tenantService = tenantService;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Indexes
        modelBuilder.Entity<Tenant>()
            .HasIndex(t => t.Subdomain)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => new { u.TenantId, u.Email })
            .IsUnique();

        modelBuilder.Entity<Category>()
            .HasIndex(c => new { c.TenantId, c.Name })
            .IsUnique();

        modelBuilder.Entity<Expense>()
            .HasIndex(e => e.TenantId);

        modelBuilder.Entity<Expense>()
            .HasIndex(e => e.UserId);

        modelBuilder.Entity<Expense>()
            .HasIndex(e => e.ExpenseDate);

        modelBuilder.Entity<Expense>()
            .HasIndex(e => e.Status);

        // Global query filters
        modelBuilder.Entity<User>().HasQueryFilter(u =>
            u.TenantId == _tenantService.GetCurrentTenantId());

        modelBuilder.Entity<Expense>().HasQueryFilter(e =>
            e.TenantId == _tenantService.GetCurrentTenantId());

        modelBuilder.Entity<Category>().HasQueryFilter(c =>
            c.TenantId == _tenantService.GetCurrentTenantId());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentTenantId = _tenantService.GetCurrentTenantId();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                var tenantIdProp = entry.Metadata.FindProperty("TenantId");
                if (tenantIdProp != null && currentTenantId.HasValue)
                {
                    entry.Property("TenantId").CurrentValue = currentTenantId.Value;
                }

                var createdAtProp = entry.Metadata.FindProperty("CreatedAt");
                if (createdAtProp != null)
                {
                    entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                var updatedAtProp = entry.Metadata.FindProperty("UpdatedAt");
                if (updatedAtProp != null)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}