using DigitalMemoryMap.DAL.Entities;

namespace DigitalMemoryMap.DAL.Interfaces;

public interface IPhotoRepository
{
    Task<MemoryPhoto?> GetByIdAsync(int photoId);
    Task<List<MemoryPhoto>> GetByMemoryIdAsync(int memoryId);
    Task<MemoryPhoto> AddAsync(MemoryPhoto photo);
    Task DeleteAsync(MemoryPhoto photo);
    Task SetCoverPhotoAsync(int memoryId, int photoId);
    Task<int> GetPhotoCountForMemoryAsync(int memoryId);
    Task<int> GetTotalPhotoCountAsync();
}
