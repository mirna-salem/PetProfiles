using System.Globalization;
using System.Collections.Concurrent;
using PetProfiles.Maui.Services;

namespace PetProfiles.Maui.Converters;

public class ImageSourceConverter : IValueConverter
{
    private static readonly ConcurrentDictionary<string, CachedImage> _imageCache = new ConcurrentDictionary<string, CachedImage>();
    
            // Static instance for sharing across the app
        public static readonly ImageSourceConverter Instance = new ImageSourceConverter();
        

    
    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("X-API-Key", ApiConfiguration.ApiKey);
        client.Timeout = TimeSpan.FromSeconds(10);
        return client;
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string imageUrl || string.IsNullOrWhiteSpace(imageUrl))
        {
            return ImageSource.FromFile("paw_icon.png");
        }

        // If the value is not an absolute URL, treat it as a filename and build the full URL
        if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
        {
            var service = new PetProfilesService(CreateHttpClient());
            imageUrl = service.GetImageUrlFromFileName(imageUrl);
        }

        // Check cache first
        if (_imageCache.TryGetValue(imageUrl, out var cachedImage))
        {
            if (!cachedImage.IsExpired)
            {
                return cachedImage.ImageSource;
            }
            else
            {
                // Remove expired entry
                _imageCache.TryRemove(imageUrl, out _);
            }
        }



        try
        {
            // Handle API image endpoints with authentication
            if (imageUrl.Contains("/api/Images/"))
            {
                var streamImage = ImageSource.FromStream(() =>
                {
                    try
                    {
                        using var httpClient = CreateHttpClient();
                        var response = httpClient.GetAsync(imageUrl).ConfigureAwait(false).GetAwaiter().GetResult();
                        
                        if (response.IsSuccessStatusCode)
                        {
                            var stream = response.Content.ReadAsStreamAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                            return stream;
                        }
                        return null;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                });
                
                // Cache for 1 hour to make it really fast
                var apiCachedImage = new CachedImage(streamImage, TimeSpan.FromHours(1));
                _imageCache[imageUrl] = apiCachedImage;
                return streamImage;
            }

            // Handle other URLs (fallback)
            var fallbackImage = ImageSource.FromUri(new Uri(imageUrl));
            var fallbackCachedImage = new CachedImage(fallbackImage, TimeSpan.FromHours(1));
            _imageCache[imageUrl] = fallbackCachedImage;
            return fallbackImage;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ImageSourceConverter] Error creating ImageSource: {ex.Message}");
            return ImageSource.FromFile("paw_icon.png");
        }
    }

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}







    private class CachedImage
    {
        public ImageSource ImageSource { get; }
        public DateTime ExpirationTime { get; }
        public bool IsExpired => DateTime.UtcNow > ExpirationTime;

        public CachedImage(ImageSource imageSource, TimeSpan cacheDuration)
        {
            ImageSource = imageSource;
            ExpirationTime = DateTime.UtcNow.Add(cacheDuration);
        }
    }
}

public class InverseBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return value;
    }
} 