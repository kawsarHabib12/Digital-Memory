using DigitalMemoryMap.DAL.Entities;

namespace DigitalMemoryMap.DAL.Interfaces;

public class MemoryFilterParams
{
    public int? UserId { get; set; }
    public string? Keyword { get; set; }
    public string? Location { get; set; }
    public DateTime? Date { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? CategoryId { get; set; }
    public byte? MoodId { get; set; }
    public string? Tag { get; set; }
    public string? Sort { get; set; } = "date_desc"; // date_desc | date_asc
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class PagedResult<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / (PageSize > 0 ? PageSize : 1));
    public List<T> Items { get; set; } = new();
}

public class CategoryMemoryCount
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int MemoryCount { get; set; }
}

public class MonthlyMemoryCount
{
    public int Month { get; set; }
    public int Count { get; set; }
}

public interface IMemoryRepository
{
    Task<Memory?> GetByIdAsync(int memoryId);
    Task<Memory?> GetByIdAndUserIdAsync(int memoryId, int userId, bool includeRemoved = false);
    Task<Memory> AddAsync(Memory memory);
    Task UpdateAsync(Memory memory);
    Task DeleteAsync(Memory memory);
    Task<PagedResult<Memory>> GetPagedMemoriesAsync(MemoryFilterParams filter);
    Task<List<Memory>> GetMapPinsAsync(int? userId, int? categoryId, byte? moodId, DateTime? from, DateTime? to);
    Task<List<Memory>> GetNearbyMemoriesAsync(int userId, decimal lat, decimal lng, double radiusKm);
    Task<PagedResult<Memory>> GetAllForAdminAsync(int page, int pageSize, string? keyword);
    Task<int> GetTotalCountAsync();
    Task<List<CategoryMemoryCount>> GetCategoryMemoryCountsAsync();
    Task<List<MonthlyMemoryCount>> GetMonthlyMemoryCountsAsync(int year);
}
