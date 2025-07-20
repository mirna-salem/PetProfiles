using PetProfiles.Maui.Views;
using PetProfiles.Maui.ViewModels;

namespace PetProfiles.Maui;

public partial class AppShell : Shell
{
	public AppShell(PetProfilesViewModel petProfilesViewModel, ThemeViewModel themeViewModel)
	{
		InitializeComponent();
		Routing.RegisterRoute("PetProfilesPage", typeof(PetProfilesPage));
		this.Items.Clear();
		this.Items.Add(new ShellContent
		{
			Title = "Pet Profiles",
			ContentTemplate = new DataTemplate(() => new PetProfilesPage(petProfilesViewModel, themeViewModel)),
			Route = "PetProfilesPage"
		});
	}
}
