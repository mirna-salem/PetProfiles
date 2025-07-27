using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using PetProfiles.Maui.Models;
using PetProfiles.Maui.Services;

namespace PetProfiles.Maui.ViewModels;

public class AddPetProfilePopupViewModel : BaseViewModel
{
    private readonly IValidationService _validationService;

    // Private fields
    private string _name = string.Empty;
    private string _breed = string.Empty;
    private string _age = string.Empty;
    private string? _imageUrl;
    private ImageSource? _selectedImage;
    private Stream? _selectedImageStream;
    private string? _selectedImageFileName;
    private int? _petId;
    private string _nameError = string.Empty;
    private string _breedError = string.Empty;
    private string _ageError = string.Empty;
    private bool _hasErrors;

    // Events
    public event Action<AddPetProfileResult>? SaveCompleted;
    public event Action? CancelRequested;

    // Public properties
    public bool IsEditMode { get; }
    public int? PetId => _petId;
    public bool HasSelectedImage => SelectedImage != null;
    
    public bool HasErrors 
    { 
        get => _hasErrors;
        set => SetProperty(ref _hasErrors, value);
    }

    // Input properties with validation
    public string Name
    {
        get => _name;
        set 
        { 
            SetProperty(ref _name, value);
            ValidateName();
        }
    }

    public string Breed
    {
        get => _breed;
        set 
        { 
            SetProperty(ref _breed, value);
            ValidateBreed();
        }
    }

    public string Age
    {
        get => _age;
        set 
        { 
            SetProperty(ref _age, value);
            ValidateAge();
        }
    }

    // Error properties
    public string NameError
    {
        get => _nameError;
        set => SetProperty(ref _nameError, value);
    }

    public string BreedError
    {
        get => _breedError;
        set => SetProperty(ref _breedError, value);
    }

    public string AgeError
    {
        get => _ageError;
        set => SetProperty(ref _ageError, value);
    }

    // Image properties
    public string? ImageUrl
    {
        get => _imageUrl;
        set => SetProperty(ref _imageUrl, value);
    }

    public ImageSource? SelectedImage
    {
        get => _selectedImage;
        set
        {
            SetProperty(ref _selectedImage, value);
            OnPropertyChanged(nameof(HasSelectedImage));
        }
    }

    public Stream? SelectedImageStream
    {
        get => _selectedImageStream;
        set => SetProperty(ref _selectedImageStream, value);
    }

    public string? SelectedImageFileName
    {
        get => _selectedImageFileName;
        set => SetProperty(ref _selectedImageFileName, value);
    }

    // Commands
    public ICommand SaveCommand => new Command(ExecuteSave);
    public ICommand CancelCommand => new Command(ExecuteCancel);
    public ICommand UploadImageCommand => new Command(async () => await ExecuteUploadImage());
    public ICommand DeleteImageCommand => new Command(DeleteImage);

    // Constructors
    public AddPetProfilePopupViewModel(IValidationService validationService)
    {
        _validationService = validationService;
    }

    public AddPetProfilePopupViewModel(IValidationService validationService, PetProfile pet)
    {
        _validationService = validationService;
        
        if (pet != null)
        {
            _petId = pet.Id;
            Name = pet.Name;
            Breed = pet.Breed;
            Age = pet.Age.ToString();
            ImageUrl = pet.ImageUrl;
            
            if (!string.IsNullOrEmpty(pet.ImageUrl))
            {
                // Use pre-loaded image if available, otherwise load from cache
                if (pet.PreloadedImage != null)
                {
                    SelectedImage = pet.PreloadedImage;
                }
                else
                {
                    // Fallback to loading from cache if not pre-loaded
                    var converter = Converters.ImageSourceConverter.Instance;
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var imageSource = (ImageSource)converter.Convert(pet.ImageUrl, typeof(ImageSource), null, System.Globalization.CultureInfo.CurrentCulture);
                            await MainThread.InvokeOnMainThreadAsync(() =>
                            {
                                SelectedImage = imageSource;
                            });
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error loading image: {ex.Message}");
                        }
                    });
                }
            }
            IsEditMode = true;
        }
    }

    // Public methods
    public void DeleteImage()
    {
        SelectedImage = null;
        SelectedImageStream = null;
        SelectedImageFileName = null;
        ImageUrl = null;
    }

    // Private command execution methods
    private async Task ExecuteUploadImage()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select an image",
                FileTypes = FilePickerFileType.Images
            });
            
            if (result != null)
            {
                await HandleImageSelection(result);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error picking file: {ex.Message}");
        }
    }

    private void ExecuteSave()
    {
        var result = ValidateAndCreateResult();
        if (result != null)
        {
            SaveCompleted?.Invoke(result);
        }
    }

    private void ExecuteCancel()
    {
        Dispose();
        CancelRequested?.Invoke();
    }

    // Private helper methods
    private async Task HandleImageSelection(FileResult result)
    {
        try
        {
            using var stream = await result.OpenReadAsync();
            var bytes = new byte[stream.Length];
            await stream.ReadAsync(bytes, 0, (int)stream.Length);
            
            SelectedImage = ImageSource.FromStream(() => new MemoryStream(bytes));
            SelectedImageStream = new MemoryStream(bytes);
            SelectedImageFileName = result.FileName;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error handling image selection: {ex.Message}");
        }
    }

    private AddPetProfileResult? ValidateAndCreateResult()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Breed) || string.IsNullOrWhiteSpace(Age))
        {
            return null;
        }

        if (!int.TryParse(Age, out int age))
        {
            return null;
        }

        return new AddPetProfileResult
        {
            Name = Name.Trim(),
            Breed = Breed.Trim(),
            Age = age,
            ImageUrl = string.IsNullOrWhiteSpace(ImageUrl) ? null : ImageUrl.Trim(),
            ImageStream = SelectedImageStream,
            ImageFileName = SelectedImageFileName,
            PetId = PetId
        };
    }

    // Validation methods
    private void ValidateName()
    {
        var result = _validationService.ValidateName(_name);
        NameError = result.ErrorMessage;
        UpdateHasErrors();
    }

    private void ValidateBreed()
    {
        var result = _validationService.ValidateBreed(_breed);
        BreedError = result.ErrorMessage;
        UpdateHasErrors();
    }

    private void ValidateAge()
    {
        var result = _validationService.ValidateAge(_age);
        AgeError = result.ErrorMessage;
        UpdateHasErrors();
    }
    
    private void UpdateHasErrors()
    {
        HasErrors = !string.IsNullOrEmpty(NameError) || !string.IsNullOrEmpty(BreedError) || !string.IsNullOrEmpty(AgeError);
    }
    
    // Cleanup
    protected override void OnDispose()
    {
        try
        {
            // Dispose managed resources
            _selectedImageStream?.Dispose();
            _selectedImageStream = null;
            
            // Clear references
            _selectedImage = null;
            _selectedImageFileName = null;
            _imageUrl = null;
            
            // Clear validation state
            _nameError = string.Empty;
            _breedError = string.Empty;
            _ageError = string.Empty;
            
            // Clear input fields
            _name = string.Empty;
            _breed = string.Empty;
            _age = string.Empty;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error disposing AddPetProfilePopupViewModel: {ex.Message}");
        }
    }
    

} 