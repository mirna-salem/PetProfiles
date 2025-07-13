using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using PetProfiles.Maui.Models;

namespace PetProfiles.Maui.Services;

// In DEBUG mode, use a mock implementation of IPetProfilesService to avoid real API calls.

public class PetProfilesService : IPetProfilesService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _apiKey;

    public PetProfilesService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _baseUrl = ApiConfiguration.BaseUrl;
        _apiKey = ApiConfiguration.ApiKey;
        
        if (!ApiConfiguration.IsValidConfiguration())
        {
            throw new ArgumentException("Invalid API configuration");
        }
        
        if (!Uri.IsWellFormedUriString(_baseUrl, UriKind.Absolute))
        {
            throw new ArgumentException($"Invalid base URL format: {_baseUrl}");
        }
        
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private string BuildUrl(string endpoint) => $"{_baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";

    public async Task<List<PetProfile>> GetPetProfilesAsync()
    {
        try
        {
            var url = BuildUrl("PetProfiles");
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<PetProfile>>(content) ?? new List<PetProfile>();
        }
        catch
        {
            return new List<PetProfile>();
        }
    }

    public async Task<PetProfile?> GetPetProfileAsync(int id)
    {
        try
        {
            var url = BuildUrl($"PetProfiles/{id}");
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<PetProfile>(content);
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<PetProfile> CreatePetProfileAsync(PetProfile petProfile)
    {
        var url = BuildUrl("PetProfiles");
        var json = JsonConvert.SerializeObject(petProfile);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<PetProfile>(responseContent) ?? petProfile;
    }

    public async Task<bool> UpdatePetProfileAsync(PetProfile petProfile)
    {
        try
        {
            var url = BuildUrl($"PetProfiles/{petProfile.Id}");
            var json = JsonConvert.SerializeObject(petProfile);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync(url, content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeletePetProfileAsync(int id)
    {
        try
        {
            var url = BuildUrl($"PetProfiles/{id}");
            var response = await _httpClient.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<PetProfile> CreatePetProfileWithImageAsync(string name, string breed, int age, Stream? imageStream, string? imageFileName)
    {
        string? fileName = null;
        string? imageUrl = null;
        
        // First, upload the image if provided
        if (imageStream != null && !string.IsNullOrEmpty(imageFileName))
        {
            System.Diagnostics.Debug.WriteLine($"[CreatePetProfileWithImageAsync] Uploading image: {imageFileName}");
            (fileName, imageUrl) = await UploadImageAsync(imageStream, imageFileName, name);
            System.Diagnostics.Debug.WriteLine($"[CreatePetProfileWithImageAsync] Upload result - fileName: {fileName}, imageUrl: {imageUrl}");
        }
        
        // Then create the pet profile with the image filename as ImageUrl
        var petProfile = new PetProfile
        {
            Name = name,
            Breed = breed,
            Age = age,
            ImageUrl = fileName // Store only the filename
        };
        
        System.Diagnostics.Debug.WriteLine($"[CreatePetProfileWithImageAsync] Creating pet profile with ImageUrl: {fileName}");
        return await CreatePetProfileAsync(petProfile);
    }
    
    private async Task<(string? fileName, string? imageUrl)> UploadImageAsync(Stream imageStream, string imageFileName, string name)
    {
        try
        {
            var url = BuildUrl("Images");
            System.Diagnostics.Debug.WriteLine($"[UploadImageAsync] Uploading to URL: {url}");
            
            // Create a copy of the stream to avoid disposal issues
            byte[] imageBytes;
            using (var memoryStream = new MemoryStream())
            {
                // Reset stream position if possible
                if (imageStream.CanSeek)
                {
                    imageStream.Position = 0;
                }
                
                // Copy the stream content
                await imageStream.CopyToAsync(memoryStream);
                imageBytes = memoryStream.ToArray();
            }
            
            System.Diagnostics.Debug.WriteLine($"[UploadImageAsync] Image bytes length: {imageBytes.Length}");
            
            using var form = new MultipartFormDataContent();
            
            var imageContent = new ByteArrayContent(imageBytes);
            var contentType = GetContentTypeFromFileName(imageFileName);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            
            form.Add(imageContent, "file", imageFileName);
            form.Add(new StringContent(name), "name");
            
            System.Diagnostics.Debug.WriteLine($"[UploadImageAsync] Sending request...");
            var response = await _httpClient.PostAsync(url, form);
            
            System.Diagnostics.Debug.WriteLine($"[UploadImageAsync] Response status: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[UploadImageAsync] Error response: {errorContent}");
                return (null, null);
            }
            
            var responseContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"[UploadImageAsync] Success response: {responseContent}");
            
            var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
            
            return (result?.fileName?.ToString(), result?.imageUrl?.ToString());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UploadImageAsync] Exception: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[UploadImageAsync] Exception type: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"[UploadImageAsync] Inner exception: {ex.InnerException.Message}");
            }
            return (null, null);
        }
    }
    
    private static string GetContentTypeFromFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }

    public string GetImageUrlFromFileName(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return string.Empty;
        // Use API proxy for simple, controlled access
        return BuildUrl($"Images/{fileName}");
    }

    public async Task<PetProfile?> UpdatePetProfileImageAsync(int id, Stream imageStream, string imageFileName)
    {
        try
        {
            // First, get the current pet profile to check if it has an existing image
            var currentPet = await GetPetProfileAsync(id);
            if (currentPet == null)
            {
                return null;
            }
            
            // Upload the new image
            (var newFileName, var newImageUrl) = await UploadImageAsync(imageStream, imageFileName, currentPet.Name);
            if (newFileName == null)
            {
                return null;
            }
            
            // Delete the old image if it exists
            if (!string.IsNullOrEmpty(currentPet.ImageUrl))
            {
                await DeleteImageByFileNameAsync(currentPet.ImageUrl);
            }
            
            // Update the pet profile with the new image filename
            currentPet.ImageUrl = newFileName;
            var updateSuccess = await UpdatePetProfileAsync(currentPet);
            
            return updateSuccess ? currentPet : null;
        }
        catch
        {
            return null;
        }
    }
    
    private async Task<bool> DeleteImageByFileNameAsync(string fileName)
    {
        try
        {
            var url = BuildUrl($"Images/{fileName}");
            var response = await _httpClient.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

#if DEBUG
public class MockPetProfilesService : IPetProfilesService
{
    private static readonly List<PetProfile> _pets = new List<PetProfile>
    {
        new PetProfile { Id = 1, Name = "Fido", Breed = "Labrador", Age = 3, ImageUrl = null },
        new PetProfile { Id = 2, Name = "Whiskers", Breed = "Tabby", Age = 2, ImageUrl = null }
    };
    private static int _nextId = 3;

    public Task<List<PetProfile>> GetPetProfilesAsync() => Task.FromResult(_pets.Select(p => Clone(p)).ToList());

    public Task<PetProfile?> GetPetProfileAsync(int id)
    {
        var pet = _pets.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(pet == null ? null : Clone(pet));
    }

    public Task<PetProfile> CreatePetProfileAsync(PetProfile petProfile)
    {
        var newPet = Clone(petProfile);
        newPet.Id = _nextId++;
        _pets.Add(newPet);
        return Task.FromResult(Clone(newPet));
    }

    public Task<bool> UpdatePetProfileAsync(PetProfile petProfile)
    {
        var existing = _pets.FirstOrDefault(p => p.Id == petProfile.Id);
        if (existing == null) return Task.FromResult(false);
        existing.Name = petProfile.Name;
        existing.Breed = petProfile.Breed;
        existing.Age = petProfile.Age;
        existing.ImageUrl = petProfile.ImageUrl;
        return Task.FromResult(true);
    }

    public Task<bool> DeletePetProfileAsync(int id)
    {
        var pet = _pets.FirstOrDefault(p => p.Id == id);
        if (pet == null) return Task.FromResult(false);
        _pets.Remove(pet);
        return Task.FromResult(true);
    }

    public Task<PetProfile> CreatePetProfileWithImageAsync(string name, string breed, int age, Stream? imageStream, string? imageFileName)
    {
        var newPet = new PetProfile { Id = _nextId++, Name = name, Breed = breed, Age = age, ImageUrl = imageFileName };
        _pets.Add(newPet);
        return Task.FromResult(Clone(newPet));
    }

    public Task<PetProfile?> UpdatePetProfileImageAsync(int id, Stream imageStream, string imageFileName)
    {
        var pet = _pets.FirstOrDefault(p => p.Id == id);
        if (pet == null) return Task.FromResult<PetProfile?>(null);
        pet.ImageUrl = imageFileName;
        return Task.FromResult<PetProfile?>(Clone(pet));
    }

    // Helper to clone PetProfile to avoid exposing internal list
    private static PetProfile Clone(PetProfile p) => new PetProfile { Id = p.Id, Name = p.Name, Breed = p.Breed, Age = p.Age, ImageUrl = p.ImageUrl };
}
#endif 