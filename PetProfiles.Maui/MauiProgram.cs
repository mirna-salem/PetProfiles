using Microsoft.Extensions.Logging;
using PetProfiles.Maui.Services;
using PetProfiles.Maui.ViewModels;
using PetProfiles.Maui.Views;
using CommunityToolkit.Maui;

namespace PetProfiles.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Register services
		builder.Services.AddHttpClient();
		builder.Services.AddSingleton<IValidationService, ValidationService>();
#if DEBUG
		builder.Services.AddSingleton<IPetProfilesService, MockPetProfilesService>();
#else
		builder.Services.AddSingleton<IPetProfilesService, PetProfilesService>();
#endif
		
		// Register ViewModels
		builder.Services.AddTransient<PetProfilesViewModel>();
		
		// Register Views
		builder.Services.AddTransient<PetProfilesPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
