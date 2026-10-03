using DigitalMemoryMap.DAL.Data;
using DigitalMemoryMap.DAL.Entities;
using DigitalMemoryMap.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DigitalMemoryMap.DAL.Repositories;

public class TagRepository : ITagRepository
{
    private readonly AppDbContext _context;

    public TagRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Tag>> GetByUserIdAsync(int userId)
    {
        return await _context.Tags
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<Tag?> GetByUserIdAndNameAsync(int userId, string name)
    {
        var normalized = name.Trim().ToLower();
        return await _context.Tags
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Name.ToLower() == normalized);
    }

    public async Task<List<Tag>> GetOrCreateTagsAsync(int userId, IEnumerable<string> tagNames)
    {
        var distinctNames = tagNames
            .Select(t => t.Trim().ToLower())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct()
            .ToList();

        if (distinctNames.Count == 0)
        {
            return new List<Tag>();
        }

        var existingTags = await _context.Tags
            .Where(t => t.UserId == userId && distinctNames.Contains(t.Name.ToLower()))
            .ToListAsync();

        var existingNames = existingTags.Select(t => t.Name.ToLower()).ToHashSet();
        var newTags = new List<Tag>();

        foreach (var name in distinctNames)
        {
            if (!existingNames.Contains(name))
            {
                var newTag = new Tag
                {
                    UserId = userId,
                    Name = name
                };
                newTags.Add(newTag);
                _context.Tags.Add(newTag);
            }
        }

        if (newTags.Count > 0)
        {
            await _context.SaveChangesAsync();
        }

        return existingTags.Concat(newTags).ToList();
    }

    public async Task<Tag> AddAsync(Tag tag)
    {
        tag.Name = tag.Name.Trim().ToLower();
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return tag;
    }
}
