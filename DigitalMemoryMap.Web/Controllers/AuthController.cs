using DigitalMemoryMap.BLL.DTOs;
using DigitalMemoryMap.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalMemoryMap.Web.Controllers;

[Route("api/[controller]")]
public class AuthController : BaseApiController
{
    private readonly IUserService _userService;
    public const string CookieName = "dmm_token";

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var response = await _userService.RegisterAsync(dto);
        SetAuthCookie(response.Token, response.ExpiresAt);
        return StatusCode(StatusCodes.Status201Created, new
        {
            userId = response.UserId,
            fullName = response.FullName,
            email = response.Email,
            role = response.Role
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var response = await _userService.LoginAsync(dto);
        SetAuthCookie(response.Token, response.ExpiresAt);
        return Ok(new
        {
            userId = response.UserId,
            fullName = response.FullName,
            role = response.Role,
            expiresAt = response.ExpiresAt
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(CookieName, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = Request.IsHttps,
            Path = "/"
        });

        return Ok(new { message = "Logged out successfully." });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto dto)
    {
        await _userService.ChangePasswordAsync(CurrentUserId, dto);
        return Ok(new { message = "Password changed successfully." });
    }

    private void SetAuthCookie(string token, DateTime expiresAt)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = Request.IsHttps,
            Expires = expiresAt,
            Path = "/"
        };

        Response.Cookies.Append(CookieName, token, cookieOptions);
    }
}
