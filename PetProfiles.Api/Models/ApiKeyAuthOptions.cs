namespace PetProfiles.Api.Models;

public class ApiKeyAuthOptions
{
	public const string SectionName = "ApiKeyAuth";
	public string ApiKey { get; set; } = string.Empty;
	public string HeaderName { get; set; } = "X-API-Key";
}