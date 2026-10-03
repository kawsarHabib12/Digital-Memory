namespace DigitalMemoryMap.DAL.Entities;

public class MemoryPhoto
{
    public int PhotoId { get; set; }
    public int MemoryId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public int FileSizeKb { get; set; }
    public bool IsCover { get; set; } = false;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public Memory Memory { get; set; } = null!;
}
