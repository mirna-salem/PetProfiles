using Microsoft.Maui.Controls;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using PetProfiles.Maui.Models;

namespace PetProfiles.Maui.Services;

public class ImagePreloadService : IImagePreloadService
{
    private const int MaxImageSizeBytes = 1_000_000; // 1MB
    private const int MaxImageWidth = 800;
    private const int MaxImageHeight = 600;
    private const string CacheDirectoryName = "PetProfiles";
    private const string ImageCacheSubdirectory = "ImageCache";
    
    private readonly string _cacheDirectory;
    
    public ImagePreloadService()
    {
        _cacheDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
            CacheDirectoryName, 
            ImageCacheSubdirectory);
        Directory.CreateDirectory(_cacheDirectory);
    }
    
    public async Task PreloadImagesAsync(IEnumerable<PetProfile> profiles)
    {
        try
        {
            var tasks = profiles
                .Where(p => !string.IsNullOrEmpty(p.ImageUrl))
                .Select(async profile =>
                {
                    try
                    {
                        var imageSource = await PreloadImageAsync(profile.ImageUrl);
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            profile.PreloadedImage = imageSource;
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error preloading image for {profile.Name}: {ex.Message}");
                    }
                });
            
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in preload images: {ex.Message}");
        }
    }
    
    public async Task<ImageSource> PreloadImageAsync(string imageUrl)
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
        var cacheFile = Path.Combine(_cacheDirectory, imageUrl);
        
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
} 