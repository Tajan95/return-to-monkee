using ReturnToMonkee.Infrastructure.Persistence.Entities;

namespace ReturnToMonkee.Infrastructure.Persistence.Repositories;

/// <summary>
/// Persistierter Zugriff auf Benutzereinstellungen.
/// </summary>
public interface IUserSettingsRepository
{
    /// <summary>
    /// Gibt die aktuellen Einstellungen zurück.
    /// Legt Standardwerte an, wenn noch kein Datensatz existiert.
    /// </summary>
    Task<UserSettingsEntity> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Speichert die Einstellungen (Insert oder Replace des Singleton-Datensatzes).
    /// </summary>
    Task SaveAsync(UserSettingsEntity settings, CancellationToken cancellationToken = default);
}
