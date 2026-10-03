namespace DigitalMemoryMap.DAL.Entities;

public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Memory> Memories { get; set; } = new List<Memory>();
}
