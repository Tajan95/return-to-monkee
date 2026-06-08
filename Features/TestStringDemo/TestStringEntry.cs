using SQLite;

namespace ReturnToMonkee.Features.TestStringDemo;

/// <summary>
/// Sehr einfache Demo-Tabelle fuer genau einen gespeicherten String.
/// </summary>
public sealed class TestStringEntry
{
	/// <summary>
	/// Feste ID, damit die Demo immer nur einen Datensatz verwaltet.
	/// </summary>
	[PrimaryKey]
	public int Id { get; set; }

	/// <summary>
	/// Der gespeicherte Demo-Text.
	/// </summary>
	public string Value { get; set; } = string.Empty;
}
