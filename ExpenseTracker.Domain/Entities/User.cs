namespace ExpenseTracker.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}

public enum UserRole
{
    User = 0,
    Manager = 1,
    Admin = 2,
    Owner = 3
}