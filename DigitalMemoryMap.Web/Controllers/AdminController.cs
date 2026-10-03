using DigitalMemoryMap.BLL.DTOs;
using DigitalMemoryMap.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalMemoryMap.Web.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class AdminController : BaseApiController
{
    private readonly IAdminService _adminService;
    private readonly ICategoryService _categoryService;

    public AdminController(IAdminService adminService, ICategoryService categoryService)
    {
        _adminService = adminService;
        _categoryService = categoryService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _adminService.GetStatsAsync();
        return Ok(stats);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _adminService.GetUsersAsync(search, page, pageSize);
        return Ok(result);
    }

    [HttpPut("users/{id:int}/status")]
    public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusDto dto)
    {
        await _adminService.UpdateUserStatusAsync(id, dto.IsActive);
        return Ok(new { message = $"User status updated to {(dto.IsActive ? "Active" : "Deactivated")}." });
    }

    [HttpGet("memories")]
    public async Task<IActionResult> GetMemories(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null)
    {
        var result = await _adminService.GetAllMemoriesAsync(page, pageSize, keyword);
        return Ok(result);
    }

    [HttpDelete("memories/{id:int}")]
    public async Task<IActionResult> RemoveMemory(int id)
    {
        await _adminService.RemoveMemoryAsync(id);
        return Ok(new { message = "Memory has been removed by admin." });
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var category = await _categoryService.CreateAsync(dto);
        return StatusCode(StatusCodes.Status201Created, category);
    }

    [HttpPut("categories/{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
    {
        var category = await _categoryService.UpdateAsync(id, dto);
        return Ok(category);
    }

    [HttpDelete("categories/{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        await _categoryService.DeleteAsync(id);
        return NoContent();
    }
}
