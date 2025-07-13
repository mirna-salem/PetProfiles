namespace PetProfiles.Maui.Services;

public class ValidationService : IValidationService
{
    public ValidationResult ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new ValidationResult { IsValid = true, ErrorMessage = string.Empty };
        }

        if (name.Any(char.IsDigit))
        {
            return new ValidationResult { IsValid = false, ErrorMessage = "Name cannot contain numbers" };
        }

        return new ValidationResult { IsValid = true, ErrorMessage = string.Empty };
    }

    public ValidationResult ValidateBreed(string breed)
    {
        if (string.IsNullOrWhiteSpace(breed))
        {
            return new ValidationResult { IsValid = true, ErrorMessage = string.Empty };
        }

        if (breed.Any(char.IsDigit))
        {
            return new ValidationResult { IsValid = false, ErrorMessage = "Breed cannot contain numbers" };
        }

        return new ValidationResult { IsValid = true, ErrorMessage = string.Empty };
    }

    public ValidationResult ValidateAge(string age)
    {
        if (string.IsNullOrWhiteSpace(age))
        {
            return new ValidationResult { IsValid = true, ErrorMessage = string.Empty };
        }

        if (age.Any(char.IsLetter))
        {
            return new ValidationResult { IsValid = false, ErrorMessage = "Age can only contain numbers" };
        }

        return new ValidationResult { IsValid = true, ErrorMessage = string.Empty };
    }
} 