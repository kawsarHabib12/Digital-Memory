using System.ComponentModel.DataAnnotations;

namespace DigitalMemoryMap.BLL.DTOs;

public class CategoryDto
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreateCategoryDto
{
    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 50 characters.")]
    public string Name { get; set; } = string.Empty;
}

public class UpdateCategoryDto
{
    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 50 characters.")]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public class MoodDto
{
    public byte MoodId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
}

public class TagDto
{
    public int TagId { get; set; }
    public string Name { get; set; } = string.Empty;
}
