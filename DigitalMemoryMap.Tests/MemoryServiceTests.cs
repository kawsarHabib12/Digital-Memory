using DigitalMemoryMap.BLL.DTOs;
using DigitalMemoryMap.BLL.Exceptions;
using DigitalMemoryMap.BLL.Services;
using DigitalMemoryMap.DAL.Entities;
using DigitalMemoryMap.DAL.Interfaces;
using Moq;
using Xunit;

namespace DigitalMemoryMap.Tests;

public class MemoryServiceTests
{
    private readonly Mock<IMemoryRepository> _memoryRepoMock = new();
    private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
    private readonly Mock<IMoodRepository> _moodRepoMock = new();
    private readonly Mock<ITagRepository> _tagRepoMock = new();
    private readonly MemoryService _memoryService;

    public MemoryServiceTests()
    {
        _memoryService = new MemoryService(
            _memoryRepoMock.Object,
            _categoryRepoMock.Object,
            _moodRepoMock.Object,
            _tagRepoMock.Object);
    }

    [Fact]
    public async Task CreateMemory_ValidData_CreatesMemory()
    {
        // Arrange
        var userId = 10;
        var dto = new CreateMemoryDto
        {
            Title = "Sunset in Cox's Bazar",
            Description = "Walking on the beach.",
            MemoryDate = DateTime.UtcNow.AddDays(-1),
            Latitude = 21.4272m,
            Longitude = 91.9788m,
            LocationName = "Cox's Bazar",
            CategoryId = 1,
            MoodId = 1,
            Tags = new List<string> { "beach", "sunset" }
        };

        _categoryRepoMock.Setup(c => c.GetByIdAsync(1)).ReturnsAsync(new Category { CategoryId = 1, Name = "Travel", IsActive = true });
        _moodRepoMock.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(new Mood { MoodId = 1, Name = "Happy", Emoji = "😊" });
        _tagRepoMock.Setup(t => t.GetOrCreateTagsAsync(userId, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<Tag> { new() { TagId = 1, Name = "beach" }, new() { TagId = 2, Name = "sunset" } });

        var createdMemory = new Memory
        {
            MemoryId = 42,
            UserId = userId,
            Title = dto.Title,
            Description = dto.Description,
            MemoryDate = dto.MemoryDate.Date,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            LocationName = dto.LocationName,
            CategoryId = 1,
            Category = new Category { Name = "Travel" },
            Mood = new Mood { MoodId = 1, Name = "Happy", Emoji = "😊" },
            Status = 1
        };

        _memoryRepoMock.Setup(r => r.AddAsync(It.IsAny<Memory>())).ReturnsAsync(createdMemory);
        _memoryRepoMock.Setup(r => r.GetByIdAndUserIdAsync(42, userId, false)).ReturnsAsync(createdMemory);

        // Act
        var result = await _memoryService.CreateMemoryAsync(userId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Sunset in Cox's Bazar", result.Title);
        Assert.Equal("Travel", result.Category);
    }

    [Fact]
    public async Task CreateMemory_MissingTitle_ThrowsValidationException()
    {
        // Arrange
        var dto = new CreateMemoryDto
        {
            Title = " ",
            MemoryDate = DateTime.UtcNow.AddDays(-1),
            Latitude = 21.0m,
            Longitude = 90.0m,
            CategoryId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _memoryService.CreateMemoryAsync(1, dto));
    }

    [Fact]
    public async Task CreateMemory_InvalidCoordinates_ThrowsValidationException()
    {
        // Arrange: Latitude > 90
        var dto = new CreateMemoryDto
        {
            Title = "Laboni Beach",
            MemoryDate = DateTime.UtcNow.AddDays(-1),
            Latitude = 95.0m,
            Longitude = 90.0m,
            CategoryId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _memoryService.CreateMemoryAsync(1, dto));
    }

    [Fact]
    public async Task CreateMemory_FutureDate_ThrowsValidationException()
    {
        // Arrange: Tomorrow's date
        var dto = new CreateMemoryDto
        {
            Title = "Laboni Beach",
            MemoryDate = DateTime.UtcNow.AddDays(5),
            Latitude = 21.0m,
            Longitude = 90.0m,
            CategoryId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _memoryService.CreateMemoryAsync(1, dto));
    }

    [Fact]
    public async Task CreateMemory_TooManyTags_ThrowsValidationException()
    {
        // Arrange: 11 tags
        var dto = new CreateMemoryDto
        {
            Title = "Laboni Beach",
            MemoryDate = DateTime.UtcNow.AddDays(-1),
            Latitude = 21.0m,
            Longitude = 90.0m,
            CategoryId = 1,
            Tags = Enumerable.Range(1, 11).Select(i => $"tag{i}").ToList()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _memoryService.CreateMemoryAsync(1, dto));
    }

    [Fact]
    public async Task GetMemory_AnotherUsersId_ThrowsNotFoundException()
    {
        // Arrange: User 2 tries to access User 1's memory
        _memoryRepoMock.Setup(r => r.GetByIdAndUserIdAsync(100, 2, false)).ReturnsAsync((Memory?)null);

        // Act & Assert (PRD requirement: User A accessing User B's memory gets 404)
        await Assert.ThrowsAsync<NotFoundException>(() => _memoryService.GetMemoryDetailsAsync(100, 2));
    }

    [Fact]
    public async Task DeleteMemory_AnotherUsersId_ThrowsNotFoundException()
    {
        // Arrange: User 2 tries to delete User 1's memory
        _memoryRepoMock.Setup(r => r.GetByIdAndUserIdAsync(100, 2, false)).ReturnsAsync((Memory?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _memoryService.DeleteMemoryAsync(100, 2));
    }

    [Fact]
    public async Task DeleteMemory_OwnMemory_DeletesSuccessfully()
    {
        // Arrange
        var ownMemory = new Memory { MemoryId = 100, UserId = 1 };
        _memoryRepoMock.Setup(r => r.GetByIdAndUserIdAsync(100, 1, false)).ReturnsAsync(ownMemory);
        _memoryRepoMock.Setup(r => r.DeleteAsync(ownMemory)).Returns(Task.CompletedTask);

        // Act
        await _memoryService.DeleteMemoryAsync(100, 1);

        // Assert
        _memoryRepoMock.Verify(r => r.DeleteAsync(ownMemory), Times.Once);
    }
}
