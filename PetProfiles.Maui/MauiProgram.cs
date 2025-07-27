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
		builder.Services.AddSingleton<IPetProfileCacheService, PetProfileCacheService>();
#if DEBUG
		builder.Services.AddSingleton<IPetProfilesService, MockPetProfilesService>();
#else
		builder.Services.AddSingleton<IPetProfilesService, PetProfilesService>();
#endif
		
		// Register ViewModels
		builder.Services.AddTransient<PetProfilesViewModel>();
		builder.Services.AddSingleton<ThemeViewModel>();
		
		// Register Views
		builder.Services.AddTransient<PetProfilesPage>();
		builder.Services.AddSingleton<AppShell>(sp =>
    new AppShell(
        sp.GetRequiredService<PetProfilesViewModel>(),
        sp.GetRequiredService<ThemeViewModel>()
    )
);

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
