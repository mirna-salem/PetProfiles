using System.Collections.ObjectModel;
using System.Windows.Input;
using PetProfiles.Maui.Models;
using PetProfiles.Maui.Services;
using CommunityToolkit.Maui.Views;
using PetProfiles.Maui.Views;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui;
using System.Linq;
using Microsoft.Maui.Controls;

namespace PetProfiles.Maui.ViewModels;

public class PetProfilesViewModel : BaseViewModel
{
    private readonly IPetProfilesService _petProfilesService;
    private readonly IImagePreloadService _imagePreloadService;
    private PetProfile? _selectedPetProfile;
    private bool _isLoading;
    private static bool _isPopupOpen = false;

    public PetProfilesViewModel(IPetProfilesService petProfilesService, IImagePreloadService imagePreloadService)
    {
        _petProfilesService = petProfilesService;
        _imagePreloadService = imagePreloadService;
        Title = "Pet Profiles";
        
        PetProfiles = new ObservableCollection<PetProfile>();
        
        LoadPetProfilesCommand = new Command(async () => await LoadPetProfilesAsync());
        AddPetProfileCommand = new Command(async () => await ShowAddPetProfilePopupAsync());
        DeletePetProfileCommand = new Command<PetProfile>(async (pet) => await DeletePetProfileAsync(pet));
        EditPetProfileCommand = new Command<PetProfile>(async (pet) => await ShowEditPetProfilePopupAsync(pet));
    }

    public ObservableCollection<PetProfile> PetProfiles { get; }

    public PetProfile? SelectedPetProfile
    {
        get => _selectedPetProfile;
        set => SetProperty(ref _selectedPetProfile, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ICommand LoadPetProfilesCommand { get; }
    public ICommand AddPetProfileCommand { get; }
    public ICommand DeletePetProfileCommand { get; }
    public ICommand EditPetProfileCommand { get; }

    public async Task LoadPetProfilesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            IsLoading = true;

            var profiles = await _petProfilesService.GetPetProfilesAsync();
            
            // Pre-load images BEFORE adding to UI
            await _imagePreloadService.PreloadImagesAsync(profiles);
            
            // Only clear and update if we successfully got data
            PetProfiles.Clear();
            foreach (var profile in profiles)
            {
                PetProfiles.Add(profile);
            }
        }
        catch (Exception ex)
        {
            // In a real app, you'd want to show a user-friendly error message
            await Application.Current?.MainPage?.DisplayAlert("Error", "Failed to load pet profiles", "OK");
        }
        finally
        {
            IsBusy = false;
            IsLoading = false;
        }
    }

