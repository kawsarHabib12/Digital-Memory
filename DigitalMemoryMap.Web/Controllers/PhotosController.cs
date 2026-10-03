using DigitalMemoryMap.BLL.Interfaces;
using DigitalMemoryMap.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalMemoryMap.Web.Controllers;

[Authorize]
[ApiController]
public class PhotosController : BaseApiController
{
    private readonly IPhotoService _photoService;
    private readonly IWebHostEnvironment _environment;

    public PhotosController(IPhotoService photoService, IWebHostEnvironment environment)
    {
        _photoService = photoService;
        _environment = environment;
    }

    private string BaseUploadPath => Path.Combine(_environment.ContentRootPath, "..", "uploads");

    [HttpPost("/api/memories/{memoryId:int}/photos")]
    public async Task<IActionResult> UploadPhotos(int memoryId, [FromForm] List<IFormFile> files)
    {
        var wrappedFiles = files.Select(f => (IPhotoFile)new FormFileWrapper(f));
        var result = await _photoService.UploadPhotosAsync(memoryId, CurrentUserId, wrappedFiles, BaseUploadPath);
        return StatusCode(StatusCodes.Status201Created, new { photos = result });
    }

    [HttpDelete("{photoId:int}")]
    public async Task<IActionResult> DeletePhoto(int photoId)
    {
        await _photoService.DeletePhotoAsync(photoId, CurrentUserId, BaseUploadPath);
        return NoContent();
    }

    [HttpPut("{photoId:int}/cover")]
    public async Task<IActionResult> SetCover(int photoId)
    {
        await _photoService.SetCoverPhotoAsync(photoId, CurrentUserId);
        return Ok(new { message = "Cover photo updated successfully." });
    }

    [HttpGet("{photoId:int}/file")]
    public async Task<IActionResult> GetPhotoFile(int photoId)
    {
        var (filePath, contentType, originalFileName) = await _photoService.GetPhotoFileAsync(
            photoId,
            CurrentUserId,
            IsCurrentUserAdmin,
            BaseUploadPath);

        return PhysicalFile(filePath, contentType, enableRangeProcessing: true);
    }
}
