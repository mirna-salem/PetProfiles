using PetProfiles.Maui.Views;

namespace PetProfiles.Maui;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(PetProfilesPage), typeof(PetProfilesPage));
	}
}
