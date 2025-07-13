using Microsoft.AspNetCore.Mvc;
using PetProfiles.Api.Models;

namespace PetProfiles.Api.Controllers;

/// <summary>
/// Base controller providing common functionality for URL generation and image handling.
/// </summary>
[ApiController]
public abstract class BaseController : ControllerBase
{
    protected readonly ILogger Logger;

    protected BaseController(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }



    /// <summary>
    /// Generates a complete image URL for the specified file name.
    /// Uses API proxy for simple, controlled access.
    /// </summary>
    /// <param name="fileName">The name of the image file.</param>
    /// <returns>The complete image URL.</returns>
    protected string GenerateImageUrl(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
        }

        var requestHost = GetRequestHost();
        var scheme = GetRequestScheme();
        var port = GetRequestPort();
        
        var baseUrl = $"{scheme}://{requestHost}";
        if (port.HasValue)
        {
            baseUrl += $":{port.Value}";
        }
        
        return $"{baseUrl}/api/Images/{fileName}";
    }

    /// <summary>
    /// Gets the host from the current HTTP request.
    /// </summary>
    /// <returns>The request host.</returns>
    protected string GetRequestHost() => HttpContext.Request.Host.Host;

    /// <summary>
    /// Gets the scheme from the current HTTP request.
    /// </summary>
    /// <returns>The request scheme.</returns>
    protected string GetRequestScheme() => HttpContext.Request.Scheme;

    /// <summary>
    /// Gets the port from the current HTTP request.
    /// </summary>
    /// <returns>The request port, or null if not specified.</returns>
    protected int? GetRequestPort() => HttpContext.Request.Host.Port;

    /// <summary>
    /// Generates a CDN URL for the specified file name (for production use).
    /// Provides even better performance and caching.
    /// </summary>
    /// <param name="fileName">The name of the image file.</param>
    /// <returns>The CDN URL.</returns>
    protected string GenerateCdnUrl(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
        }

        // Replace with your actual CDN endpoint when you set one up
        return $"https://your-cdn-endpoint.azureedge.net/pet-images/{fileName}";
    }
} 