namespace PetProfiles.Maui.Services;

public interface IValidationService
{
    ValidationResult ValidateName(string name);
    ValidationResult ValidateBreed(string breed);
    ValidationResult ValidateAge(string age);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
} 