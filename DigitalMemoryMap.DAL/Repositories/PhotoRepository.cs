using System;
using System.Linq;
using System.Collections.Generic;
using DigitalMemoryMap.DAL.Data;
using DigitalMemoryMap.DAL.Entities;
using DigitalMemoryMap.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DigitalMemoryMap.DAL.Repositories;

public class PhotoRepository : IPhotoRepository
{
    private readonly AppDbContext _context;

    public PhotoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MemoryPhoto?> GetByIdAsync(int photoId)
    {
        return await _context.MemoryPhotos
            .Include(p => p.Memory)
            .FirstOrDefaultAsync(p => p.PhotoId == photoId);
    }

    public async Task<List<MemoryPhoto>> GetByMemoryIdAsync(int memoryId)
    {
        return await _context.MemoryPhotos
            .Where(p => p.MemoryId == memoryId)
            .OrderByDescending(p => p.IsCover)
            .ThenBy(p => p.UploadedAt)
            .ToListAsync();
    }

    public async Task<MemoryPhoto> AddAsync(MemoryPhoto photo)
    {
        photo.UploadedAt = DateTime.UtcNow;
        _context.MemoryPhotos.Add(photo);
        await _context.SaveChangesAsync();
        return photo;
    }

    public async Task DeleteAsync(MemoryPhoto photo)
    {
        _context.MemoryPhotos.Remove(photo);
        await _context.SaveChangesAsync();
    }

    public async Task SetCoverPhotoAsync(int memoryId, int photoId)
    {
        var photos = await _context.MemoryPhotos
            .Where(p => p.MemoryId == memoryId)
            .ToListAsync();

        foreach (var p in photos)
        {
            p.IsCover = (p.PhotoId == photoId);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<int> GetPhotoCountForMemoryAsync(int memoryId)
    {
        return await _context.MemoryPhotos.CountAsync(p => p.MemoryId == memoryId);
    }

    public async Task<int> GetTotalPhotoCountAsync()
    {
        return await _context.MemoryPhotos.CountAsync();
    }
}
