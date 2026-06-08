using ReturnToMonkee.Infrastructure.Persistence;

namespace ReturnToMonkee.Features.TestStringDemo;

/// <summary>
/// Speichert und liest den Demo-String ueber die gemeinsame SQLite-Verbindung.
/// </summary>
public sealed class TestStringRepository : ITestStringRepository
{
	private const int SingleDemoEntryId = 1;

	private readonly ILocalDatabase localDatabase;

	public TestStringRepository(ILocalDatabase localDatabase)
	{
		this.localDatabase = localDatabase;
	}

	/// <summary>
	/// Erstellt die Demo-Tabelle, falls sie fehlt, und liest danach den einzigen Datensatz.
	/// </summary>
	public async Task<string?> LoadAsync()
	{
		var connection = await GetReadyConnectionAsync();
		var entry = await connection.FindAsync<TestStringEntry>(SingleDemoEntryId);

		return entry?.Value;
	}

	/// <summary>
	/// Nutzt InsertOrReplace: ohne Datensatz ist es Create, mit Datensatz ist es Update.
	/// </summary>
	public async Task SaveAsync(string value)
	{
		var connection = await GetReadyConnectionAsync();
		var entry = new TestStringEntry
		{
			Id = SingleDemoEntryId,
			Value = value,
		};

		await connection.InsertOrReplaceAsync(entry);
	}

	/// <summary>
	/// Loescht den einzigen Demo-Datensatz anhand seiner festen ID.
	/// </summary>
	public async Task DeleteAsync()
	{
		var connection = await GetReadyConnectionAsync();
		await connection.DeleteAsync<TestStringEntry>(SingleDemoEntryId);
	}

	private async Task<SQLite.SQLiteAsyncConnection> GetReadyConnectionAsync()
	{
		var connection = await localDatabase.GetConnectionAsync();
		await connection.CreateTableAsync<TestStringEntry>();

		return connection;
	}
}
