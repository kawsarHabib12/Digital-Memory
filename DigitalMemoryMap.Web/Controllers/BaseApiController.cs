using System.Security.Claims;
using DigitalMemoryMap.BLL.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace DigitalMemoryMap.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected int CurrentUserId
    {
        get
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(claim, out var id))
            {
                return id;
            }
            throw new UnauthorizedException("Please log in.");
        }
    }

    protected bool IsCurrentUserAdmin => User.IsInRole("Admin");
}
