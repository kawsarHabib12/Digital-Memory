namespace DigitalMemoryMap.DAL.Entities;

public class Mood
{
    public byte MoodId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;

    public ICollection<Memory> Memories { get; set; } = new List<Memory>();
}
