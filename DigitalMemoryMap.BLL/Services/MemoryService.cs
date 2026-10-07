using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DigitalMemoryMap.BLL.DTOs;
using DigitalMemoryMap.BLL.Exceptions;
using DigitalMemoryMap.BLL.Interfaces;
using DigitalMemoryMap.DAL.Entities;
using DigitalMemoryMap.DAL.Interfaces;

namespace DigitalMemoryMap.BLL.Services;

public class MemoryService : IMemoryService
{
    private readonly IMemoryRepository _memoryRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMoodRepository _moodRepository;
    private readonly ITagRepository _tagRepository;

    public MemoryService(
        IMemoryRepository memoryRepository,
        ICategoryRepository categoryRepository,
        IMoodRepository moodRepository,
        ITagRepository tagRepository)
    {
        _memoryRepository = memoryRepository;
        _categoryRepository = categoryRepository;
        _moodRepository = moodRepository;
        _tagRepository = tagRepository;
    }

    public async Task<MemoryDetailsDto> CreateMemoryAsync(int userId, CreateMemoryDto dto)
    {
        ValidateMemoryInput(dto.Title, dto.Description, dto.MemoryDate, dto.Latitude, dto.Longitude, dto.Tags);

        // Verify category exists and is active
        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (category == null || !category.IsActive)
        {
            throw new ValidationException("CategoryId", "The selected category does not exist or is inactive.");
        }

        // Verify mood if provided
        Mood? mood = null;
        if (dto.MoodId.HasValue)
        {
            mood = await _moodRepository.GetByIdAsync(dto.MoodId.Value);
            if (mood == null)
            {
                throw new ValidationException("MoodId", "The selected mood does not exist.");
            }
        }

        // Handle tags
        var sanitizedTags = SanitizeTags(dto.Tags);
        var tags = await _tagRepository.GetOrCreateTagsAsync(userId, sanitizedTags);

        var memory = new Memory
        {
            UserId = userId,
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            MemoryDate = dto.MemoryDate.Date,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            LocationName = dto.LocationName?.Trim(),
            CategoryId = dto.CategoryId,
            MoodId = dto.MoodId,
            Visibility = dto.Visibility,
            Status = 1, // Active
            CreatedAt = DateTime.UtcNow,
            MemoryTags = tags.Select(t => new MemoryTag { TagId = t.TagId }).ToList()
        };

        var saved = await _memoryRepository.AddAsync(memory);
        return await GetMemoryDetailsAsync(saved.MemoryId, userId);
    }

    public async Task<MemoryDetailsDto> GetMemoryDetailsAsync(int memoryId, int? userId)
    {
        Memory? memory;
        if (!userId.HasValue || userId.Value == 0)
        {
            memory = await _memoryRepository.GetByIdAsync(memoryId);
        }
        else
        {
            memory = await _memoryRepository.GetByIdAndUserIdAsync(memoryId, userId.Value);
        }

        if (memory == null)
        {
            throw new NotFoundException("Memory not found.");
        }

        return MapToDetailsDto(memory);
    }

    public async Task<MemoryDetailsDto> UpdateMemoryAsync(int memoryId, int userId, UpdateMemoryDto dto)
    {
        ValidateMemoryInput(dto.Title, dto.Description, dto.MemoryDate, dto.Latitude, dto.Longitude, dto.Tags);

        var memory = await _memoryRepository.GetByIdAndUserIdAsync(memoryId, userId);
        if (memory == null)
        {
            throw new NotFoundException("Memory not found.");
        }

        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (category == null || !category.IsActive)
        {
            throw new ValidationException("CategoryId", "The selected category does not exist or is inactive.");
        }

        if (dto.MoodId.HasValue)
        {
            var mood = await _moodRepository.GetByIdAsync(dto.MoodId.Value);
            if (mood == null)
            {
                throw new ValidationException("MoodId", "The selected mood does not exist.");
            }
        }

        memory.Title = dto.Title.Trim();
        memory.Description = dto.Description?.Trim();
        memory.MemoryDate = dto.MemoryDate.Date;
        memory.Latitude = dto.Latitude;
        memory.Longitude = dto.Longitude;
        memory.LocationName = dto.LocationName?.Trim();
        memory.CategoryId = dto.CategoryId;
        memory.MoodId = dto.MoodId;
        memory.Visibility = dto.Visibility;
        memory.UpdatedAt = DateTime.UtcNow;

        // Update tags
        var sanitizedTags = SanitizeTags(dto.Tags);
        var tags = await _tagRepository.GetOrCreateTagsAsync(userId, sanitizedTags);

        memory.MemoryTags.Clear();
        foreach (var tag in tags)
        {
            memory.MemoryTags.Add(new MemoryTag { MemoryId = memory.MemoryId, TagId = tag.TagId });
        }

        await _memoryRepository.UpdateAsync(memory);

        return await GetMemoryDetailsAsync(memoryId, userId);
    }

