using DigitalMemoryMap.DAL.Entities;

namespace DigitalMemoryMap.DAL.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllActiveAsync();
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<Category?> GetByNameAsync(string name);
    Task<Category> AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task<bool> IsCategoryInUseAsync(int categoryId);
}
