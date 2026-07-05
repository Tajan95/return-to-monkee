using ReturnToMonkee.Infrastructure.Persistence.Entities;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;
using ReturnToMonkee.Onboarding;

namespace ReturnToMonkee.Tests.Features.Onboarding;

public sealed class GoalOrientationViewModelTests
{
    private const int GoalsStepIndex = 1;
    private const int MovementStepIndex = 2;

    [Fact]
    public async Task Next_IsBlocked_OnGoalsStep_WhenNoGoalSelected()
    {
        var viewModel = await CreateLoadedViewModelAsync();
        viewModel.CurrentStepIndex = GoalsStepIndex;

        await viewModel.NextCommand.ExecuteAsync(null);

        // Kein Ziel gewaehlt -> Schritt bleibt stehen und ein Hinweis erscheint.
        Assert.Equal(GoalsStepIndex, viewModel.CurrentStepIndex);
        Assert.False(string.IsNullOrEmpty(viewModel.ValidationMessage));
    }

    [Fact]
    public async Task Next_Advances_OnGoalsStep_WhenAtLeastOneGoalSelected()
    {
        var viewModel = await CreateLoadedViewModelAsync();
        viewModel.CurrentStepIndex = GoalsStepIndex;

        viewModel.ToggleGoalCommand.Execute(viewModel.Goals[0]);
        await viewModel.NextCommand.ExecuteAsync(null);

        Assert.Equal(MovementStepIndex, viewModel.CurrentStepIndex);
        Assert.True(string.IsNullOrEmpty(viewModel.ValidationMessage));
    }

    [Fact]
    public async Task ToggleGoal_ClearsValidationMessage_OnceAGoalIsSelected()
    {
        var viewModel = await CreateLoadedViewModelAsync();
        viewModel.CurrentStepIndex = GoalsStepIndex;

        // Blockieren, um eine Validierungsmeldung zu erzeugen ...
        await viewModel.NextCommand.ExecuteAsync(null);
        Assert.False(string.IsNullOrEmpty(viewModel.ValidationMessage));

        // ... danach Ziel waehlen: Meldung verschwindet.
        viewModel.ToggleGoalCommand.Execute(viewModel.Goals[0]);

        Assert.True(string.IsNullOrEmpty(viewModel.ValidationMessage));
    }

    private static async Task<GoalOrientationViewModel> CreateLoadedViewModelAsync()
    {
        var viewModel = new GoalOrientationViewModel(
            new StubGoalsRepository(),
            new StubOnboardingRepository(),
            new StubTimeLimitRuleRepository(),
            new StubUserSettingsRepository());

        await viewModel.LoadAsync();
        return viewModel;
    }

    private sealed class StubGoalsRepository : IGoalsRepository
    {
        public Task<List<GoalEntity>> GetAllGoalsAsync() => Task.FromResult(new List<GoalEntity>
        {
            new() { Id = 1, Title = "Mehr Fokus" },
            new() { Id = 2, Title = "Besser schlafen" }
        });

        public Task<List<int>> GetSelectedGoalIdsAsync() => Task.FromResult(new List<int>());

        public Task SaveSelectedGoalsAsync(List<int> goalIds) => Task.CompletedTask;

        public Task SeedAsync() => Task.CompletedTask;
    }

    private sealed class StubOnboardingRepository : IOnboardingRepository
    {
        public Task<string?> GetGoalOrientationAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);

        public Task<int> GetMovementReminderIntervalMinutesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(60);

        public Task<bool> GetMovementReminderEnabledAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task SaveGoalOrientationAsync(string goalOrientation, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveMovementReminderIntervalMinutesAsync(int intervalMinutes, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveMovementReminderEnabledAsync(bool isEnabled, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<bool> IsOnboardingCompletedAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(false);
    }

    private sealed class StubTimeLimitRuleRepository : ITimeLimitRuleRepository
    {
        public Task SaveInitialTimeLimitRuleAsync(string category, int timeLimitMinutes, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<List<global::TimeLimitRule>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new List<global::TimeLimitRule>());

        public Task AddAsync(global::TimeLimitRule rule, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task UpdateAsync(global::TimeLimitRule rule, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task DeleteAsync(global::TimeLimitRule rule, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class StubUserSettingsRepository : IUserSettingsRepository
    {
        public Task<UserSettingsEntity> GetAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new UserSettingsEntity());

        public Task SaveAsync(UserSettingsEntity settings, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
