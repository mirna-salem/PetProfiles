namespace PetProfiles.Api.Models;

public class PetProfile
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Breed { get; set; } = string.Empty;
	public int Age { get; set; }
	public string? ImageUrl { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}