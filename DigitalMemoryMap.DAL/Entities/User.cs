namespace DigitalMemoryMap.DAL.Entities;

public class User
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public byte RoleId { get; set; } = 1; // Default User
    public string? Bio { get; set; }
    public string? ProfilePhotoPath { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Role Role { get; set; } = null!;
    public ICollection<Memory> Memories { get; set; } = new List<Memory>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
