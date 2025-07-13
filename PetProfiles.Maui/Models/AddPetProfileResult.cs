namespace PetProfiles.Maui.Models;

public class AddPetProfileResult
{
    public int? PetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? ImageUrl { get; set; }
    public Stream? ImageStream { get; set; }
    public string? ImageFileName { get; set; }
} 