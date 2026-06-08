using SQLite;

namespace ReturnToMonkee.Infrastructure.Persistence;

public sealed class LocalDatabase : ILocalDatabase
{
	private const string DatabaseFilename = "return_to_monkee.db3";

	private static readonly SQLiteOpenFlags OpenFlags =
		SQLiteOpenFlags.ReadWrite |
		SQLiteOpenFlags.Create |
		SQLiteOpenFlags.SharedCache;

	private readonly Lazy<SQLiteAsyncConnection> connection = new(CreateConnection);

	public async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		Directory.CreateDirectory(FileSystem.AppDataDirectory);
		await connection.Value.ExecuteScalarAsync<int>("SELECT 1");
	}

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

	private static SQLiteAsyncConnection CreateConnection() =>
		new(Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename), OpenFlags);
}
