using SQLite;

namespace ReturnToMonkee.Infrastructure.Persistence.Entities;

/// <summary>
/// Lokale Benutzereinstellungen — genau ein Datensatz in der DB.
/// Neue Felder hier ergänzen (z. B. #18: MovementIntervalMinutes).
/// CreateTableAsync ergänzt neue Spalten automatisch beim nächsten App-Start (intern via
/// MigrateTable), ohne Datenverlust.
/// </summary>
[Table("UserSettings")]
public class UserSettingsEntity
{
    /// <summary>
    /// Feste PK-ID des Singleton-Datensatzes.
    /// </summary>
    public static readonly Guid DefaultId = new("00000000-0000-0000-0000-000000000001");

    [PrimaryKey]
    public Guid Id { get; set; } = DefaultId;

    /// <summary>
    /// Schlafenszeit in Minuten seit Mitternacht (z. B. 22 * 60 = 1320 für 22:00 Uhr).
    /// Als Integer gespeichert, da TimeSpan kein nativer SQLite-Typ ist.
    /// Standard: 22:00 Uhr.
    /// </summary>
    public int SleepTimeMinutes { get; set; } = 22 * 60;

    // Platzhalter für #18: public int MovementIntervalMinutes { get; set; } = 60;

    /// <summary>
    /// Schlafenszeit als TimeSpan — nicht in DB gespeichert ([Ignore]).
    /// </summary>
    [Ignore]
    public TimeSpan SleepTime
    {
        get => TimeSpan.FromMinutes(SleepTimeMinutes);
        set => SleepTimeMinutes = (int)value.TotalMinutes;
    }
}
