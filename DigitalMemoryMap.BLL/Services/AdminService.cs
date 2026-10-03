using System;
using System.Linq;
using DigitalMemoryMap.BLL.DTOs;
using DigitalMemoryMap.BLL.Exceptions;
using DigitalMemoryMap.BLL.Interfaces;
using DigitalMemoryMap.DAL.Entities;
using DigitalMemoryMap.DAL.Interfaces;

namespace DigitalMemoryMap.BLL.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly IMemoryRepository _memoryRepository;
    private readonly IPhotoRepository _photoRepository;

    public AdminService(
        IUserRepository userRepository,
        IMemoryRepository memoryRepository,
        IPhotoRepository photoRepository)
    {
        _userRepository = userRepository;
        _memoryRepository = memoryRepository;
        _photoRepository = photoRepository;
    }

    public async Task<AdminStatsDto> GetStatsAsync()
    {
        var totalUsers = await _userRepository.CountAsync(null);
        var activeUsers = await _userRepository.CountActiveUsersAsync();
        var newUsersThisWeek = await _userRepository.GetNewUsersThisWeekCountAsync();
        var totalMemories = await _memoryRepository.GetTotalCountAsync();
        var totalPhotos = await _photoRepository.GetTotalPhotoCountAsync();
        var categoryCounts = await _memoryRepository.GetCategoryMemoryCountsAsync();
        var monthlyCounts = await _memoryRepository.GetMonthlyMemoryCountsAsync(DateTime.UtcNow.Year);

        return new AdminStatsDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            NewUsersThisWeek = newUsersThisWeek,
            TotalMemories = totalMemories,
            TotalPhotos = totalPhotos,
            CategoryStats = categoryCounts.Select(c => new CategoryMemoryStatDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                MemoryCount = c.MemoryCount
            }).ToList(),
            MonthlyStats = monthlyCounts.Select(m => new MonthlyStatDto
            {
                Month = m.Month,
                Count = m.Count
            }).ToList()
        };
    }

    public async Task<PagedResultDto<AdminUserListItemDto>> GetUsersAsync(string? search, int page, int pageSize)
    {
        var safePage = page < 1 ? 1 : page;
        var safePageSize = pageSize < 1 ? 10 : pageSize;

        var users = await _userRepository.GetAllAsync(search, safePage, safePageSize);
        var total = await _userRepository.CountAsync(search);

        var items = users.Select(u => new AdminUserListItemDto
        {
            UserId = u.UserId,
            FullName = u.FullName,
            Email = u.Email,
            Role = u.Role?.RoleName ?? "User",
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            MemoryCount = u.Memories.Count(m => m.Status == 1)
        }).ToList();

        return new PagedResultDto<AdminUserListItemDto>
        {
            Page = safePage,
            PageSize = safePageSize,
            TotalItems = total,
            Items = items
        };
    }

    public async Task UpdateUserStatusAsync(int userId, bool isActive)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        user.IsActive = isActive;
        await _userRepository.UpdateAsync(user);
    }

    public async Task<PagedResultDto<MemoryListItemDto>> GetAllMemoriesAsync(int page, int pageSize, string? keyword)
    {
        var paged = await _memoryRepository.GetAllForAdminAsync(page, pageSize, keyword);

        var items = paged.Items.Select(m =>
        {
            var cover = m.Photos.FirstOrDefault(p => p.IsCover) ?? m.Photos.FirstOrDefault();
            return new MemoryListItemDto
            {
                MemoryId = m.MemoryId,
                Title = m.Title,
                MemoryDate = m.MemoryDate,
                LocationName = m.LocationName,
                Snippet = !string.IsNullOrWhiteSpace(m.Description)
                    ? (m.Description.Length > 100 ? m.Description[..100] + "..." : m.Description)
                    : m.Title,
                CoverPhotoUrl = cover != null ? $"/api/photos/{cover.PhotoId}/file" : null,
                Category = m.Category?.Name ?? string.Empty,
                MoodEmoji = m.Mood?.Emoji
            };
        }).ToList();

        return new PagedResultDto<MemoryListItemDto>
        {
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalItems = paged.TotalItems,
            Items = items
        };
    }

    public async Task RemoveMemoryAsync(int memoryId)
    {
        var memory = await _memoryRepository.GetByIdAsync(memoryId);
        if (memory == null)
        {
            throw new NotFoundException("Memory not found.");
        }

        // Soft remove (sets Status = 2 RemovedByAdmin)
        memory.Status = 2;
        await _memoryRepository.UpdateAsync(memory);
    }
}
