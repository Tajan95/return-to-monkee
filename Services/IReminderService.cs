using System.Threading;
using System.Threading.Tasks;

namespace ReturnToMonkee.Services
{
    public interface IReminderService
    {
        Task StartAsync();
        Task StopAsync();

        /// <summary>
        /// Loest den Schlafenszeit-Reminder manuell aus (Testtrigger), unabhaengig von der
        /// konfigurierten Schlafenszeit und ohne die Einmal-pro-Tag-Sperre zu pruefen.
        /// </summary>
        Task TriggerSleepReminderAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Loest den Bewegungs-Reminder manuell aus (Testtrigger) und setzt den Intervall-Zaehler
        /// zurueck, sodass der naechste automatische Reminder erst ein volles Intervall spaeter kommt.
        /// </summary>
        Task TriggerMovementReminderAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Liefert den voraussichtlichen Zeitpunkt des naechsten automatischen Bewegungs-Reminders
        /// (letzter Reminder bzw. App-Start + konfiguriertes Intervall).
        /// </summary>
        Task<DateTime> GetNextMovementReminderTimeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Liefert den naechsten Zeitpunkt des automatischen Schlafenszeit-Reminders unter
        /// Beruecksichtigung des Vorlaufs (Schlafenszeit minus Vorlauf).
        /// </summary>
        Task<DateTime> GetNextSleepReminderTimeAsync(CancellationToken cancellationToken = default);
    }
}