    public async Task DeleteMemoryAsync(int memoryId, int userId)
    {
        var memory = await _memoryRepository.GetByIdAndUserIdAsync(memoryId, userId);
        if (memory == null)
        {
            throw new NotFoundException("Memory not found.");
        }

        await _memoryRepository.DeleteAsync(memory);
    }

    public async Task<PagedResultDto<MemoryListItemDto>> GetPagedMemoriesAsync(MemoryFilterRequestDto filter)
    {
        var dalFilter = new MemoryFilterParams
        {
            UserId = filter.UserId,
            Keyword = filter.Keyword,
            Location = filter.Location,
            Date = filter.Date,
            From = filter.From,
            To = filter.To,
            CategoryId = filter.CategoryId,
            MoodId = filter.MoodId,
            Tag = filter.Tag,
            Sort = filter.Sort,
            Page = filter.Page,
            PageSize = filter.PageSize
        };

        var paged = await _memoryRepository.GetPagedMemoriesAsync(dalFilter);

        return new PagedResultDto<MemoryListItemDto>
        {
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalItems = paged.TotalItems,
            Items = paged.Items.Select(MapToListItemDto).ToList()
        };
    }

    public async Task<List<MapPinDto>> GetMapPinsAsync(int? userId, int? categoryId, byte? moodId, DateTime? from, DateTime? to)
    {
        var memories = await _memoryRepository.GetMapPinsAsync(userId, categoryId, moodId, from, to);

        return memories.Select(m =>
        {
            var coverPhoto = m.Photos.FirstOrDefault(p => p.IsCover) ?? m.Photos.FirstOrDefault();
            return new MapPinDto
            {
                MemoryId = m.MemoryId,
                Title = m.Title,
                MemoryDate = m.MemoryDate,
                Latitude = m.Latitude,
                Longitude = m.Longitude,
                Category = m.Category?.Name ?? string.Empty,
                MoodEmoji = m.Mood?.Emoji,
                ThumbnailUrl = coverPhoto != null ? $"/api/photos/{coverPhoto.PhotoId}/file" : null
            };
        }).ToList();
    }

    public async Task<List<MemoryListItemDto>> GetNearbyMemoriesAsync(int userId, decimal lat, decimal lng, double radiusKm)
    {
        if (radiusKm <= 0) radiusKm = 10;
        var memories = await _memoryRepository.GetNearbyMemoriesAsync(userId, lat, lng, radiusKm);
        return memories.Select(MapToListItemDto).ToList();
    }

    public async Task<List<JourneyPointDto>> GetJourneyPointsAsync(int userId)
    {
        var memories = await _memoryRepository.GetJourneyMemoriesAsync(userId);
        return memories.Select(m =>
        {
            var coverPhoto = m.Photos.FirstOrDefault(p => p.IsCover) ?? m.Photos.FirstOrDefault();
            return new JourneyPointDto
            {
                MemoryId = m.MemoryId,
                Title = m.Title,
                Description = m.Description,
                MemoryDate = m.MemoryDate,
                Latitude = m.Latitude,
                Longitude = m.Longitude,
                LocationName = m.LocationName,
                CategoryId = m.CategoryId,
                Category = m.Category?.Name ?? string.Empty,
                MoodEmoji = m.Mood?.Emoji,
                MoodName = m.Mood?.Name,
                ThumbnailUrl = coverPhoto != null ? $"/api/photos/{coverPhoto.PhotoId}/file" : null
            };
        }).ToList();
    }

