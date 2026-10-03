using DigitalMemoryMap.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalMemoryMap.Web.Controllers;

[Authorize]
[Route("api/[controller]")]
public class CategoriesController : BaseApiController
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _categoryService.GetAllActiveAsync();
        return Ok(categories);
    }
}

[Authorize]
[Route("api/[controller]")]
public class MoodsController : BaseApiController
{
    private readonly IMoodService _moodService;

    public MoodsController(IMoodService moodService)
    {
        _moodService = moodService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMoods()
    {
        var moods = await _moodService.GetAllMoodsAsync();
        return Ok(moods);
    }
}

[Authorize]
[Route("api/[controller]")]
public class TagsController : BaseApiController
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTags()
    {
        var tags = await _tagService.GetTagsForUserAsync(CurrentUserId);
        return Ok(tags);
    }
}
