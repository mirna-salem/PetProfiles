namespace PetProfiles.Maui.Services;

// EXAMPLE CONFIGURATION FILE
// Copy this file to ApiConfiguration.cs and update with your actual values
// The ApiConfiguration.cs file is excluded from Git for security

/*
public static class ApiConfiguration
{
    // Development settings - Update these values to match your backend
    public const string DevelopmentBaseUrl = "https://localhost:7027/api";
    public const string DevelopmentApiKey = "your-dev-api-key-here";
    
    // Production settings (update when you deploy to Azure)
    public const string ProductionBaseUrl = "https://your-api-url.azurewebsites.net/api";
    public const string ProductionApiKey = "your-azure-api-key";
    
    // Current environment - change this when deploying
    public static bool IsDevelopment => true;
    
    public static string BaseUrl => IsDevelopment ? DevelopmentBaseUrl : ProductionBaseUrl;
    public static string ApiKey => IsDevelopment ? DevelopmentApiKey : ProductionApiKey;
    
    // Helper method to validate configuration
    public static bool IsValidConfiguration()
    {
        return !string.IsNullOrEmpty(BaseUrl) && 
               !string.IsNullOrEmpty(ApiKey) && 
               !ApiKey.Contains("YOUR_") &&
               !BaseUrl.Contains("your-api-url");
    }
}
*/ 