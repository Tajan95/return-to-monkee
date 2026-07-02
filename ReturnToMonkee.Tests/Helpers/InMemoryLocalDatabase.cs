using ReturnToMonkee.Infrastructure.Persistence;
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
}
