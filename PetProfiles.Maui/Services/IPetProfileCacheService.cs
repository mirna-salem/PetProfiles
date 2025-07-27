using PetProfiles.Maui.Models;

namespace PetProfiles.Maui.Services;

public interface IPetProfileCacheService
{
    Task CacheProfilesAsync(IEnumerable<PetProfile> profiles);
    Task<IEnumerable<PetProfile>> GetCachedProfilesAsync();
    Task<PetProfile?> GetCachedProfileAsync(int id);
    Task InvalidateCacheAsync();
    Task<bool> HasCachedProfilesAsync();
} 