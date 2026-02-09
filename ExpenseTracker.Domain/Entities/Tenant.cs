namespace ExpenseTracker.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty; // mycompany.expensetracker.com
    public string ContactEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Subscription info
    public SubscriptionPlan Plan { get; set; } = SubscriptionPlan.Free;
    public DateTime? SubscriptionExpiresAt { get; set; }
    public int MaxUsers { get; set; } = 5;

    // Navigation
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}

public enum SubscriptionPlan
{
    Free = 0,
    Basic = 1,
    Pro = 2,
    Enterprise = 3
}