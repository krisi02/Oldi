using Microsoft.Extensions.Logging;
using System.Globalization;
using DailyPlannerMauiBlazor.Remote;
using DailyPlannerMauiBlazor.Services;

namespace DailyPlannerMauiBlazor;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		// Set Italian culture
		var italianCulture = new CultureInfo("it-IT");
		CultureInfo.DefaultThreadCurrentCulture = italianCulture;
		CultureInfo.DefaultThreadCurrentUICulture = italianCulture;

		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		// Register services
		builder.Services.AddSingleton<IStorageService, MauiStorageService>();
		
		// Register HttpClient-based Remote API
		var remoteAPI = new DailyPlannerAPI(
			DailyPlannerMauiBlazor.Utility.GeneralUtility.address,
			DailyPlannerMauiBlazor.Utility.GeneralUtility.apiPrefix
		);
		builder.Services.AddSingleton<IRemoteAPI>(remoteAPI);
		
		// Register Repository
		builder.Services.AddSingleton<DailyPlannerMauiBlazor.Repository.IRepository, MauiRepository>();

		return builder.Build();
	}
}
