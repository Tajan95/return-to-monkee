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

    /// <summary>
    /// Aktiviert den automatischen Schlafenszeit-Reminder. Testtrigger duerfen unabhaengig
    /// davon weiterhin manuell ausgeloest werden.
    /// </summary>
    public bool SleepReminderEnabled { get; set; } = true;

    // Platzhalter für #18: public int MovementIntervalMinutes { get; set; } = 60;

    /// <summary>
    /// Entwickler-Option: Onboarding bei jedem App-Start erneut anzeigen, unabhängig vom
    /// Abschlussstatus. Ersetzt den bisherigen hartkodierten <c>AppSettings.ForceShowOnboarding</c>-Flag
    /// durch eine persistierte, in den Einstellungen umschaltbare Option. Standard: true (aktuelles
    /// Entwicklungsverhalten bleibt unverändert, ist aber jetzt abschaltbar).
    /// </summary>
    public bool ShowOnboardingOnStartup { get; set; } = true;

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
