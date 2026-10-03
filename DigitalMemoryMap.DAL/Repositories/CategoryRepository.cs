using DigitalMemoryMap.DAL.Data;
using DigitalMemoryMap.DAL.Entities;
using DigitalMemoryMap.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DigitalMemoryMap.DAL.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllActiveAsync()
    {
        return await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        var normalized = name.Trim().ToLower();
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == normalized);
    }

    public async Task<Category> AddAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsCategoryInUseAsync(int categoryId)
    {
        return await _context.Memories.AnyAsync(m => m.CategoryId == categoryId);
    }
}
