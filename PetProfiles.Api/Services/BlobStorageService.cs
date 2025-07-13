using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace PetProfiles.Api.Services;

public interface IBlobStorageService
{
    Task UploadImageAsync(IFormFile file, string fileName);
    Task<Stream> DownloadImageAsync(string fileName);
    Task DeleteImageAsync(string fileName);
    string GetBlobUrl(string fileName);
    string GetSignedUrl(string fileName, TimeSpan? expiration = null);
}

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly ILogger<BlobStorageService> _logger;
    private readonly string _blobBaseUrl;

    public BlobStorageService(IConfiguration configuration, ILogger<BlobStorageService> logger)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"];
        _containerName = configuration["AzureStorage:ContainerName"] ?? "pet-images";
        _blobServiceClient = new BlobServiceClient(connectionString);
        _logger = logger;
        _blobBaseUrl = configuration["AzureStorage:BlobBaseUrl"] ?? string.Empty;
    }

    public async Task UploadImageAsync(IFormFile file, string fileName)
    {
        try
        {
            _logger.LogInformation("Uploading image: {FileName}", fileName);

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);

            _logger.LogInformation("Successfully uploaded image: {FileName}", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image: {FileName}", fileName);
            throw;
        }
    }

    public async Task<Stream> DownloadImageAsync(string fileName)
    {
        try
        {
            _logger.LogInformation("Downloading image: {FileName}", fileName);

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            var response = await blobClient.DownloadAsync();
            return response.Value.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading image: {FileName}", fileName);
            throw;
        }
    }

    public async Task DeleteImageAsync(string fileName)
    {
        try
        {
            _logger.LogInformation("Deleting image: {FileName}", fileName);

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.DeleteIfExistsAsync();

            _logger.LogInformation("Successfully deleted image: {FileName}", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image: {FileName}", fileName);
            throw;
        }
    }

    public string GetBlobUrl(string fileName)
    {
        if (string.IsNullOrWhiteSpace(_blobBaseUrl))
        {
            // Fallback to constructing the URL from the blob client
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            return blobClient.Uri.ToString();
        }
        return $"{_blobBaseUrl.TrimEnd('/')}/{fileName}";
    }

    public string GetSignedUrl(string fileName, TimeSpan? expiration = null)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            // Create a SAS token that expires in 1 hour by default
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerName,
                BlobName = fileName,
                Resource = "b", // blob
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.Add(expiration ?? TimeSpan.FromHours(1))
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasToken = sasBuilder.ToSasQueryParameters(
                new Azure.Storage.StorageSharedKeyCredential(
                    _blobServiceClient.AccountName, 
                    GetAccountKeyFromConnectionString()
                )
            ).ToString();

            return $"{blobClient.Uri}?{sasToken}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating signed URL for: {FileName}", fileName);
            // Fallback to public URL if signed URL generation fails
            return GetBlobUrl(fileName);
        }
    }

    private string GetAccountKeyFromConnectionString()
    {
        // Extract account key from connection string
        var connectionString = _blobServiceClient.Uri.ToString();
        // This is a simplified approach - you might want to store the key separately
        // For now, we'll use the connection string approach
        return "YOUR_ACCOUNT_KEY"; // Replace with actual key extraction logic
    }
} 