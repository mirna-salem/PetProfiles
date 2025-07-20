namespace PetProfiles.Maui;

public partial class App : Application
{
    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        ServiceProvider = serviceProvider;
    }

    public IServiceProvider ServiceProvider { get; }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = ServiceProvider.GetRequiredService<AppShell>();
        return new Window(shell);
    }
}