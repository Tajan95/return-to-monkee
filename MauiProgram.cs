using Microsoft.Extensions.Logging;
using ReturnToMonkee.Features.Onboarding;
using ReturnToMonkee.Features.PersonTest;
using ReturnToMonkee.Features.Reminders;
using ReturnToMonkee.Features.Rules;
using ReturnToMonkee.Features.Settings;
using ReturnToMonkee.Features.Statistics;
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
				fonts.AddFont("fa-solid-900.ttf", "FontAwesomeSolid");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Datenbank
		builder.Services.AddSingleton<ILocalDatabase, LocalDatabase>();
		builder.Services.AddSingleton<IReminderService, ReminderService>();
		builder.Services.AddSingleton<INotificationEventRepository, NotificationEventRepository>();

		// Notifications
		builder.Services.AddSingleton<INotificationAdapter, MockNotificationAdapter>();

		// Services
		builder.Services.AddSingleton<IPersonRepository, PersonRepository>();
		builder.Services.AddSingleton<DemoDataSeeder>();
		builder.Services.AddSingleton<IOnboardingRepository, OnboardingRepository>();
		builder.Services.AddSingleton<IGoalsRepository, GoalsRepository>();
		builder.Services.AddSingleton<ITimeLimitRuleRepository, TimeLimitRuleRepository>();
		builder.Services.AddSingleton<IUserSettingsRepository, UserSettingsRepository>();

        // Seiten
        builder.Services.AddSingleton<MainPage>();
		builder.Services.AddSingleton<PersonListPage>();
		builder.Services.AddTransient<PersonEditPage>();
		builder.Services.AddTransient<GoalOrientationView>();
		builder.Services.AddTransient<GoalOrientationViewModel>();
		builder.Services.AddSingleton<SettingsPage>();
		builder.Services.AddTransient<OnboardingStep2Page>();

		// Regeln
		builder.Services.AddSingleton<RulesViewModel>();
		builder.Services.AddSingleton<RulesPage>();

		// Reminder-Pages (Bewegung / Schlaf)
		builder.Services.AddTransient<MovementReminderViewModel>();
		builder.Services.AddTransient<MovementReminderPage>();
		builder.Services.AddTransient<SleepReminderViewModel>();
		builder.Services.AddTransient<SleepReminderPage>();

		// Statistics Services
		builder.Services.AddSingleton<INotificationEventQueryRepository, NotificationEventQueryRepository>();
        builder.Services.AddSingleton<IStatisticsService, StatisticsService>();	
		builder.Services.AddSingleton<StatisticsViewModel>();
		builder.Services.AddSingleton<StatisticsView>();

		builder.Services.AddSingleton<AppShell>();

		return builder.Build();
	}
}
