using ReturnToMonkee.Infrastructure.Persistence.Entities;
using SQLite;

namespace ReturnToMonkee.Infrastructure.Persistence.Repositories;

public sealed class OnboardingRepository : IOnboardingRepository
{
    private readonly ILocalDatabase localDatabase;

    public OnboardingRepository(
        ILocalDatabase localDatabase)
    {
        this.localDatabase = localDatabase;
    }

    public async Task SaveGoalOrientationAsync(
        string goalOrientation,
        CancellationToken cancellationToken = default)
    {
        var connection =
            await localDatabase.GetConnectionAsync(cancellationToken);

        await connection.CreateTableAsync<OnboardingSettingsEntity>();

        await connection.InsertOrReplaceAsync(
            new OnboardingSettingsEntity
            {
                Id = 1,
                GoalOrientation = goalOrientation
            });
    }

    public async Task<string?> GetGoalOrientationAsync(
        CancellationToken cancellationToken = default)
    {
        var connection =
            await localDatabase.GetConnectionAsync(cancellationToken);

        await connection.CreateTableAsync<OnboardingSettingsEntity>();

        var settings =
            await connection.Table<OnboardingSettingsEntity>()
                .FirstOrDefaultAsync();

        return settings?.GoalOrientation;
    }

    public async Task<bool> IsOnboardingCompletedAsync(
    CancellationToken cancellationToken = default)
    {
        var connection =
            await localDatabase.GetConnectionAsync(cancellationToken);

        await connection.CreateTableAsync<OnboardingSettingsEntity>();

        var settings =
            await connection.Table<OnboardingSettingsEntity>()
                .FirstOrDefaultAsync();

        return !string.IsNullOrWhiteSpace(settings?.GoalOrientation);
    }
}