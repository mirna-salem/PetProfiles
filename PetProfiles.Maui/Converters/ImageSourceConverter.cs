using System.Globalization;
using System.Collections.Concurrent;
using PetProfiles.Maui.Services;

namespace PetProfiles.Maui.Converters;

public class ImageSourceConverter : IValueConverter
{
    private static readonly ConcurrentDictionary<string, ImageSource> _imageCache = new ConcurrentDictionary<string, ImageSource>();
    
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
            return cachedImage;
        }

        // Debug print for troubleshooting
        System.Diagnostics.Debug.WriteLine($"[ImageSourceConverter] Loading image URL: {imageUrl}");

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
                        
                        // Use ConfigureAwait(false) to avoid deadlocks
                        var response = httpClient.GetAsync(imageUrl).ConfigureAwait(false).GetAwaiter().GetResult();
                        
                        System.Diagnostics.Debug.WriteLine($"[ImageSourceConverter] HTTP Status: {response.StatusCode} for URL: {imageUrl}");
                        
                        if (response.IsSuccessStatusCode)
                        {
                            var stream = response.Content.ReadAsStreamAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                            System.Diagnostics.Debug.WriteLine($"[ImageSourceConverter] Successfully loaded stream, length: {stream?.Length ?? 0}");
                            return stream;
                        }
                        
                        var errorContent = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                        System.Diagnostics.Debug.WriteLine($"[ImageSourceConverter] Error response: {errorContent}");
                        return null;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ImageSourceConverter] Error loading image: {ex.Message}");
                        return null;
                    }
                });
                _imageCache[imageUrl] = streamImage;
                return streamImage;
            }

            // Handle other URLs (fallback)
            var fallbackImage = ImageSource.FromUri(new Uri(imageUrl));
            _imageCache[imageUrl] = fallbackImage;
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