    private async Task ShowAddPetProfilePopupAsync()
    {
        var mainPage = Application.Current?.MainPage;
        if (mainPage is null) return;
        
        // Check if a popup is already open
        if (_isPopupOpen)
        {
            System.Diagnostics.Debug.WriteLine("Popup already open, ignoring request");
            return;
        }
        
        _isPopupOpen = true;
        AddPetProfilePopup? popup = null;
        
        try
        {
            popup = new AddPetProfilePopup();
            var options = new PopupOptions
            {
                Shape = null, // Remove border
                Shadow = null  // Remove shadow
            };
            
            await mainPage.ShowPopupAsync(popup, options);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("Ambiguous routes"))
        {
            // Handle the ambiguous routes error
            System.Diagnostics.Debug.WriteLine($"Popup routing error: {ex.Message}");
            return;
        }
        catch (Exception ex)
        {
            // Handle any other popup-related errors
            System.Diagnostics.Debug.WriteLine($"Popup error: {ex.Message}");
            return;
        }
        finally
        {
            _isPopupOpen = false;
        }
        
        if (popup.PopupResult != null)
        {
            try
            {
                IsBusy = true;
                PetProfile createdPet;
                if (popup.PopupResult.ImageStream != null && !string.IsNullOrEmpty(popup.PopupResult.ImageFileName))
                {
                    createdPet = await _petProfilesService.CreatePetProfileWithImageAsync(
                        popup.PopupResult.Name,
                        popup.PopupResult.Breed,
                        popup.PopupResult.Age,
                        popup.PopupResult.ImageStream,
                        popup.PopupResult.ImageFileName);
                }
                else
                {
                    var newPet = new PetProfile
                    {
                        Name = popup.PopupResult.Name,
                        Breed = popup.PopupResult.Breed,
                        Age = popup.PopupResult.Age,
                        ImageUrl = popup.PopupResult.ImageUrl
                    };
                    createdPet = await _petProfilesService.CreatePetProfileAsync(newPet);
                }
                // Reload the pet profiles from the backend to get the correct image URLs
                // await LoadPetProfilesAsync();
                // Add the new pet profile directly to the collection for instant UI update
                PetProfiles.Add(createdPet);
                
                // Dispose the popup's ViewModel after successful upload
                if (popup.BindingContext is AddPetProfilePopupViewModel popupViewModel)
                {
                    popupViewModel.Dispose();
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"🌐 Network error: {ex.Message}");
                await mainPage.DisplayAlert("Network Error", $"Failed to connect to server: {ex.Message}", "OK");
                
                // Dispose the popup's ViewModel even on error
                if (popup.BindingContext is AddPetProfilePopupViewModel popupViewModel)
                {
                    popupViewModel.Dispose();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error creating pet profile: {ex.Message}");
                await mainPage.DisplayAlert("Error", $"Failed to create pet profile: {ex.Message}", "OK");
                
                // Dispose the popup's ViewModel even on error
                if (popup.BindingContext is AddPetProfilePopupViewModel popupViewModel)
                {
                    popupViewModel.Dispose();
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    private async Task DeletePetProfileAsync(PetProfile? petProfile)
    {
        if (petProfile == null) return;

        var confirmed = await Application.Current?.MainPage?.DisplayAlert(
            "Delete Pet Profile", 
            $"Are you sure you want to delete {petProfile.Name}?", 
            "Delete", 
            "Cancel");

        if (confirmed == true)
        {
            try
            {
                IsBusy = true;
                var success = await _petProfilesService.DeletePetProfileAsync(petProfile.Id);
                if (success)
                {
                    PetProfiles.Remove(petProfile);
                }
                else
                {
                    await Application.Current?.MainPage?.DisplayAlert("Error", "Failed to delete pet profile", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current?.MainPage?.DisplayAlert("Error", "Failed to delete pet profile", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    public async Task ShowEditPetProfilePopupAsync(PetProfile pet)
    {
        if (pet == null) return;
        var mainPage = Application.Current?.MainPage;
        if (mainPage is null) return;
        
        // Check if a popup is already open
        if (_isPopupOpen)
        {
            System.Diagnostics.Debug.WriteLine("Popup already open, ignoring request");
            return;
        }
        
        _isPopupOpen = true;
        AddPetProfilePopup? popup = null;
        
        try
        {
            popup = new AddPetProfilePopup(pet);
            var options = new PopupOptions { Shape = null, Shadow = null };
            
            await mainPage.ShowPopupAsync(popup, options);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("Ambiguous routes"))
        {
            // Handle the ambiguous routes error
            System.Diagnostics.Debug.WriteLine($"Popup routing error: {ex.Message}");
            return;
        }
        catch (Exception ex)
        {
            // Handle any other popup-related errors
            System.Diagnostics.Debug.WriteLine($"Popup error: {ex.Message}");
            return;
        }
        finally
        {
            _isPopupOpen = false;
            
            // Ensure popup is disposed if it was created
            if (popup != null)
            {
                try
                {
                    if (popup.BindingContext is AddPetProfilePopupViewModel viewModel)
                    {
                        viewModel.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error disposing popup ViewModel: {ex.Message}");
                }
            }
        }
        if (popup.PopupResult != null)
        {
            try
            {
                IsBusy = true;
                if (popup.PopupResult.PetId.HasValue)
                {
                    var updatedPet = new PetProfile
                    {
                        Id = popup.PopupResult.PetId.Value,
                        Name = popup.PopupResult.Name,
                        Breed = popup.PopupResult.Breed,
                        Age = popup.PopupResult.Age,
                        ImageUrl = popup.PopupResult.ImageUrl
                    };
                    PetProfile? updatedFromApi = null;
                    if (popup.PopupResult.ImageStream != null && !string.IsNullOrEmpty(popup.PopupResult.ImageFileName))
                    {
                        // User uploaded a new image during edit
                        updatedFromApi = await _petProfilesService.UpdatePetProfileImageAsync(updatedPet.Id, popup.PopupResult.ImageStream, popup.PopupResult.ImageFileName);
                        if (updatedFromApi != null)
                        {
                            updatedPet.ImageUrl = updatedFromApi.ImageUrl;
                        }
                    }
                    var success = await _petProfilesService.UpdatePetProfileAsync(updatedPet);
                    if (success)
                    {
                        var existing = PetProfiles.FirstOrDefault(p => p.Id == updatedPet.Id);
                        if (existing != null)
                        {
                            if (existing.Name != updatedPet.Name)
                                existing.Name = updatedPet.Name;
                            if (existing.Breed != updatedPet.Breed)
                                existing.Breed = updatedPet.Breed;
                            if (existing.Age != updatedPet.Age)
                                existing.Age = updatedPet.Age;
                            if (existing.ImageUrl != updatedPet.ImageUrl)
                                existing.ImageUrl = updatedPet.ImageUrl;
                        }
                    }
                }
            }
            finally
            {
                IsBusy = false;
                SelectedPetProfile = null;
            }
        }
    }
} 