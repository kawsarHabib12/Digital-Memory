namespace DigitalMemoryMap.BLL.DTOs;

public class PhotoDto
{
    public int PhotoId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public int FileSizeKb { get; set; }
    public bool IsCover { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class UploadPhotoResultDto
{
    public List<PhotoDto> Photos { get; set; } = new();
}
