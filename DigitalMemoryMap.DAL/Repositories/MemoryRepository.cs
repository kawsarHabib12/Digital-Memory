using System;
using System.Linq;
using System.Collections.Generic;
using DigitalMemoryMap.DAL.Data;
using DigitalMemoryMap.DAL.Entities;
using DigitalMemoryMap.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DigitalMemoryMap.DAL.Repositories;

public class MemoryRepository : IMemoryRepository
{
    private readonly AppDbContext _context;

    public MemoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Memory?> GetByIdAsync(int memoryId)
    {
        return await _context.Memories
            .Include(m => m.Category)
            .Include(m => m.Mood)
            .Include(m => m.Photos)
            .Include(m => m.MemoryTags)
                .ThenInclude(mt => mt.Tag)
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.MemoryId == memoryId);
    }

    public async Task<Memory?> GetByIdAndUserIdAsync(int memoryId, int userId, bool includeRemoved = false)
    {
        var query = _context.Memories
            .Include(m => m.Category)
            .Include(m => m.Mood)
            .Include(m => m.Photos)
            .Include(m => m.MemoryTags)
                .ThenInclude(mt => mt.Tag)
            .Where(m => m.MemoryId == memoryId && m.UserId == userId);

        if (!includeRemoved)
        {
            query = query.Where(m => m.Status == 1);
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<Memory> AddAsync(Memory memory)
    {
        memory.CreatedAt = DateTime.UtcNow;
        _context.Memories.Add(memory);
        await _context.SaveChangesAsync();
        return memory;
    }

    public async Task UpdateAsync(Memory memory)
    {
        memory.UpdatedAt = DateTime.UtcNow;
        _context.Memories.Update(memory);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Memory memory)
    {
        _context.Memories.Remove(memory);
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResult<Memory>> GetPagedMemoriesAsync(MemoryFilterParams filter)
    {
        var query = _context.Memories
            .Include(m => m.Category)
            .Include(m => m.Mood)
            .Include(m => m.Photos)
            .Include(m => m.MemoryTags)
                .ThenInclude(mt => mt.Tag)
            .Include(m => m.User)
            .Where(m => m.Status == 1)
            .AsQueryable();

        if (filter.UserId.HasValue)
        {
            query = query.Where(m => m.UserId == filter.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var term = filter.Keyword.Trim().ToLower();
            query = query.Where(m => m.Title.ToLower().Contains(term) || (m.Description != null && m.Description.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Location))
        {
            var loc = filter.Location.Trim().ToLower();
            query = query.Where(m => m.LocationName != null && m.LocationName.ToLower().Contains(loc));
        }

        if (filter.Date.HasValue)
        {
            var d = filter.Date.Value.Date;
            query = query.Where(m => m.MemoryDate.Date == d);
        }

        if (filter.From.HasValue)
        {
            var f = filter.From.Value.Date;
            query = query.Where(m => m.MemoryDate.Date >= f);
        }

        if (filter.To.HasValue)
        {
            var t = filter.To.Value.Date;
            query = query.Where(m => m.MemoryDate.Date <= t);
        }

        if (filter.CategoryId.HasValue && filter.CategoryId.Value > 0)
        {
            query = query.Where(m => m.CategoryId == filter.CategoryId.Value);
        }

        if (filter.MoodId.HasValue && filter.MoodId.Value > 0)
        {
            query = query.Where(m => m.MoodId == filter.MoodId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Tag))
        {
            var normalizedTag = filter.Tag.Trim().ToLower();
            query = query.Where(m => m.MemoryTags.Any(mt => mt.Tag.Name == normalizedTag));
        }

        if (string.Equals(filter.Sort, "date_asc", StringComparison.OrdinalIgnoreCase))
        {
            query = query.OrderBy(m => m.MemoryDate).ThenBy(m => m.MemoryId);
        }
        else
        {
            query = query.OrderByDescending(m => m.MemoryDate).ThenByDescending(m => m.MemoryId);
        }

        var totalItems = await query.CountAsync();
        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize < 1 ? 10 : (filter.PageSize > 50 ? 50 : filter.PageSize);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Memory>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            Items = items
        };
    }

    public async Task<List<Memory>> GetMapPinsAsync(int? userId, int? categoryId, byte? moodId, DateTime? from, DateTime? to)
    {
        var query = _context.Memories
            .Include(m => m.Category)
            .Include(m => m.Mood)
            .Include(m => m.Photos)
            .Where(m => m.Status == 1)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(m => m.UserId == userId.Value);
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(m => m.CategoryId == categoryId.Value);
        }

        if (moodId.HasValue && moodId.Value > 0)
        {
            query = query.Where(m => m.MoodId == moodId.Value);
        }

        if (from.HasValue)
        {
            var f = from.Value.Date;
            query = query.Where(m => m.MemoryDate.Date >= f);
        }

        if (to.HasValue)
        {
            var t = to.Value.Date;
            query = query.Where(m => m.MemoryDate.Date <= t);
        }

        return await query
            .OrderByDescending(m => m.MemoryDate)
            .ToListAsync();
    }

    public async Task<List<Memory>> GetNearbyMemoriesAsync(int userId, decimal lat, decimal lng, double radiusKm)
    {
        // 1 deg latitude ≈ 111 km
        var latDelta = (decimal)(radiusKm / 111.0);
        var cosLat = Math.Cos((double)lat * Math.PI / 180.0);
        var lngDelta = cosLat > 0 ? (decimal)(radiusKm / (111.0 * cosLat)) : latDelta;

        var minLat = lat - latDelta;
        var maxLat = lat + latDelta;
        var minLng = lng - lngDelta;
        var maxLng = lng + lngDelta;

        var candidates = await _context.Memories
            .Include(m => m.Category)
            .Include(m => m.Mood)
            .Include(m => m.Photos)
            .Where(m => m.UserId == userId && m.Status == 1)
            .Where(m => m.Latitude >= minLat && m.Latitude <= maxLat && m.Longitude >= minLng && m.Longitude <= maxLng)
            .ToListAsync();

        // Refine with Haversine formula in C#
        var results = new List<Memory>();
        const double earthRadiusKm = 6371.0;

        foreach (var mem in candidates)
        {
            var dLat = ((double)mem.Latitude - (double)lat) * Math.PI / 180.0;
            var dLng = ((double)mem.Longitude - (double)lng) * Math.PI / 180.0;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos((double)lat * Math.PI / 180.0) * Math.Cos((double)mem.Latitude * Math.PI / 180.0) *
                    Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var dist = earthRadiusKm * c;

            if (dist <= radiusKm)
            {
                results.Add(mem);
            }
        }

        return results.OrderByDescending(m => m.MemoryDate).ToList();
    }

    public async Task<PagedResult<Memory>> GetAllForAdminAsync(int page, int pageSize, string? keyword)
    {
        var query = _context.Memories
            .Include(m => m.User)
            .Include(m => m.Category)
            .Include(m => m.Mood)
            .Include(m => m.Photos)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var term = keyword.Trim().ToLower();
            query = query.Where(m => m.Title.ToLower().Contains(term) ||
                                     m.User.FullName.ToLower().Contains(term) ||
                                     m.User.Email.ToLower().Contains(term));
        }

        var totalItems = await query.CountAsync();
        var safePage = page < 1 ? 1 : page;
        var safePageSize = pageSize < 1 ? 10 : (pageSize > 50 ? 50 : pageSize);

        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync();

        return new PagedResult<Memory>
        {
            Page = safePage,
            PageSize = safePageSize,
            TotalItems = totalItems,
            Items = items
        };
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Memories.CountAsync();
    }

    public async Task<List<CategoryMemoryCount>> GetCategoryMemoryCountsAsync()
    {
        return await _context.Categories
            .Select(c => new CategoryMemoryCount
            {
                CategoryId = c.CategoryId,
                CategoryName = c.Name,
                MemoryCount = c.Memories.Count(m => m.Status == 1)
            })
            .ToListAsync();
    }

    public async Task<List<MonthlyMemoryCount>> GetMonthlyMemoryCountsAsync(int year)
    {
        var data = await _context.Memories
            .Where(m => m.MemoryDate.Year == year && m.Status == 1)
            .GroupBy(m => m.MemoryDate.Month)
            .Select(g => new MonthlyMemoryCount
            {
                Month = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        var result = new List<MonthlyMemoryCount>();
        for (int m = 1; m <= 12; m++)
        {
            var found = data.FirstOrDefault(d => d.Month == m);
            result.Add(new MonthlyMemoryCount { Month = m, Count = found?.Count ?? 0 });
        }
        return result;
    }
}
