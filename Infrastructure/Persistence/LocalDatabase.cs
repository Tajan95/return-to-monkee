using SQLite;

namespace ReturnToMonkee.Infrastructure.Persistence;

public sealed class LocalDatabase : ILocalDatabase
{
	private const string DatabaseFilename = "return_to_monkee.db3";

	// Öffnet die Datenbank schreibend und legt sie an, falls sie noch nicht existiert.
	private static readonly SQLiteOpenFlags OpenFlags =
		SQLiteOpenFlags.ReadWrite |
		SQLiteOpenFlags.Create |
		SQLiteOpenFlags.SharedCache;

	// Die Verbindung wird erst erstellt, wenn sie zum ersten Mal gebraucht wird.
	private readonly Lazy<SQLiteAsyncConnection> connection = new(CreateConnection);







    // Führt eine einfache Abfrage aus und liefert einen UI - tauglichen Status zurück.
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



    // Initialisiert die Datenbank und gibt danach die wiederverwendbare Verbindung zurück.
    public async Task<SQLiteAsyncConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
	{
		await EnsureDatabaseAccessibleAsync(cancellationToken);
		return connection.Value;
	}



    // Prüft ob die SQLite Datei erreichbar ist.
    public async Task EnsureDatabaseAccessibleAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Directory.CreateDirectory(GetDatabaseDirectory());
        await connection.Value.ExecuteScalarAsync<int>("SELECT 1");
    }



    // Baut den vollständigen Pfad zur Datenbankdatei und erstellt die SQLite-Verbindung.
    private static SQLiteAsyncConnection CreateConnection()
	{
		return new(Path.Combine(GetDatabaseDirectory(), DatabaseFilename), OpenFlags);
    } 
		


	// Holt SQLite Datenbank Directory (Windows vs. Android, iOS)
	private static string GetDatabaseDirectory()
	{
		#if WINDOWS
		var projectRoot = FindProjectRoot();
		if (projectRoot != null)
		{
			return Path.Combine(projectRoot, "Infrastructure", "Database");
		}
		#endif

		return FileSystem.AppDataDirectory;
	}



    // Holt den Root Pfad des Projektes (nur für Windows relevant)
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
