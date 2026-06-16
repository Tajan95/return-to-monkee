using ReturnToMonkee;
using ReturnToMonkee.Features.Onboarding;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;

public sealed class StartupNavigator : IStartupNavigator
{
    private readonly IOnboardingRepository onboardingRepository;
    private readonly IServiceProvider serviceProvider;

    public StartupNavigator(
        IOnboardingRepository onboardingRepository,
        IServiceProvider serviceProvider)
    {
        this.onboardingRepository = onboardingRepository;
        this.serviceProvider = serviceProvider;
    }

    public async Task<Page> GetStartPageAsync()
    {
        if (AppSettings.ForceShowOnboarding)
        {
            return serviceProvider.GetRequiredService<GoalOrientationView>();
        }

        var onboardingCompleted =
            await onboardingRepository.IsOnboardingCompletedAsync();

        if (!onboardingCompleted)
        {
            return serviceProvider.GetRequiredService<GoalOrientationView>();
        }

        return serviceProvider.GetRequiredService<MainPage>();
    }
}