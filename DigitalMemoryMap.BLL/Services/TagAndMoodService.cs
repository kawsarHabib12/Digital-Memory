using System;
using System.Linq;
using System.Collections.Generic;
using DigitalMemoryMap.BLL.DTOs;
using DigitalMemoryMap.BLL.Interfaces;
using DigitalMemoryMap.DAL.Interfaces;

namespace DigitalMemoryMap.BLL.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;

    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<List<TagDto>> GetTagsForUserAsync(int userId)
    {
        var tags = await _tagRepository.GetByUserIdAsync(userId);
        return tags.Select(t => new TagDto
        {
            TagId = t.TagId,
            Name = t.Name
        }).ToList();
    }
}

public class MoodService : IMoodService
{
    private readonly IMoodRepository _moodRepository;

    public MoodService(IMoodRepository moodRepository)
    {
        _moodRepository = moodRepository;
    }

    public async Task<List<MoodDto>> GetAllMoodsAsync()
    {
        var moods = await _moodRepository.GetAllAsync();
        return moods.Select(m => new MoodDto
        {
            MoodId = m.MoodId,
            Name = m.Name,
            Emoji = m.Emoji
        }).ToList();
    }
}
