using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PetProfiles.Maui.ViewModels;

public abstract class BaseViewModel : INotifyPropertyChanged, IDisposable
{
    private bool _isBusy;
    private string _title = string.Empty;
    private bool _disposed = false;

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            // Clean up managed resources here
            OnDispose();
            _disposed = true;
        }
    }

    protected virtual void OnDispose()
    {
        // Override in derived classes to clean up specific resources
    }
} 