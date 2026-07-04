using ReturnToMonkee.Infrastructure.Persistence;
using ReturnToMonkee.Infrastructure.Persistence.Entities;
using SQLite;

namespace ReturnToMonkee.Tests.Helpers;

/// <summary>
/// In-memory SQLite für Tests — kein Dateizugriff, jede Instanz vollständig isoliert.
/// Verwendet eine eindeutige Datei pro Instanz im Temp-Verzeichnis (SQLiteConnectionPool
/// cached ":memory:" über Instanzen hinweg, weshalb ein eindeutiger Pfad nötig ist).
/// </summary>
internal sealed class InMemoryLocalDatabase : ILocalDatabase
{
    // Unique temp file per instance — SQLiteConnectionPool caches by path,
    // so ":memory:" is shared; unique paths guarantee full isolation.
    private readonly string dbPath = Path.Combine(Path.GetTempPath(), $"rtm_test_{Guid.NewGuid():N}.db");
    private readonly SQLiteAsyncConnection connection;

    public InMemoryLocalDatabase()
    {
        connection = new SQLiteAsyncConnection(dbPath);
    }

    public Task EnsureDatabaseAccessibleAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<DatabaseHealthResult> CheckHealthAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(DatabaseHealthResult.Ready());

    public Task<SQLiteAsyncConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(connection);

    public async Task ResetAllDataAsync(CancellationToken cancellationToken = default)
    {
        await connection.CreateTableAsync<UserSettingsEntity>();
        await connection.DeleteAllAsync<UserSettingsEntity>();

        await connection.CreateTableAsync<OnboardingSettingsEntity>();
        await connection.DeleteAllAsync<OnboardingSettingsEntity>();

        await connection.CreateTableAsync<GoalEntity>();
        await connection.DeleteAllAsync<GoalEntity>();

        await connection.CreateTableAsync<UserGoalEntity>();
        await connection.DeleteAllAsync<UserGoalEntity>();

        await connection.CreateTableAsync<global::TimeLimitRule>();
        await connection.DeleteAllAsync<global::TimeLimitRule>();

        await connection.CreateTableAsync<global::NotificationEvent>();
        await connection.DeleteAllAsync<global::NotificationEvent>();
    }
}
