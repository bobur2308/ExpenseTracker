namespace ExpenseTracker.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ColorCode { get; set; } = "#000000";
    public bool IsActive { get; set; } = true;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}