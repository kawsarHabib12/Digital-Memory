using System.ComponentModel.DataAnnotations;

namespace DigitalMemoryMap.BLL.DTOs;

public class CreateMemoryDto
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Memory date is required.")]
    public DateTime MemoryDate { get; set; }

    [Required(ErrorMessage = "Latitude is required.")]
    [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90.")]
    public decimal Latitude { get; set; }

    [Required(ErrorMessage = "Longitude is required.")]
    [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180.")]
    public decimal Longitude { get; set; }

    [StringLength(200, ErrorMessage = "Location name cannot exceed 200 characters.")]
    public string? LocationName { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category.")]
    public int CategoryId { get; set; }

    public byte? MoodId { get; set; }

    public List<string> Tags { get; set; } = new();

    public byte Visibility { get; set; } = 0; // 0 Private, 1 Public
}

public class UpdateMemoryDto
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Memory date is required.")]
    public DateTime MemoryDate { get; set; }

    [Required(ErrorMessage = "Latitude is required.")]
    [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90.")]
    public decimal Latitude { get; set; }

    [Required(ErrorMessage = "Longitude is required.")]
    [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180.")]
    public decimal Longitude { get; set; }

    [StringLength(200, ErrorMessage = "Location name cannot exceed 200 characters.")]
    public string? LocationName { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category.")]
    public int CategoryId { get; set; }

    public byte? MoodId { get; set; }

    public List<string> Tags { get; set; } = new();

    public byte Visibility { get; set; } = 0;
}

public class MemoryDetailsDto
{
    public int MemoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime MemoryDate { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? LocationName { get; set; }
    public int CategoryId { get; set; }
    public string Category { get; set; } = string.Empty;
    public MoodDto? Mood { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<PhotoDto> Photos { get; set; } = new();
    public byte Visibility { get; set; }
    public byte Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class MemoryListItemDto
{
    public int MemoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime MemoryDate { get; set; }
    public string? LocationName { get; set; }
    public string Snippet { get; set; } = string.Empty;
    public string? CoverPhotoUrl { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? MoodEmoji { get; set; }
}

public class MapPinDto
{
    public int MemoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime MemoryDate { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? MoodEmoji { get; set; }
    public string? ThumbnailUrl { get; set; }
}
