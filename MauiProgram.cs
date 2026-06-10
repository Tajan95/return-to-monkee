using Microsoft.Extensions.Logging;
using ReturnToMonkee.Features.BewegungsErinnerungDemo;
using ReturnToMonkee.Features.TestStringDemo;
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
		builder.Services.AddSingleton<ITestStringRepository, TestStringRepository>();
		builder.Services.AddSingleton<IBewegungsErinnerungsRepository, BewegungsErinnerungsRepository>();
		builder.Services.AddSingleton<IReminderService, ReminderService>();
		builder.Services.AddSingleton<BewegungsErinnerungViewModel>();
		builder.Services.AddSingleton<MainPage>();
		builder.Services.AddSingleton<TestStringCrudPage>();
		builder.Services.AddSingleton<BewegungsErinnerungPage>();
		builder.Services.AddSingleton<AppShell>();

		return builder.Build();
	}
}
