namespace DigitalMemoryMap.DAL.Entities;

public class Memory
{
    public int MemoryId { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime MemoryDate { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? LocationName { get; set; }
    public int CategoryId { get; set; }
    public byte? MoodId { get; set; }
    public byte Visibility { get; set; } = 0; // 0 Private, 1 Public
    public byte Status { get; set; } = 1;     // 1 Active, 2 RemovedByAdmin
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public Mood? Mood { get; set; }
    public ICollection<MemoryPhoto> Photos { get; set; } = new List<MemoryPhoto>();
    public ICollection<MemoryTag> MemoryTags { get; set; } = new List<MemoryTag>();
}
