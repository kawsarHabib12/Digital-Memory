namespace DigitalMemoryMap.DAL.Entities;

public class Tag
{
    public int TagId { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;

    public User User { get; set; } = null!;
    public ICollection<MemoryTag> MemoryTags { get; set; } = new List<MemoryTag>();
}
