namespace ReturnToMonkee.Infrastructure.Persistence;

/// <summary>
/// Beschreibt den aktuellen Zustand der lokalen Datenbank so, dass die UI ihn direkt anzeigen kann.
/// </summary>
public sealed record DatabaseHealthResult(bool IsReady, string Message, string? Details)
{
	/// <summary>
	/// Erstellt das Ergebnis fuer eine erfolgreiche Datenbankpruefung.
	/// </summary>
	public static DatabaseHealthResult Ready() =>
		new(true, "DB bereit", null);

	/// <summary>
	/// Erstellt das Ergebnis fuer eine fehlgeschlagene Datenbankpruefung.
	/// </summary>
	public static DatabaseHealthResult Unavailable(string details) =>
		new(false, "DB nicht verfügbar", details);
}
