using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.Domain.Entities;

[Table("tenants")]
public class Tenant
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    [Column("subdomain")]
    public string Subdomain { get; set; } = string.Empty; 
    [Column("contact_email")]
    public string ContactEmail { get; set; } = string.Empty;
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    [Column("is_active")]
    public bool IsActive { get; set; } = true;
    [Column("plan")]
    public SubscriptionPlan Plan { get; set; } = SubscriptionPlan.Free;
    [Column("subscription_expires_at")]
    public DateTime? SubscriptionExpiresAt { get; set; }
    [Column("max_users")]
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