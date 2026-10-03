using DigitalMemoryMap.BLL.DTOs;
using DigitalMemoryMap.DAL.Entities;

namespace DigitalMemoryMap.BLL.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}

public interface IUserService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
    Task ChangePasswordAsync(int userId, ChangePasswordRequestDto dto);
    Task<UserProfileDto> GetProfileAsync(int userId);
    Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);
}

public interface IMemoryService
{
    Task<MemoryDetailsDto> CreateMemoryAsync(int userId, CreateMemoryDto dto);
    Task<MemoryDetailsDto> GetMemoryDetailsAsync(int memoryId, int userId);
    Task<MemoryDetailsDto> UpdateMemoryAsync(int memoryId, int userId, UpdateMemoryDto dto);
    Task DeleteMemoryAsync(int memoryId, int userId);
    Task<PagedResultDto<MemoryListItemDto>> GetPagedMemoriesAsync(MemoryFilterRequestDto filter);
    Task<List<MapPinDto>> GetMapPinsAsync(int userId, int? categoryId, byte? moodId, DateTime? from, DateTime? to);
    Task<List<MemoryListItemDto>> GetNearbyMemoriesAsync(int userId, decimal lat, decimal lng, double radiusKm);
}

public class MemoryFilterRequestDto
{
    public int UserId { get; set; }
    public string? Keyword { get; set; }
    public string? Location { get; set; }
    public DateTime? Date { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? CategoryId { get; set; }
    public byte? MoodId { get; set; }
    public string? Tag { get; set; }
    public string? Sort { get; set; } = "date_desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class PagedResultDto<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / (PageSize > 0 ? PageSize : 1));
    public List<T> Items { get; set; } = new();
}

public interface IPhotoFile
{
    string FileName { get; }
    long Length { get; }
    string ContentType { get; }
    Stream OpenReadStream();
}

public interface IPhotoService
{
    Task<List<PhotoDto>> UploadPhotosAsync(int memoryId, int userId, IEnumerable<IPhotoFile> files, string baseUploadPath);
    Task DeletePhotoAsync(int photoId, int userId, string baseUploadPath);
    Task SetCoverPhotoAsync(int photoId, int userId);
    Task<(string filePath, string contentType, string originalFileName)> GetPhotoFileAsync(int photoId, int currentUserId, bool isAdmin, string baseUploadPath);
}

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllActiveAsync();
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
    Task<CategoryDto> UpdateAsync(int categoryId, UpdateCategoryDto dto);
    Task DeleteAsync(int categoryId);
}

public interface ITagService
{
    Task<List<TagDto>> GetTagsForUserAsync(int userId);
}

public interface IMoodService
{
    Task<List<MoodDto>> GetAllMoodsAsync();
}

public interface IAdminService
{
    Task<AdminStatsDto> GetStatsAsync();
    Task<PagedResultDto<AdminUserListItemDto>> GetUsersAsync(string? search, int page, int pageSize);
    Task UpdateUserStatusAsync(int userId, bool isActive);
    Task<PagedResultDto<MemoryListItemDto>> GetAllMemoriesAsync(int page, int pageSize, string? keyword);
    Task RemoveMemoryAsync(int memoryId);
}
