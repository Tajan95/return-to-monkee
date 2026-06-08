namespace ReturnToMonkee.Infrastructure.Persistence;

public interface ILocalDatabase
{
	Task InitializeAsync(CancellationToken cancellationToken = default);

	Task<DatabaseHealthResult> CheckHealthAsync(CancellationToken cancellationToken = default);
}
