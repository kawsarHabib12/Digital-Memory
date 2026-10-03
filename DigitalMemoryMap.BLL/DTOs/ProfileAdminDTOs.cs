using System.ComponentModel.DataAnnotations;

namespace DigitalMemoryMap.BLL.DTOs;

public class UserProfileDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? ProfilePhotoPath { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateProfileDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "Bio must not exceed 300 characters.")]
    public string? Bio { get; set; }
}

public class AdminStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalMemories { get; set; }
    public int TotalPhotos { get; set; }
    public int NewUsersThisWeek { get; set; }
    public int ActiveUsers { get; set; }
    public List<CategoryMemoryStatDto> CategoryStats { get; set; } = new();
    public List<MonthlyStatDto> MonthlyStats { get; set; } = new();
}

public class CategoryMemoryStatDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int MemoryCount { get; set; }
}

public class MonthlyStatDto
{
    public int Month { get; set; }
    public int Count { get; set; }
}

public class AdminUserListItemDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MemoryCount { get; set; }
}

public class UpdateUserStatusDto
{
    public bool IsActive { get; set; }
}
