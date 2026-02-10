using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using ExpenseTracker.Shared.DTOs.Expense;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ExpenseTracker.API.Controllers.Expenses;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ExpensesController(AppDbContext context)
    {
        _context = context;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetExpenses(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] ExpenseStatus? status)
    {
        var query = _context.Expenses
            .Include(e => e.Category)
            .Include(e => e.User)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(e => e.ExpenseDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(e => e.ExpenseDate <= endDate.Value);

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);

        var expenses = await query
            .OrderByDescending(e => e.ExpenseDate)
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Amount = e.Amount,
                Currency = e.Currency,
                CategoryId = e.CategoryId,
                CategoryName = e.Category.Name,
                ExpenseDate = e.ExpenseDate,
                Status = e.Status.ToString(),
                UserName = $"{e.User.FirstName} {e.User.LastName}",
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();

        return Ok(expenses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExpenseDto>> GetExpense(Guid id)
    {
        var expense = await _context.Expenses
            .Include(e => e.Category)
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (expense == null)
            return NotFound();

        return Ok(new ExpenseDto
        {
            Id = expense.Id,
            Title = expense.Title,
            Description = expense.Description,
            Amount = expense.Amount,
            Currency = expense.Currency,
            CategoryId = expense.CategoryId,
            CategoryName = expense.Category.Name,
            ExpenseDate = expense.ExpenseDate,
            Status = expense.Status.ToString(),
            UserName = $"{expense.User.FirstName} {expense.User.LastName}",
            CreatedAt = expense.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> CreateExpense([FromBody] CreateExpenseDto dto)
    {
        var userId = GetCurrentUserId();

        // Category mavjudligini tekshirish
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
            return BadRequest(new { message = "Category not found" });

        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = dto.Title,
            Description = dto.Description,
            Amount = dto.Amount,
            Currency = dto.Currency,
            CategoryId = dto.CategoryId,
            ExpenseDate = dto.ExpenseDate,
            Status = ExpenseStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        // Yangi expense'ni to'liq ma'lumot bilan qaytarish
        var created = await _context.Expenses
            .Include(e => e.Category)
            .Include(e => e.User)
            .FirstAsync(e => e.Id == expense.Id);

        return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, new ExpenseDto
        {
            Id = created.Id,
            Title = created.Title,
            Description = created.Description,
            Amount = created.Amount,
            Currency = created.Currency,
            CategoryId = created.CategoryId,
            CategoryName = created.Category.Name,
            ExpenseDate = created.ExpenseDate,
            Status = created.Status.ToString(),
            UserName = $"{created.User.FirstName} {created.User.LastName}",
            CreatedAt = created.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExpense(Guid id, [FromBody] UpdateExpenseDto dto)
    {
        var expense = await _context.Expenses.FindAsync(id);

        if (expense == null)
            return NotFound();

        // Faqat o'z expense'ini yoki manager/admin update qilishi mumkin
        var userId = GetCurrentUserId();
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (expense.UserId != userId && userRole != "Manager" && userRole != "Admin" && userRole != "Owner")
            return Forbid();

        if (!string.IsNullOrEmpty(dto.Title))
            expense.Title = dto.Title;

        if (dto.Description != null)
            expense.Description = dto.Description;

        if (dto.Amount.HasValue)
            expense.Amount = dto.Amount.Value;

        if (dto.CategoryId.HasValue)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId.Value);
            if (!categoryExists)
                return BadRequest(new { message = "Category not found" });

            expense.CategoryId = dto.CategoryId.Value;
        }

        if (dto.ExpenseDate.HasValue)
            expense.ExpenseDate = dto.ExpenseDate.Value;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(Guid id)
    {
        var expense = await _context.Expenses.FindAsync(id);

        if (expense == null)
            return NotFound();

        // Faqat o'z expense'ini yoki admin delete qilishi mumkin
        var userId = GetCurrentUserId();
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (expense.UserId != userId && userRole != "Admin" && userRole != "Owner")
            return Forbid();

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Manager,Admin,Owner")]
    public async Task<IActionResult> UpdateExpenseStatus(Guid id, [FromBody] ExpenseStatus status)
    {
        var expense = await _context.Expenses.FindAsync(id);

        if (expense == null)
            return NotFound();

        expense.Status = status;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}