using ReturnToMonkee.Features.PersonTest;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;

namespace ReturnToMonkee;

public partial class AppShell : Shell
{
    public AppShell(IOnboardingRepository repo)
    {
        InitializeComponent();

        _ = InitAsync(repo);
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
