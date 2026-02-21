using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using ExpenseTracker.Shared.DTOs;
using ExpenseTracker.Shared.DTOs.Category;

namespace ExpenseTracker.API.Controllers.Categories;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ColorCode = c.ColorCode,
                IsActive = c.IsActive,
                ExpenseCount = c.Expenses.Count()
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(Guid id)
    {
        var category = await _context.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ColorCode = c.ColorCode,
                IsActive = c.IsActive,
                ExpenseCount = c.Expenses.Count()
            })
            .FirstOrDefaultAsync();

        if (category == null)
            return NotFound();

        return Ok(category);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var exists = await _context.Categories
            .AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower());

        if (exists)
            return BadRequest(new { message = "Category with this name already exists" });

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            ColorCode = dto.ColorCode ?? "#000000",
            IsActive = true
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ColorCode = category.ColorCode,
            IsActive = category.IsActive,
            ExpenseCount = 0
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return NotFound();

        if (!string.IsNullOrEmpty(dto.Name))
            category.Name = dto.Name;

        if (dto.Description != null)
            category.Description = dto.Description;

        if (!string.IsNullOrEmpty(dto.ColorCode))
            category.ColorCode = dto.ColorCode;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var category = await _context.Categories
            .Include(c => c.Expenses)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return NotFound();

        // Hard delete o'rniga soft delete
        if (category.Expenses.Any())
        {
            category.IsActive = false;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Category deactivated because it has expenses" });
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}