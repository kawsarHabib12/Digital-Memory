using DigitalMemoryMap.DAL.Entities;

namespace DigitalMemoryMap.DAL.Interfaces;

public interface IMoodRepository
{
    Task<List<Mood>> GetAllAsync();
    Task<Mood?> GetByIdAsync(byte moodId);
}
