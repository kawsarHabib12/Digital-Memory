using DigitalMemoryMap.DAL.Entities;

namespace DigitalMemoryMap.DAL.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int userId);
    Task<User?> GetByEmailAsync(string email);
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
    Task<List<User>> GetAllAsync(string? search, int page, int pageSize);
    Task<int> CountAsync(string? search);
    Task<int> GetNewUsersThisWeekCountAsync();
    Task<int> CountActiveUsersAsync();
}
