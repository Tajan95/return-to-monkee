using ReturnToMonkee.Features.Interventions;
using ReturnToMonkee.Features.Onboarding;
using ReturnToMonkee.Features.PersonTest;
using ReturnToMonkee.Features.Rules;
using ReturnToMonkee.Features.Settings;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;
using ReturnToMonkee.Services;

namespace ReturnToMonkee;

public partial class AppShell : Shell
{
	private readonly IReminderService? reminderService;

	public AppShell(
        IOnboardingRepository repo,
        IUserSettingsRepository userSettingsRepository,
        MainPage mainPage,
        PersonListPage personListPage,
        RulesPage rulesPage,
        TimeLimitInterventionPage timeLimitInterventionPage,
        IReminderService reminderService,
        SettingsPage settingsPage)
	{
		InitializeComponent();

        _ = InitAsync(repo, userSettingsRepository);

        Routing.RegisterRoute(nameof(PersonEditPage), typeof(PersonEditPage));
        Routing.RegisterRoute(nameof(OnboardingStep2Page), typeof(OnboardingStep2Page));

        HomeShellContent.Content = mainPage;
        PersonListShellContent.Content = personListPage;
        RulesShellContent.Content = rulesPage;
        InterventionShellContent.Content = timeLimitInterventionPage;
        SettingsShellContent.Content = settingsPage;

		this.reminderService = reminderService;
		_ = this.reminderService.StartAsync();
    }

    private async Task InitAsync(IOnboardingRepository repo, IUserSettingsRepository userSettingsRepository)
    {
        var completed = await repo.IsOnboardingCompletedAsync();
        var settings = await userSettingsRepository.GetAsync();

        await Task.Delay(50);

        if (settings.ShowOnboardingOnStartup || !completed)
            await GoToAsync("//onboarding");
        else
            await GoToAsync("//home");
    }
}
