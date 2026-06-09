using Microsoft.Extensions.Logging;
using ReturnToMonkee.Features.PersonTest;
using ReturnToMonkee.Infrastructure.Persistence;

namespace ReturnToMonkee;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});


#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Datenbank
		builder.Services.AddSingleton<ILocalDatabase, LocalDatabase>();

		// Services
		builder.Services.AddSingleton<IPersonRepository, PersonRepository>();

		// Seiten
		builder.Services.AddSingleton<MainPage>();
		builder.Services.AddSingleton<AppShell>();

		return builder.Build();
	}
}
