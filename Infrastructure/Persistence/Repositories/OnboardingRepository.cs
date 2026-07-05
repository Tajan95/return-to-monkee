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

        await EnsureSettingsTableAsync(connection);

        var settings = await GetOrCreateSettingsAsync(connection);
        settings.GoalOrientation = goalOrientation;

        await connection.InsertOrReplaceAsync(
            settings);
    }

    public async Task SaveMovementReminderIntervalMinutesAsync(
        int intervalMinutes,
        CancellationToken cancellationToken = default)
    {
        if (intervalMinutes is not (30 or 60 or 90))
            throw new ArgumentOutOfRangeException(
                nameof(intervalMinutes),
                "Movement reminder interval must be 30, 60, or 90 minutes.");

        var connection =
            await localDatabase.GetConnectionAsync(cancellationToken);

        await EnsureSettingsTableAsync(connection);

        var settings = await GetOrCreateSettingsAsync(connection);
        settings.MovementReminderIntervalMinutes = intervalMinutes;

        await connection.InsertOrReplaceAsync(settings);
    }

    public async Task<string?> GetGoalOrientationAsync(
        CancellationToken cancellationToken = default)
    {
        var connection =
            await localDatabase.GetConnectionAsync(cancellationToken);

        await EnsureSettingsTableAsync(connection);

        var settings =
            await connection.Table<OnboardingSettingsEntity>()
                .FirstOrDefaultAsync();

        return settings?.GoalOrientation;
    }

    public async Task<int> GetMovementReminderIntervalMinutesAsync(
        CancellationToken cancellationToken = default)
    {
        var connection =
            await localDatabase.GetConnectionAsync(cancellationToken);

        await EnsureSettingsTableAsync(connection);

        var settings =
            await connection.Table<OnboardingSettingsEntity>()
                .FirstOrDefaultAsync();

        return settings?.MovementReminderIntervalMinutes is 30 or 60 or 90
            ? settings.MovementReminderIntervalMinutes
            : 60;
    }

    public async Task<bool> GetMovementReminderEnabledAsync(
        CancellationToken cancellationToken = default)
    {
        var connection =
            await localDatabase.GetConnectionAsync(cancellationToken);

        await EnsureSettingsTableAsync(connection);

        var settings =
            await connection.Table<OnboardingSettingsEntity>()
                .FirstOrDefaultAsync();

        return settings?.MovementReminderEnabled ?? true;
    }

    public async Task SaveMovementReminderEnabledAsync(
        bool isEnabled,
        CancellationToken cancellationToken = default)
    {
        var connection =
            await localDatabase.GetConnectionAsync(cancellationToken);

        await EnsureSettingsTableAsync(connection);

        var settings = await GetOrCreateSettingsAsync(connection);
        settings.MovementReminderEnabled = isEnabled;

        await connection.InsertOrReplaceAsync(settings);
    }

    public async Task<bool> IsOnboardingCompletedAsync(
    CancellationToken cancellationToken = default)
    {
        var connection =
            await localDatabase.GetConnectionAsync(cancellationToken);

        await EnsureSettingsTableAsync(connection);

        var settings =
            await connection.Table<OnboardingSettingsEntity>()
                .FirstOrDefaultAsync();

        return string.Equals(
            settings?.GoalOrientation,
            "completed",
            StringComparison.OrdinalIgnoreCase);
    }

    private static async Task EnsureSettingsTableAsync(SQLiteAsyncConnection connection)
    {
        await connection.CreateTableAsync<OnboardingSettingsEntity>();

        var columns = await connection.GetTableInfoAsync("OnboardingSettings");
        if (columns.All(column => column.Name != nameof(OnboardingSettingsEntity.MovementReminderIntervalMinutes)))
        {
            await connection.ExecuteAsync(
                $"ALTER TABLE OnboardingSettings ADD COLUMN {nameof(OnboardingSettingsEntity.MovementReminderIntervalMinutes)} INTEGER NOT NULL DEFAULT 60");
        }

        if (columns.All(column => column.Name != nameof(OnboardingSettingsEntity.MovementReminderEnabled)))
        {
            await connection.ExecuteAsync(
                $"ALTER TABLE OnboardingSettings ADD COLUMN {nameof(OnboardingSettingsEntity.MovementReminderEnabled)} INTEGER NOT NULL DEFAULT 1");
        }
    }

    private static async Task<OnboardingSettingsEntity> GetOrCreateSettingsAsync(
        SQLiteAsyncConnection connection)
    {
        return await connection.Table<OnboardingSettingsEntity>()
            .Where(settings => settings.Id == 1)
            .FirstOrDefaultAsync()
            ?? new OnboardingSettingsEntity { Id = 1 };
    }
}
