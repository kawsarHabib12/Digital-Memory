using DigitalMemoryMap.BLL.Interfaces;

namespace DigitalMemoryMap.Web.Helpers;

public class FormFileWrapper : IPhotoFile
{
    private readonly IFormFile _file;

    public FormFileWrapper(IFormFile file)
    {
        _file = file;
    }

    public string FileName => _file.FileName;
    public long Length => _file.Length;
    public string ContentType => _file.ContentType;

    public Stream OpenReadStream()
    {
        return _file.OpenReadStream();
    }
}
