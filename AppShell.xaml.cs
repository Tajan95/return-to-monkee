using ReturnToMonkee.Features.BewegungsErinnerungDemo;
using ReturnToMonkee.Features.PersonTest;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;

namespace ReturnToMonkee;

public partial class AppShell : Shell
{
	private readonly IReminderService? reminderService;

	public AppShell(IOnboardingRepository repo, MainPage mainPage, PersonListPage personListPage, BewegungsErinnerungPage bewegungsErinnerungPage, IReminderService reminderService)
	{
		InitializeComponent();

        _ = InitAsync(repo);

        Routing.RegisterRoute(nameof(PersonEditPage), typeof(PersonEditPage));

        HomeShellContent.Content = mainPage;
        PersonListShellContent.Content = personListPage;
		BewegungsErinnerungShellContent.Content = bewegungsErinnerungPage;

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
