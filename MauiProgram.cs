using Microsoft.Extensions.Logging;
using ReturnToMonkee.Features.BewegungsErinnerung;
using ReturnToMonkee.Features.Onboarding;
using ReturnToMonkee.Features.PersonTest;
using ReturnToMonkee.Infrastructure.Notifications;
using ReturnToMonkee.Infrastructure.Persistence;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;
using ReturnToMonkee.Onboarding;
using ReturnToMonkee.Services;

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
		builder.Services.AddSingleton<IBewegungsErinnerungsRepository, BewegungsErinnerungsRepository>();
		builder.Services.AddSingleton<IReminderService, ReminderService>();

		// Notifications
		builder.Services.AddSingleton<INotificationAdapter, MockNotificationAdapter>();

		// Services
		builder.Services.AddSingleton<IPersonRepository, PersonRepository>();
		builder.Services.AddSingleton<DemoDataSeeder>();
        builder.Services.AddSingleton<IStartupNavigator, StartupNavigator>();
        builder.Services.AddSingleton<IOnboardingRepository, OnboardingRepository>();
		builder.Services.AddSingleton<IGoalsRepository, GoalsRepository>();

        // Seiten
        builder.Services.AddSingleton<MainPage>();
		builder.Services.AddSingleton<PersonListPage>();
		builder.Services.AddTransient<PersonEditPage>();
		builder.Services.AddSingleton<BewegungsErinnerungPage>();
		builder.Services.AddSingleton<BewegungsErinnerungViewModel>();
        builder.Services.AddTransient<GoalOrientationView>();
        builder.Services.AddTransient<GoalOrientationViewModel>();
        builder.Services.AddSingleton<AppShell>();

		return builder.Build();
	}
}
