using ReturnToMonkee.Infrastructure.Persistence.Entities;
using SQLite;

namespace ReturnToMonkee.Infrastructure.Persistence.Repositories;

/// <summary>
/// SQLite-Repository für Benutzereinstellungen.
/// Verwaltet genau einen Datensatz (<see cref="UserSettingsEntity.DefaultId"/>).
///
/// Bewusst kein PRAGMA user_version (globales Pragma, konfliktanfällig bei mehreren Repos).
/// CreateTableAsync ergänzt neue Spalten automatisch beim nächsten App-Start (via MigrateTable intern) —
/// z. B. #18 kann MovementIntervalMinutes einfach zur Entity hinzufügen.
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
    public async Task<UserSettingsEntity> GetAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var connection = await GetDbConnectionAsync();
        return await connection.FindAsync<UserSettingsEntity>(UserSettingsEntity.DefaultId) ?? new UserSettingsEntity();
    }

    /// <inheritdoc/>
    public async Task SaveAsync(UserSettingsEntity settings, CancellationToken cancellationToken = default)
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
            await connection.CreateTableAsync<UserSettingsEntity>();
            schemaEnsured = true;
        }
        return connection;
    }
}
