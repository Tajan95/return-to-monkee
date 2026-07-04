using ReturnToMonkee.Infrastructure.Persistence.Entities;
using SQLite;
using System.Diagnostics;

namespace ReturnToMonkee.Infrastructure.Persistence;

public sealed class LocalDatabase : ILocalDatabase
{
	private const string DatabaseFilename = "return_to_monkee.db3";

	// �ffnet die Datenbank schreibend und legt sie an, falls sie noch nicht existiert.
	private static readonly SQLiteOpenFlags OpenFlags =
		SQLiteOpenFlags.ReadWrite |
		SQLiteOpenFlags.Create |
		SQLiteOpenFlags.SharedCache;

	// Die Verbindung wird erst erstellt, wenn sie zum ersten Mal gebraucht wird.
	private readonly Lazy<SQLiteAsyncConnection> connection = new(CreateConnection);







    // F�hrt eine einfache Abfrage aus und liefert einen UI - tauglichen Status zur�ck.
    public async Task<DatabaseHealthResult> CheckHealthAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			await EnsureDatabaseAccessibleAsync(cancellationToken);
			return DatabaseHealthResult.Ready();
		}
		catch (Exception exception)
		{
			return DatabaseHealthResult.Unavailable(exception.Message);
		}
	}



    // Gibt die wiederverwendbare Verbindung zur�ck.
    public Task<SQLiteAsyncConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return Task.FromResult(connection.Value);
	}



    // Werkseinstellungen: leert alle Tabellen. CreateTableAsync stellt sicher, dass die
    // Tabelle existiert, bevor DeleteAllAsync sie leert (falls sie noch nie angelegt wurde).
    public async Task ResetAllDataAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var db = connection.Value;

        await db.CreateTableAsync<UserSettingsEntity>();
        await db.DeleteAllAsync<UserSettingsEntity>();

        await db.CreateTableAsync<OnboardingSettingsEntity>();
        await db.DeleteAllAsync<OnboardingSettingsEntity>();

        await db.CreateTableAsync<GoalEntity>();
        await db.DeleteAllAsync<GoalEntity>();

        await db.CreateTableAsync<UserGoalEntity>();
        await db.DeleteAllAsync<UserGoalEntity>();

        await db.CreateTableAsync<global::TimeLimitRule>();
        await db.DeleteAllAsync<global::TimeLimitRule>();

        await db.CreateTableAsync<global::NotificationEvent>();
        await db.DeleteAllAsync<global::NotificationEvent>();
    }



    // Pr�ft ob die SQLite Datei erreichbar ist.
    public async Task EnsureDatabaseAccessibleAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Directory.CreateDirectory(GetDatabaseDirectory());
        await connection.Value.ExecuteScalarAsync<int>("SELECT 1");
    }



    // Baut den vollst�ndigen Pfad zur Datenbankdatei und erstellt die SQLite-Verbindung.
    private static SQLiteAsyncConnection CreateConnection()
	{
		var path = Path.Combine(GetDatabaseDirectory(), DatabaseFilename);
		Debug.WriteLine($"LocalDatabase: opening SQLite DB at: {path}");
		return new(path, OpenFlags);
    } 
		


	// Holt SQLite Datenbank Directory (Windows vs. Android, iOS)
	private static string GetDatabaseDirectory()
	{
		// Always use the platform app data directory at runtime so mobile and desktop use the same location.
		var appData = FileSystem.AppDataDirectory;

		// If we are running on Windows during development and there is a project-local database file,
		// copy it into AppData if the runtime DB does not exist yet. This makes it easier to work
		// with a single database file across development and devices.
#if WINDOWS
		try
		{
			var projectRoot = FindProjectRoot();
			if (projectRoot != null)
			{
				var projectDb = Path.Combine(projectRoot, "Infrastructure", "Database", DatabaseFilename);
				var destDb = Path.Combine(appData, DatabaseFilename);
				if (File.Exists(projectDb) && !File.Exists(destDb))
				{
					Directory.CreateDirectory(appData);
					File.Copy(projectDb, destDb);
					Debug.WriteLine($"LocalDatabase: copied project DB to AppData: {destDb}");
				}
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"LocalDatabase: failed to copy project DB: {ex.Message}");
		}
#endif

		return appData;
	}



    // Holt den Root Pfad des Projektes (nur f�r Windows relevant)
    private static string? FindProjectRoot()
	{
		var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);

		while (currentDirectory is not null)
		{
			if (currentDirectory.EnumerateFiles("*.csproj").Any())
			{
				return currentDirectory.FullName;
			}

			currentDirectory = currentDirectory.Parent;
		}

		return null;
	}
}
