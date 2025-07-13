using PetProfiles.Maui.Models;

namespace PetProfiles.Maui.Services;

// In DEBUG mode, a mock implementation of this interface can be used to avoid real API calls.
public interface IPetProfilesService
{
    Task<List<PetProfile>> GetPetProfilesAsync();
    Task<PetProfile?> GetPetProfileAsync(int id);
    Task<PetProfile> CreatePetProfileAsync(PetProfile petProfile);
    Task<bool> UpdatePetProfileAsync(PetProfile petProfile);
    Task<bool> DeletePetProfileAsync(int id);
    Task<PetProfile> CreatePetProfileWithImageAsync(string name, string breed, int age, Stream? imageStream, string? imageFileName);
    Task<PetProfile?> UpdatePetProfileImageAsync(int id, Stream imageStream, string imageFileName);
} 