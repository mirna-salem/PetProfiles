namespace PetProfiles.Api.Services;

public static class CacheKeys
{
    public const string AllPetProfiles = "all_pet_profiles";
    
    public static string PetProfile(int id) => $"pet_profile_{id}";
    
    public static string PetProfilePattern => "pet_profile_*";
} 