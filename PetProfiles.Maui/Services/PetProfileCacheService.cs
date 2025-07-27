using PetProfiles.Maui.Models;
using Microsoft.Maui.Controls;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Text.Json;

namespace PetProfiles.Maui.Services;

public class PetProfileCacheService : IPetProfileCacheService
{
    private const int MaxImageSizeBytes = 1_000_000; // 1MB
    private const int MaxImageWidth = 800;
    private const int MaxImageHeight = 600;
    private const string CacheDirectoryName = "PetProfiles";
    private const string ProfileCacheSubdirectory = "ProfileCache";
    private const string ImageCacheSubdirectory = "ImageCache";
    private const string CacheMetadataFile = "cache_metadata.json";
    
    private readonly string _profileCacheDirectory;
    private readonly string _imageCacheDirectory;
    private readonly string _metadataFile;
    
    public PetProfileCacheService()
    {
        var baseCacheDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
            CacheDirectoryName);
            
        _profileCacheDirectory = Path.Combine(baseCacheDir, ProfileCacheSubdirectory);
        _imageCacheDirectory = Path.Combine(baseCacheDir, ImageCacheSubdirectory);
        _metadataFile = Path.Combine(_profileCacheDirectory, CacheMetadataFile);
        
        Directory.CreateDirectory(_profileCacheDirectory);
        Directory.CreateDirectory(_imageCacheDirectory);
    }
    
    public async Task CacheProfilesAsync(IEnumerable<PetProfile> profiles)
    {
        try
        {
            var cacheTasks = profiles.Select(async profile =>
            {
                try
                {
                    // Pre-load and cache the image if it exists
                    if (!string.IsNullOrEmpty(profile.ImageUrl))
                    {
                        var imageSource = await PreloadAndCacheImageAsync(profile.ImageUrl);
                        profile.PreloadedImage = imageSource;
                    }
                    
                    // Cache the profile data
                    await CacheProfileDataAsync(profile);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error caching profile {profile.Name}: {ex.Message}");
                }
            });
            
            await Task.WhenAll(cacheTasks);
            
            // Save cache metadata
            await SaveCacheMetadataAsync(profiles);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in CacheProfilesAsync: {ex.Message}");
        }
    }
    
    public async Task<IEnumerable<PetProfile>> GetCachedProfilesAsync()
    {
        try
        {
            if (!await HasCachedProfilesAsync())
            {
                return Enumerable.Empty<PetProfile>();
            }
            
            var metadata = await LoadCacheMetadataAsync();
            if (metadata == null || !metadata.ProfileIds.Any())
            {
                return Enumerable.Empty<PetProfile>();
            }
            
            var profiles = new List<PetProfile>();
            foreach (var profileId in metadata.ProfileIds)
            {
                var profile = await GetCachedProfileAsync(profileId);
                if (profile != null)
                {
                    profiles.Add(profile);
                }
            }
            
            return profiles;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetCachedProfilesAsync: {ex.Message}");
            return Enumerable.Empty<PetProfile>();
        }
    }
    
    public async Task<PetProfile?> GetCachedProfileAsync(int id)
    {
        try
        {
            var profileFile = Path.Combine(_profileCacheDirectory, $"profile_{id}.json");
            if (!File.Exists(profileFile))
            {
                return null;
            }
            
            var json = await File.ReadAllTextAsync(profileFile);
            var profile = JsonSerializer.Deserialize<PetProfile>(json);
            
            if (profile != null && !string.IsNullOrEmpty(profile.ImageUrl))
            {
                // Load the cached image
                var imageFile = Path.Combine(_imageCacheDirectory, profile.ImageUrl);
                if (File.Exists(imageFile))
                {
                    profile.PreloadedImage = ImageSource.FromFile(imageFile);
                }
            }
            
            return profile;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetCachedProfileAsync: {ex.Message}");
            return null;
        }
    }
    
    public async Task InvalidateCacheAsync()
    {
        try
        {
            if (Directory.Exists(_profileCacheDirectory))
            {
                Directory.Delete(_profileCacheDirectory, true);
                Directory.CreateDirectory(_profileCacheDirectory);
            }
            
            if (Directory.Exists(_imageCacheDirectory))
            {
                Directory.Delete(_imageCacheDirectory, true);
                Directory.CreateDirectory(_imageCacheDirectory);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in InvalidateCacheAsync: {ex.Message}");
        }
    }
    
    public async Task<bool> HasCachedProfilesAsync()
    {
        try
        {
            return File.Exists(_metadataFile);
        }
        catch
        {
            return false;
        }
    }
    
    private async Task<ImageSource> PreloadAndCacheImageAsync(string imageUrl)
    {
        try
        {
            var fullUrl = BuildFullUrl(imageUrl);
            var imageBytes = await DownloadImageAsync(fullUrl);
            var cacheFile = await CacheImageAsync(imageUrl, imageBytes);
            
            return ImageSource.FromFile(cacheFile);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error preloading image data for {imageUrl}: {ex.Message}");
            return ImageSource.FromFile("paw_icon.png");
        }
    }
    
    private async Task CacheProfileDataAsync(PetProfile profile)
    {
        try
        {
            var profileFile = Path.Combine(_profileCacheDirectory, $"profile_{profile.Id}.json");
            var json = JsonSerializer.Serialize(profile, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            await File.WriteAllTextAsync(profileFile, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error caching profile data: {ex.Message}");
        }
    }
    
    private async Task SaveCacheMetadataAsync(IEnumerable<PetProfile> profiles)
    {
        try
        {
            var metadata = new CacheMetadata
            {
                CachedAt = DateTime.UtcNow,
                ProfileIds = profiles.Select(p => p.Id).ToList()
            };
            
            var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            await File.WriteAllTextAsync(_metadataFile, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving cache metadata: {ex.Message}");
        }
    }
    
    private async Task<CacheMetadata?> LoadCacheMetadataAsync()
    {
        try
        {
            if (!File.Exists(_metadataFile))
            {
                return null;
            }
            
            var json = await File.ReadAllTextAsync(_metadataFile);
            return JsonSerializer.Deserialize<CacheMetadata>(json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading cache metadata: {ex.Message}");
            return null;
        }
    }
    
    private string BuildFullUrl(string imageUrl)
    {
        if (Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
        {
            return imageUrl;
        }
        
        return $"{ApiConfiguration.BaseUrl}/Images/{imageUrl}";
    }
    
    private async Task<byte[]> DownloadImageAsync(string fullUrl)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("X-API-Key", ApiConfiguration.ApiKey);
        
        var response = await httpClient.GetAsync(fullUrl);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to download image: {response.StatusCode} - {response.ReasonPhrase}");
        }
        
        return await response.Content.ReadAsByteArrayAsync();
    }
    
    private async Task<string> CacheImageAsync(string imageUrl, byte[] imageBytes)
    {
        var cacheFile = Path.Combine(_imageCacheDirectory, imageUrl);
        
        if (imageBytes.Length > MaxImageSizeBytes)
        {
            try
            {
                var resizedBytes = await ResizeImageAsync(imageBytes, MaxImageWidth, MaxImageHeight);
                await File.WriteAllBytesAsync(cacheFile, resizedBytes);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error resizing image, using original: {ex.Message}");
                await File.WriteAllBytesAsync(cacheFile, imageBytes);
            }
        }
        else
        {
            await File.WriteAllBytesAsync(cacheFile, imageBytes);
        }
        
        return cacheFile;
    }
    
    private async Task<byte[]> ResizeImageAsync(byte[] imageBytes, int maxWidth, int maxHeight)
    {
        try
        {
            using var originalStream = new MemoryStream(imageBytes);
            using var image = await SixLabors.ImageSharp.Image.LoadAsync(originalStream);
            
            // Calculate new dimensions while maintaining aspect ratio
            var ratio = Math.Min((double)maxWidth / image.Width, (double)maxHeight / image.Height);
            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);
            
            // Resize the image
            image.Mutate(x => x.Resize(newWidth, newHeight));
            
            // Save to memory stream
            using var outputStream = new MemoryStream();
            await image.SaveAsync(outputStream, image.Metadata.DecodedImageFormat);
            
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in ResizeImageAsync: {ex.Message}");
            return imageBytes; // Return original if resizing fails
        }
    }
    
    private class CacheMetadata
    {
        public DateTime CachedAt { get; set; }
        public List<int> ProfileIds { get; set; } = new();
    }
} 