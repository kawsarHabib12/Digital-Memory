using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DigitalMemoryMap.BLL.DTOs;
using DigitalMemoryMap.BLL.Exceptions;
using DigitalMemoryMap.BLL.Interfaces;
using DigitalMemoryMap.DAL.Entities;
using DigitalMemoryMap.DAL.Interfaces;

namespace DigitalMemoryMap.BLL.Services;

public class PhotoService : IPhotoService
{
    private readonly IPhotoRepository _photoRepository;
    private readonly IMemoryRepository _memoryRepository;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
    private const int MaxPhotosPerMemory = 5;

    public PhotoService(IPhotoRepository photoRepository, IMemoryRepository memoryRepository)
    {
        _photoRepository = photoRepository;
        _memoryRepository = memoryRepository;
    }

    public async Task<List<PhotoDto>> UploadPhotosAsync(int memoryId, int userId, IEnumerable<IPhotoFile> files, string baseUploadPath)
    {
        var memory = await _memoryRepository.GetByIdAndUserIdAsync(memoryId, userId);
        if (memory == null)
        {
            throw new NotFoundException("Memory not found.");
        }

        var fileList = files.ToList();
        if (fileList.Count == 0)
        {
            return new List<PhotoDto>();
        }

        var currentPhotoCount = await _photoRepository.GetPhotoCountForMemoryAsync(memoryId);
        if (currentPhotoCount + fileList.Count > MaxPhotosPerMemory)
        {
            throw new ValidationException("Photos", $"Cannot add {fileList.Count} photo(s). Maximum {MaxPhotosPerMemory} photos allowed per memory. Current count: {currentPhotoCount}.");
        }

        if (!Directory.Exists(baseUploadPath))
        {
            Directory.CreateDirectory(baseUploadPath);
        }

        var results = new List<PhotoDto>();
        var hadCover = currentPhotoCount > 0 && memory.Photos.Any(p => p.IsCover);
        var isFirstInBatch = !hadCover && currentPhotoCount == 0;

        foreach (var file in fileList)
        {
            if (file.Length > MaxFileSizeBytes)
            {
                throw new PayloadTooLargeException($"File '{file.FileName}' must be 5 MB or less.");
            }

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
            {
                throw new ValidationException("Photos", "Only JPG, PNG, and WEBP images are allowed.");
            }

            // Verify file signature (magic bytes)
            using (var stream = file.OpenReadStream())
            {
                if (!IsValidImageSignature(stream, ext))
                {
                    throw new ValidationException("Photos", $"The file '{file.FileName}' is not a valid image file.");
                }
            }

            var safeFileName = $"{Guid.NewGuid():N}{ext.ToLower()}";
            var destinationPath = Path.Combine(baseUploadPath, safeFileName);

            using (var fileStream = new FileStream(destinationPath, FileMode.Create))
            {
                using var readStream = file.OpenReadStream();
                await readStream.CopyToAsync(fileStream);
            }

            var photo = new MemoryPhoto
            {
                MemoryId = memoryId,
                FilePath = safeFileName,
                OriginalFileName = Path.GetFileName(file.FileName),
                FileSizeKb = (int)(file.Length / 1024),
                IsCover = isFirstInBatch,
                UploadedAt = DateTime.UtcNow
            };

            var saved = await _photoRepository.AddAsync(photo);
            results.Add(new PhotoDto
            {
                PhotoId = saved.PhotoId,
                Url = $"/api/photos/{saved.PhotoId}/file",
                OriginalFileName = saved.OriginalFileName,
                FileSizeKb = saved.FileSizeKb,
                IsCover = saved.IsCover,
                UploadedAt = saved.UploadedAt
            });

            isFirstInBatch = false;
        }

        return results;
    }

    public async Task DeletePhotoAsync(int photoId, int userId, string baseUploadPath)
    {
        var photo = await _photoRepository.GetByIdAsync(photoId);
        if (photo == null)
        {
            throw new NotFoundException("Photo not found.");
        }

        // Verify memory ownership
        var memory = await _memoryRepository.GetByIdAndUserIdAsync(photo.MemoryId, userId);
        if (memory == null)
        {
            throw new NotFoundException("Photo not found.");
        }

        var fullPath = Path.Combine(baseUploadPath, photo.FilePath);
        if (File.Exists(fullPath))
        {
            try
            {
                File.Delete(fullPath);
            }
            catch
            {
                // Best effort disk cleanup
            }
        }

        var wasCover = photo.IsCover;
        await _photoRepository.DeleteAsync(photo);

        if (wasCover)
        {
            var remainingPhotos = await _photoRepository.GetByMemoryIdAsync(memory.MemoryId);
            if (remainingPhotos.Count > 0)
            {
                await _photoRepository.SetCoverPhotoAsync(memory.MemoryId, remainingPhotos[0].PhotoId);
            }
        }
    }

    public async Task SetCoverPhotoAsync(int photoId, int userId)
    {
        var photo = await _photoRepository.GetByIdAsync(photoId);
        if (photo == null)
        {
            throw new NotFoundException("Photo not found.");
        }

        var memory = await _memoryRepository.GetByIdAndUserIdAsync(photo.MemoryId, userId);
        if (memory == null)
        {
            throw new NotFoundException("Photo not found.");
        }

        await _photoRepository.SetCoverPhotoAsync(photo.MemoryId, photoId);
    }

    public async Task<(string filePath, string contentType, string originalFileName)> GetPhotoFileAsync(int photoId, int currentUserId, bool isAdmin, string baseUploadPath)
    {
        var photo = await _photoRepository.GetByIdAsync(photoId);
        if (photo == null)
        {
            throw new NotFoundException("Photo not found.");
        }

        if (!isAdmin && photo.Memory.UserId != currentUserId)
        {
            throw new NotFoundException("Photo not found.");
        }

        var fullPath = Path.Combine(baseUploadPath, photo.FilePath);
        if (!File.Exists(fullPath))
        {
            throw new NotFoundException("Photo file not found on disk.");
        }

        var ext = Path.GetExtension(photo.FilePath).ToLower();
        var contentType = ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        return (fullPath, contentType, photo.OriginalFileName);
    }

    private static bool IsValidImageSignature(Stream stream, string ext)
    {
        try
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            var buffer = new byte[12];
            var bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            if (bytesRead < 4) return false;

            var lowerExt = ext.ToLower();

            // JPEG: FF D8 FF
            if (lowerExt is ".jpg" or ".jpeg")
            {
                return buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF;
            }

            // PNG: 89 50 4E 47
            if (lowerExt == ".png")
            {
                return bytesRead >= 8 &&
                       buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47 &&
                       buffer[4] == 0x0D && buffer[5] == 0x0A && buffer[6] == 0x1A && buffer[7] == 0x0A;
            }

            // WEBP: RIFF....WEBP (52 49 46 46 .... 57 45 42 50)
            if (lowerExt == ".webp")
            {
                return bytesRead >= 12 &&
                       buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46 &&
                       buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
