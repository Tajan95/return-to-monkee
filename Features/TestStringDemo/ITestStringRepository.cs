namespace ReturnToMonkee.Features.TestStringDemo;

/// <summary>
/// Kapselt die vier CRUD-Aktionen fuer die einfache TestString-Demo.
/// </summary>
public interface ITestStringRepository
{
	/// <summary>
	/// Liest den aktuell gespeicherten Demo-String aus der Datenbank.
	/// </summary>
	Task<string?> LoadAsync();

	/// <summary>
	/// Speichert einen neuen Demo-String oder aktualisiert den bestehenden Datensatz.
	/// </summary>
	Task SaveAsync(string value);

	/// <summary>
	/// Loescht den Demo-String aus der Datenbank.
	/// </summary>
	Task DeleteAsync();
}
