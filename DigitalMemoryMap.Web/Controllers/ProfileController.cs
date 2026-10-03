using DigitalMemoryMap.BLL.DTOs;
using DigitalMemoryMap.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalMemoryMap.Web.Controllers;

[Authorize]
[Route("api/[controller]")]
public class ProfileController : BaseApiController
{
    private readonly IUserService _userService;

    public ProfileController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var profile = await _userService.GetProfileAsync(CurrentUserId);
        return Ok(profile);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var profile = await _userService.UpdateProfileAsync(CurrentUserId, dto);
        return Ok(profile);
    }
}
