using DigitalMemoryMap.BLL.Exceptions;
using DigitalMemoryMap.BLL.Interfaces;
using DigitalMemoryMap.BLL.Services;
using DigitalMemoryMap.DAL.Entities;
using DigitalMemoryMap.DAL.Interfaces;
using Moq;
using Xunit;

namespace DigitalMemoryMap.Tests;

public class PhotoServiceTests
{
    private readonly Mock<IPhotoRepository> _photoRepoMock = new();
    private readonly Mock<IMemoryRepository> _memoryRepoMock = new();
    private readonly PhotoService _photoService;

    public PhotoServiceTests()
    {
        _photoService = new PhotoService(_photoRepoMock.Object, _memoryRepoMock.Object);
    }

    private class MockPhotoFile : IPhotoFile
    {
        public string FileName { get; set; } = string.Empty;
        public long Length { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public Stream OpenReadStream() => new MemoryStream(Data);
    }

    [Fact]
    public async Task UploadPhotos_FileTooLarge_ThrowsPayloadTooLargeException()
    {
        // Arrange: 6 MB file
        _memoryRepoMock.Setup(r => r.GetByIdAndUserIdAsync(1, 1, false)).ReturnsAsync(new Memory { MemoryId = 1, UserId = 1 });
        _photoRepoMock.Setup(r => r.GetPhotoCountForMemoryAsync(1)).ReturnsAsync(0);

        var largeFile = new MockPhotoFile
        {
            FileName = "beach.jpg",
            Length = 6 * 1024 * 1024, // 6 MB > 5 MB limit
            Data = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }
        };

        // Act & Assert
        await Assert.ThrowsAsync<PayloadTooLargeException>(() =>
            _photoService.UploadPhotosAsync(1, 1, new[] { largeFile }, Path.GetTempPath()));
    }

    [Fact]
    public async Task UploadPhotos_InvalidExtension_ThrowsValidationException()
    {
        // Arrange
        _memoryRepoMock.Setup(r => r.GetByIdAndUserIdAsync(1, 1, false)).ReturnsAsync(new Memory { MemoryId = 1, UserId = 1 });
        _photoRepoMock.Setup(r => r.GetPhotoCountForMemoryAsync(1)).ReturnsAsync(0);

        var exeFile = new MockPhotoFile
        {
            FileName = "malware.exe",
            Length = 1024,
            Data = new byte[] { 0x4D, 0x5A }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _photoService.UploadPhotosAsync(1, 1, new[] { exeFile }, Path.GetTempPath()));
    }

    [Fact]
    public async Task UploadPhotos_ExceedsMaxCount_ThrowsValidationException()
    {
        // Arrange: Already has 4 photos, trying to add 2 (total 6 > 5)
        _memoryRepoMock.Setup(r => r.GetByIdAndUserIdAsync(1, 1, false)).ReturnsAsync(new Memory { MemoryId = 1, UserId = 1 });
        _photoRepoMock.Setup(r => r.GetPhotoCountForMemoryAsync(1)).ReturnsAsync(4);

        var file1 = new MockPhotoFile { FileName = "p1.jpg", Length = 100 };
        var file2 = new MockPhotoFile { FileName = "p2.jpg", Length = 100 };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _photoService.UploadPhotosAsync(1, 1, new[] { file1, file2 }, Path.GetTempPath()));
    }

    [Fact]
    public async Task GetPhotoFile_AnotherUserPhoto_ThrowsNotFoundException()
    {
        // Arrange: User 2 tries to download User 1's private photo
        var photo = new MemoryPhoto
        {
            PhotoId = 5,
            MemoryId = 1,
            Memory = new Memory { UserId = 1 }
        };

        _photoRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(photo);

        // Act & Assert (PRD requirement: T20 - open another user's photo URL returns 404)
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _photoService.GetPhotoFileAsync(5, currentUserId: 2, isAdmin: false, baseUploadPath: Path.GetTempPath()));
    }
}
