using DigitalMemoryMap.DAL.Data;
using DigitalMemoryMap.DAL.Entities;
using DigitalMemoryMap.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DigitalMemoryMap.DAL.Repositories;

public class MoodRepository : IMoodRepository
{
    private readonly AppDbContext _context;

    public MoodRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Mood>> GetAllAsync()
    {
        return await _context.Moods
            .OrderBy(m => m.MoodId)
            .ToListAsync();
    }

    public async Task<Mood?> GetByIdAsync(byte moodId)
    {
        return await _context.Moods.FindAsync(moodId);
    }
}
