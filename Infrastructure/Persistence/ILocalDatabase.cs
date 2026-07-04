using SQLite;

namespace ReturnToMonkee.Infrastructure.Persistence;

/// <summary>
/// Kapselt den Zugriff auf die lokale SQLite-Datenbank fuer den Rest der App.
/// </summary>
public interface ILocalDatabase
{
	/// <summary>
	/// Prueft, ob die lokale Datenbankdatei erreichbar ist.
	/// </summary>
	Task EnsureDatabaseAccessibleAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Prueft, ob die lokale Datenbank aktuell erreichbar ist.
	/// </summary>
	Task<DatabaseHealthResult> CheckHealthAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gibt eine initialisierte SQLite-Verbindung fuer einfache Repository-Klassen zurueck.
	/// </summary>
	Task<SQLiteAsyncConnection> GetConnectionAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Setzt die App auf Werkseinstellungen zurueck: loescht alle Datensaetze aus allen
	/// Tabellen (Ziele, Regeln, Einstellungen, Onboarding-Status, Verlauf). Nicht umkehrbar.
	/// </summary>
	Task ResetAllDataAsync(CancellationToken cancellationToken = default);
}
