using SQLite;

namespace ReturnToMonkee.Infrastructure.Persistence;

public sealed class LocalDatabase : ILocalDatabase
{
	// Name der SQLite-Datei im lokalen App-Datenordner.
	private const string DatabaseFilename = "return_to_monkee.db3";

	// Oeffnet die Datenbank schreibend und legt sie an, falls sie noch nicht existiert.
	private static readonly SQLiteOpenFlags OpenFlags =
		SQLiteOpenFlags.ReadWrite |
		SQLiteOpenFlags.Create |
		SQLiteOpenFlags.SharedCache;

	// Die Verbindung wird erst erstellt, wenn sie zum ersten Mal gebraucht wird.
	private readonly Lazy<SQLiteAsyncConnection> connection = new(CreateConnection);

	/// <summary>
	/// Stellt sicher, dass der App-Datenordner und die SQLite-Verbindung vorhanden sind.
	/// </summary>
	public async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		Directory.CreateDirectory(FileSystem.AppDataDirectory);
		await connection.Value.ExecuteScalarAsync<int>("SELECT 1");
	}

	/// <summary>
	/// Fuehrt eine einfache Abfrage aus und liefert einen UI-tauglichen Status zurueck.
	/// </summary>
	public async Task<DatabaseHealthResult> CheckHealthAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			await InitializeAsync(cancellationToken);
			return DatabaseHealthResult.Ready();
		}
		catch (Exception exception)
		{
			return DatabaseHealthResult.Unavailable(exception.Message);
		}
	}

	/// <summary>
	/// Initialisiert die Datenbank und gibt danach die wiederverwendbare Verbindung zurueck.
	/// </summary>
	public async Task<SQLiteAsyncConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
	{
		await InitializeAsync(cancellationToken);
		return connection.Value;
	}

	// Baut den vollstaendigen Pfad zur Datenbankdatei und erstellt die SQLite-Verbindung.
	private static SQLiteAsyncConnection CreateConnection() =>
		new(Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename), OpenFlags);
}
