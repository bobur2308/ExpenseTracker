using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Shared.DTOs.Expense;
public class CreateExpenseDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public Guid CategoryId { get; set; }
    public DateTime ExpenseDate { get; set; }
}