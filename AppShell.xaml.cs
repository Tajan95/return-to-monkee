using ReturnToMonkee.Features.Interventions;
using ReturnToMonkee.Features.PersonTest;
using ReturnToMonkee.Features.Rules;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;
using ReturnToMonkee.Services;

namespace ReturnToMonkee;

public partial class AppShell : Shell
{
	private readonly IReminderService? reminderService;

	public AppShell(
        IOnboardingRepository repo,
        MainPage mainPage,
        PersonListPage personListPage,
        RulesPage rulesPage,
        TimeLimitInterventionPage timeLimitInterventionPage,
        IReminderService reminderService)
	{
		InitializeComponent();

        _ = InitAsync(repo);

        Routing.RegisterRoute(nameof(PersonEditPage), typeof(PersonEditPage));

        HomeShellContent.Content = mainPage;
        PersonListShellContent.Content = personListPage;
        RulesShellContent.Content = rulesPage;
        InterventionShellContent.Content = timeLimitInterventionPage;

		this.reminderService = reminderService;
		_ = this.reminderService.StartAsync();
    }

    private async Task InitAsync(IOnboardingRepository repo)
    {
        var completed = await repo.IsOnboardingCompletedAsync();

        await Task.Delay(50);

        if (AppSettings.ForceShowOnboarding || !completed)
            await GoToAsync("//onboarding");
        else
            await GoToAsync("//home");
    }
}
