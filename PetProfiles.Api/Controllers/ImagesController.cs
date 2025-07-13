using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetProfiles.Api.Services;

namespace PetProfiles.Api.Controllers;

/// <summary>
/// Controller for handling image upload, download, and deletion operations by filename.
/// </summary>
[Route("api/[controller]")]
[Authorize]
public class ImagesController : BaseController
{
    private readonly IBlobStorageService _blobStorageService;
    
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
    private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

    public ImagesController(
        IBlobStorageService blobStorageService, 
        ILogger<ImagesController> logger)
        : base(logger)
    {
        _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
    }

    /// <summary>
    /// Uploads an image file. Returns the filename and imageUrl for use in the frontend.
    /// </summary>
    /// <param name="file">The image file to upload.</param>
    /// <param name="name">Optional name for logging purposes.</param>
    /// <returns>The uploaded image filename and imageUrl.</returns>
    [HttpPost]
    public async Task<ActionResult<object>> UploadImage(IFormFile file, [FromQuery] string? name = null)
    {
        var validationResult = ValidateUploadFile(file);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ErrorMessage);
        }

        try
        {
            var fileExtension = GetFileExtension(file.FileName);
            var fileName = $"img-{Guid.NewGuid()}{fileExtension}";
            
            await _blobStorageService.UploadImageAsync(file, fileName);
            
            var imageUrl = GenerateImageUrl(fileName);
            
            var logName = string.IsNullOrWhiteSpace(name) ? "Unknown" : name;
            Logger.LogInformation("Image uploaded successfully for {Name}: {FileName}", logName, fileName);
            
            return CreatedAtAction(nameof(GetImage), new { fileName }, new { fileName, imageUrl });
        }
        catch (Exception ex)
        {
            LogAndReturnError(ex, "Error uploading image");
            return StatusCode(500, "Error uploading image");
        }
    }

    /// <summary>
    /// Downloads an image file from blob storage by filename, or redirects to the blob URL if direct=true.
    /// </summary>
    /// <param name="fileName">The name of the file to download.</param>
    /// <param name="direct">If true, redirect to the blob URL instead of streaming.</param>
    /// <returns>The image file as a stream, or a redirect to the blob URL.</returns>
    [HttpGet("{fileName}")]
    public async Task<IActionResult> GetImage(string fileName, [FromQuery] bool direct = false)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return BadRequest("File name is required");
        }

        if (direct)
        {
            var blobUrl = _blobStorageService.GetBlobUrl(fileName);
            return Redirect(blobUrl);
        }

        try
        {
            var imageStream = await _blobStorageService.DownloadImageAsync(fileName);
            var contentType = GetContentType(fileName);
            
            return File(imageStream, contentType);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error downloading image: {FileName}", fileName);
            return NotFound();
        }
    }

    /// <summary>
    /// Deletes an image file from blob storage by filename.
    /// </summary>
    /// <param name="fileName">The name of the file to delete.</param>
    /// <returns>No content on successful deletion.</returns>
    [HttpDelete("{fileName}")]
    public async Task<IActionResult> DeleteImage(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return BadRequest("File name is required");
        }

        try
        {
            await _blobStorageService.DeleteImageAsync(fileName);
            Logger.LogInformation("Image deleted successfully: {FileName}", fileName);
            return NoContent();
        }
        catch (Exception ex)
        {
            LogAndReturnError(ex, "Error deleting image");
            return StatusCode(500, "Error deleting image");
        }
    }

    /// <summary>
    /// Validates the uploaded file for size and type restrictions.
    /// </summary>
    /// <param name="file">The file to validate.</param>
    /// <returns>Validation result with success status and error message if applicable.</returns>
    private (bool IsValid, string ErrorMessage) ValidateUploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return (false, "No file uploaded");
        }

        var fileExtension = GetFileExtension(file.FileName);
        if (!AllowedExtensions.Contains(fileExtension))
        {
            return (false, "Invalid file type. Only JPG, PNG, and GIF files are allowed.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return (false, "File size too large. Maximum size is 5MB.");
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// Gets the file extension from a filename.
    /// </summary>
    /// <param name="fileName">The filename to extract extension from.</param>
    /// <returns>The lowercase file extension.</returns>
    private static string GetFileExtension(string fileName) => 
        Path.GetExtension(fileName).ToLowerInvariant();

    /// <summary>
    /// Gets the content type based on file extension.
    /// </summary>
    /// <param name="fileName">The filename to determine content type for.</param>
    /// <returns>The appropriate content type.</returns>
    private static string GetContentType(string fileName)
    {
        var extension = GetFileExtension(fileName);
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };
    }

    /// <summary>
    /// Logs an error and returns a standardized error response.
    /// </summary>
    /// <param name="ex">The exception that occurred.</param>
    /// <param name="message">The error message to log.</param>
    private void LogAndReturnError(Exception ex, string message) => 
        Logger.LogError(ex, message);
} 