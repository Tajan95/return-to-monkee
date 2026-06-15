using ReturnToMonkee.Infrastructure.Persistence;
using SQLite;

namespace ReturnToMonkee.Features.Settings;

/// <summary>
/// SQLite-Repository für Benutzereinstellungen.
/// Verwaltet genau einen Datensatz (<see cref="UserSettings.DefaultId"/>).
///
/// Bewusst kein PRAGMA user_version (globales Pragma, konfliktanfällig bei mehreren Repos).
/// CreateTableAsync ergänzt neue Spalten automatisch beim nächsten App-Start (via MigrateTable intern) —
/// #16 und #18 können GoalDirection/MovementIntervalMinutes einfach zur Entity hinzufügen.
/// </summary>
public sealed class UserSettingsRepository : IUserSettingsRepository
{
    private bool schemaEnsured;
    private readonly ILocalDatabase localDatabase;

    public UserSettingsRepository(ILocalDatabase localDatabase)
    {
        this.localDatabase = localDatabase;
    }

    /// <inheritdoc/>
    public async Task<UserSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var connection = await GetDbConnectionAsync();
        return await connection.FindAsync<UserSettings>(UserSettings.DefaultId) ?? new UserSettings();
    }

    /// <inheritdoc/>
    public async Task SaveAsync(UserSettings settings, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var connection = await GetDbConnectionAsync();
        await connection.InsertOrReplaceAsync(settings);
    }

    private async Task<SQLiteAsyncConnection> GetDbConnectionAsync()
    {
        var connection = await localDatabase.GetConnectionAsync();
        if (!schemaEnsured)
        {
            await connection.CreateTableAsync<UserSettings>();
            schemaEnsured = true;
        }
        return connection;
    }
}
