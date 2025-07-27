using Microsoft.Maui.Controls;
using PetProfiles.Maui.Models;

namespace PetProfiles.Maui.Services;

public interface IImagePreloadService
{
    Task PreloadImagesAsync(IEnumerable<PetProfile> profiles);
    Task<ImageSource> PreloadImageAsync(string imageUrl);
} 