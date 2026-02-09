using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Shared.DTOs.Expense;
public class UpdateExpenseDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? Amount { get; set; }
    public Guid? CategoryId { get; set; }
    public DateTime? ExpenseDate { get; set; }
}