namespace ReturnToMonkee.Infrastructure.Persistence;

/// <summary>
/// Kapselt den Zugriff auf die lokale SQLite-Datenbank fuer den Rest der App.
/// </summary>
public interface ILocalDatabase
{
	/// <summary>
	/// Oeffnet oder erstellt die lokale Datenbankdatei.
	/// </summary>
	Task InitializeAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Prueft, ob die lokale Datenbank aktuell erreichbar ist.
	/// </summary>
	Task<DatabaseHealthResult> CheckHealthAsync(CancellationToken cancellationToken = default);
}
