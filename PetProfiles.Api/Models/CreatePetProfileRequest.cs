namespace PetProfiles.Api.Models;

public class CreatePetProfileRequest
{
	public string Name { get; set; } = string.Empty;
	public string Breed { get; set; } = string.Empty;
	public int Age { get; set; }
	public IFormFile? ImageFile { get; set; }
}