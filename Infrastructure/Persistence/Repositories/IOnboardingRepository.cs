

namespace ReturnToMonkee.Infrastructure.Persistence.Repositories;

public interface IOnboardingRepository
{
    Task<string?> GetGoalOrientationAsync(
        CancellationToken cancellationToken = default);

    Task<int> GetMovementReminderIntervalMinutesAsync(
        CancellationToken cancellationToken = default);

    Task SaveGoalOrientationAsync(
        string goalOrientation,
        CancellationToken cancellationToken = default);

    Task SaveMovementReminderIntervalMinutesAsync(
        int intervalMinutes,
        CancellationToken cancellationToken = default);

    Task<bool> IsOnboardingCompletedAsync(
       CancellationToken cancellationToken = default);
}
