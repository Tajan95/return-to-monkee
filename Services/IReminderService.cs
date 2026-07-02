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
    }
}
