namespace ReturnToMonkee.Features.Settings;

/// <summary>
/// Persistierter Zugriff auf Benutzereinstellungen.
/// </summary>
public interface IUserSettingsRepository
{
    /// <summary>
    /// Gibt die aktuellen Einstellungen zurück.
    /// Legt Standardwerte an, wenn noch kein Datensatz existiert.
    /// </summary>
    Task<UserSettings> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Speichert die Einstellungen (Insert oder Replace des Singleton-Datensatzes).
    /// </summary>
    Task SaveAsync(UserSettings settings, CancellationToken cancellationToken = default);
}
