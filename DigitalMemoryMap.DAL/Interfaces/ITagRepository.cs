using DigitalMemoryMap.DAL.Entities;

namespace DigitalMemoryMap.DAL.Interfaces;

public interface ITagRepository
{
    Task<List<Tag>> GetByUserIdAsync(int userId);
    Task<Tag?> GetByUserIdAndNameAsync(int userId, string name);
    Task<List<Tag>> GetOrCreateTagsAsync(int userId, IEnumerable<string> tagNames);
    Task<Tag> AddAsync(Tag tag);
}
