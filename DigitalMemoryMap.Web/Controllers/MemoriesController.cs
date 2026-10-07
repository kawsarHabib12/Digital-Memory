using DigitalMemoryMap.BLL.DTOs;
using DigitalMemoryMap.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalMemoryMap.Web.Controllers;

[Authorize]
[Route("api/[controller]")]
public class MemoriesController : BaseApiController
{
    private readonly IMemoryService _memoryService;

    public MemoriesController(IMemoryService memoryService)
    {
        _memoryService = memoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMemories([FromQuery] MemoryFilterRequestDto filter)
    {
        if (!User.IsInRole("Admin"))
        {
            filter.UserId = CurrentUserId;
        }
        var result = await _memoryService.GetPagedMemoriesAsync(filter);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchMemories([FromQuery] MemoryFilterRequestDto filter)
    {
        if (!User.IsInRole("Admin"))
        {
            filter.UserId = CurrentUserId;
        }
        var result = await _memoryService.GetPagedMemoriesAsync(filter);
        return Ok(result);
    }

    [HttpGet("map")]
    public async Task<IActionResult> GetMapPins(
        [FromQuery] int? categoryId,
        [FromQuery] byte? moodId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var targetUserId = User.IsInRole("Admin") ? (int?)null : CurrentUserId;
        var pins = await _memoryService.GetMapPinsAsync(targetUserId, categoryId, moodId, from, to);
        return Ok(pins);
    }

    [HttpGet("my-journey")]
    public async Task<IActionResult> GetMyJourney()
    {
        var journey = await _memoryService.GetJourneyPointsAsync(CurrentUserId);
        return Ok(journey);
    }

    [HttpGet("nearby")]
    public async Task<IActionResult> GetNearby(
        [FromQuery] decimal lat,
        [FromQuery] decimal lng,
        [FromQuery] double radiusKm = 10.0)
    {
        var memories = await _memoryService.GetNearbyMemoriesAsync(CurrentUserId, lat, lng, radiusKm);
        return Ok(memories);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetDetails(int id)
    {
        var targetUserId = User.IsInRole("Admin") ? (int?)null : CurrentUserId;
        var memory = await _memoryService.GetMemoryDetailsAsync(id, targetUserId);
        return Ok(memory);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMemoryDto dto)
    {
        var memory = await _memoryService.CreateMemoryAsync(CurrentUserId, dto);
        return CreatedAtAction(nameof(GetDetails), new { id = memory.MemoryId }, memory);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMemoryDto dto)
    {
        var memory = await _memoryService.UpdateMemoryAsync(id, CurrentUserId, dto);
        return Ok(memory);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _memoryService.DeleteMemoryAsync(id, CurrentUserId);
        return NoContent();
    }
}
