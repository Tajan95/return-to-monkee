using ReturnToMonkee.Features.BewegungsErinnerungDemo;
using ReturnToMonkee.Features.Onboarding;
using ReturnToMonkee.Features.PersonTest;
using ReturnToMonkee.Features.Settings;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;

namespace ReturnToMonkee;

public partial class AppShell : Shell
{
	private readonly IReminderService? reminderService;

	public AppShell(IOnboardingRepository repo, MainPage mainPage, PersonListPage personListPage, BewegungsErinnerungPage bewegungsErinnerungPage, IReminderService reminderService, SettingsPage settingsPage)
	{
		InitializeComponent();

        _ = InitAsync(repo);

        Routing.RegisterRoute(nameof(PersonEditPage), typeof(PersonEditPage));
        Routing.RegisterRoute(nameof(OnboardingStep2Page), typeof(OnboardingStep2Page));

        HomeShellContent.Content = mainPage;
        PersonListShellContent.Content = personListPage;
		BewegungsErinnerungShellContent.Content = bewegungsErinnerungPage;
        SettingsShellContent.Content = settingsPage;

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
