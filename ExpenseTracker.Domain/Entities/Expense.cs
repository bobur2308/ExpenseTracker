namespace ExpenseTracker.Domain.Entities;

public class Expense
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public Guid CategoryId { get; set; }
    public DateTime ExpenseDate { get; set; }
    public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;
    public string? ReceiptUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public User User { get; set; } = null!;
    public Category Category { get; set; } = null!;
}

public enum ExpenseStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Reimbursed = 3
}