    private static void ValidateMemoryInput(string title, string? description, DateTime date, decimal lat, decimal lng, List<string>? tags)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length < 3 || title.Trim().Length > 100)
        {
            throw new ValidationException("Title", "Title must be between 3 and 100 characters.");
        }

        if (description != null && description.Length > 2000)
        {
            throw new ValidationException("Description", "Description cannot exceed 2000 characters.");
        }

        if (date.Date > DateTime.UtcNow.Date)
        {
            throw new ValidationException("MemoryDate", "Memory date cannot be in the future.");
        }

        if (date.Year < 1900)
        {
            throw new ValidationException("MemoryDate", "Memory date cannot be before year 1900.");
        }

        if (lat < -90m || lat > 90m)
        {
            throw new ValidationException("Latitude", "Latitude must be between -90 and 90.");
        }

        if (lng < -180m || lng > 180m)
        {
            throw new ValidationException("Longitude", "Longitude must be between -180 and 180.");
        }

        if (tags != null)
        {
            if (tags.Count > 10)
            {
                throw new ValidationException("Tags", "A memory cannot have more than 10 tags.");
            }

            foreach (var tag in tags)
            {
                var clean = tag.Trim();
                if (clean.Length < 2 || clean.Length > 30)
                {
                    throw new ValidationException("Tags", $"Tag '{clean}' must be between 2 and 30 characters.");
                }

                if (!Regex.IsMatch(clean, @"^[a-zA-Z0-9_\-]+$"))
                {
                    throw new ValidationException("Tags", $"Tag '{clean}' contains invalid characters. Use letters, numbers, hyphens, and underscores only.");
                }
            }
        }
    }

    private static List<string> SanitizeTags(IEnumerable<string>? tags)
    {
        if (tags == null) return new List<string>();
        return tags
            .Select(t => t.Trim().ToLower())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct()
            .Take(10)
            .ToList();
    }

    private static MemoryDetailsDto MapToDetailsDto(Memory m)
    {
        return new MemoryDetailsDto
        {
            MemoryId = m.MemoryId,
            Title = m.Title,
            Description = m.Description,
            MemoryDate = m.MemoryDate,
            Latitude = m.Latitude,
            Longitude = m.Longitude,
            LocationName = m.LocationName,
            CategoryId = m.CategoryId,
            Category = m.Category?.Name ?? string.Empty,
            Mood = m.Mood != null ? new MoodDto { MoodId = m.Mood.MoodId, Name = m.Mood.Name, Emoji = m.Mood.Emoji } : null,
            Tags = m.MemoryTags.Select(mt => mt.Tag.Name).ToList(),
            Photos = m.Photos.Select(p => new PhotoDto
            {
                PhotoId = p.PhotoId,
                Url = $"/api/photos/{p.PhotoId}/file",
                OriginalFileName = p.OriginalFileName,
                FileSizeKb = p.FileSizeKb,
                IsCover = p.IsCover,
                UploadedAt = p.UploadedAt
            }).ToList(),
            Visibility = m.Visibility,
            Status = m.Status,
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt
        };
    }

    private static MemoryListItemDto MapToListItemDto(Memory m)
    {
        var coverPhoto = m.Photos.FirstOrDefault(p => p.IsCover) ?? m.Photos.FirstOrDefault();
        var snippet = !string.IsNullOrWhiteSpace(m.Description)
            ? (m.Description.Length > 120 ? m.Description[..120] + "..." : m.Description)
            : m.Title;

        return new MemoryListItemDto
        {
            MemoryId = m.MemoryId,
            Title = m.Title,
            MemoryDate = m.MemoryDate,
            LocationName = m.LocationName,
            Snippet = snippet,
            CoverPhotoUrl = coverPhoto != null ? $"/api/photos/{coverPhoto.PhotoId}/file" : null,
            Category = m.Category?.Name ?? string.Empty,
            MoodEmoji = m.Mood?.Emoji
        };
    }
}
