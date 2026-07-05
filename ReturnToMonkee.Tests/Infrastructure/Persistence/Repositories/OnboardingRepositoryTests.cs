using ReturnToMonkee.Infrastructure.Persistence.Repositories;
using ReturnToMonkee.Tests.Helpers;

namespace ReturnToMonkee.Tests.Infrastructure.Persistence.Repositories;

public sealed class OnboardingRepositoryTests
{
    private static OnboardingRepository CreateRepository()
        => new(new InMemoryLocalDatabase());

    [Fact]
    public async Task IsOnboardingCompletedAsync_ReturnsFalse_WhenOnlyGoalOrientationWasSaved()
    {
        var repository = CreateRepository();

        await repository.SaveGoalOrientationAsync("1,2");

        var isCompleted = await repository.IsOnboardingCompletedAsync();

        Assert.False(isCompleted);
    }

    [Fact]
    public async Task IsOnboardingCompletedAsync_ReturnsTrue_WhenCompletedMarkerWasSaved()
    {
        var repository = CreateRepository();

        await repository.SaveGoalOrientationAsync("completed");

        var isCompleted = await repository.IsOnboardingCompletedAsync();

        Assert.True(isCompleted);
    }

    [Fact]
    public async Task GetMovementReminderEnabledAsync_ReturnsTrue_WhenNothingPersisted()
    {
        var repository = CreateRepository();

        var isEnabled = await repository.GetMovementReminderEnabledAsync();

        Assert.True(isEnabled);
    }

    [Fact]
    public async Task SaveMovementReminderEnabledAsync_PersistsValue()
    {
        var repository = CreateRepository();

        await repository.SaveMovementReminderEnabledAsync(false);
        var isEnabled = await repository.GetMovementReminderEnabledAsync();

        Assert.False(isEnabled);
    }
}
