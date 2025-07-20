using System.Windows.Input;
using Microsoft.Maui.Storage;

namespace PetProfiles.Maui.ViewModels;

public class ThemeViewModel : BaseViewModel
{
    private AppTheme _theme;
    public AppTheme Theme
    {
        get => _theme;
        set => SetProperty(ref _theme, value);
    }

    public ICommand ToggleThemeCommand { get; }

    public string ThemeIcon => Theme == AppTheme.Dark ? "moon.png" : "contrast.png";

    public ThemeViewModel()
    {
        // Load saved theme or default to Light
        var saved = Preferences.Get("AppTheme", "Light");
        Theme = saved == "Dark" ? AppTheme.Dark : AppTheme.Light;
        App.Current.UserAppTheme = Theme;

        ToggleThemeCommand = new ToggleThemeCommandImpl(this);
    }

    private void ToggleTheme()
    {
        Theme = Theme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
        App.Current.UserAppTheme = Theme;
        Preferences.Set("AppTheme", Theme == AppTheme.Dark ? "Dark" : "Light");
        OnPropertyChanged(nameof(ThemeIcon));
    }

    private class ToggleThemeCommandImpl : ICommand
    {
        private readonly ThemeViewModel _vm;
        public ToggleThemeCommandImpl(ThemeViewModel vm) => _vm = vm;
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _vm.ToggleTheme();
    }
} 