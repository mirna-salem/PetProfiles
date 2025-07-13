using CommunityToolkit.Maui.Views;
using PetProfiles.Maui.ViewModels;
using PetProfiles.Maui.Models;
using Microsoft.Extensions.DependencyInjection;
using PetProfiles.Maui.Services;

namespace PetProfiles.Maui.Views;

public partial class AddPetProfilePopup : Popup
{
    public AddPetProfileResult? PopupResult { get; private set; }

    public AddPetProfilePopup() : this(null) {}

    public AddPetProfilePopup(PetProfile? pet)
    {
        InitializeComponent();
        var validationService = Handler?.MauiContext?.Services.GetService<IValidationService>() 
            ?? new ValidationService();
        var viewModel = pet != null 
            ? new AddPetProfilePopupViewModel(validationService, pet)
            : new AddPetProfilePopupViewModel(validationService);
        viewModel.SaveCompleted += OnSaveCompleted;
        viewModel.CancelRequested += OnCancelRequested;
        BindingContext = viewModel;
    }

    private void OnSaveCompleted(AddPetProfileResult result)
    {
        PopupResult = result;
        CloseAsync();
    }

    private void OnCancelRequested()
    {
        PopupResult = null;
        CloseAsync();
    }
} 