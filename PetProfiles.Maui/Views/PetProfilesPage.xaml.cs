using PetProfiles.Maui.ViewModels;
using PetProfiles.Maui.Views;
using Microsoft.Maui.Controls;

namespace PetProfiles.Maui.Views;

public partial class PetProfilesPage : ContentPage
{
    private bool _hasLoadedOnce = false;

    public PetProfilesPage(PetProfilesViewModel viewModel, ThemeViewModel themeViewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        ThemeToggleButton.BindingContext = themeViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_hasLoadedOnce)
        {
            if (BindingContext is PetProfilesViewModel viewModel)
            {
                await viewModel.LoadPetProfilesAsync();
            }
            _hasLoadedOnce = true;
        }
    }
} 