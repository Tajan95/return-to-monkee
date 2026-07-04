using System;
using System.Threading;
using System.Threading.Tasks;
using ReturnToMonkee.Infrastructure.Notifications;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;

namespace ReturnToMonkee.Services
{
    public class ReminderService : IReminderService, IDisposable
    {
        private readonly IOnboardingRepository onboardingRepository;
        private readonly INotificationEventRepository notificationEventRepository;
        private readonly INotificationAdapter notificationAdapter;
        private readonly IUserSettingsRepository userSettingsRepository;
        private Timer? timer;
        private readonly TimeSpan checkInterval = TimeSpan.FromMinutes(1);
        private DateTime lastReminderAt = DateTime.Now;
        private DateOnly? lastSleepReminderDate;
        private TimeSpan? lastSleepReminderSleepTime;
        private bool isChecking;
        private bool disposedValue;

        public ReminderService(
            IOnboardingRepository onboardingRepository,
            INotificationEventRepository notificationEventRepository,
            INotificationAdapter notificationAdapter,
            IUserSettingsRepository userSettingsRepository)
        {
            this.onboardingRepository = onboardingRepository;
            this.notificationEventRepository = notificationEventRepository;
            this.notificationAdapter = notificationAdapter;
            this.userSettingsRepository = userSettingsRepository;
        }

        public Task StartAsync()
        {
            // Start timer that checks every checkInterval
            timer = new Timer(async _ => await CheckDueAsync(), null, TimeSpan.Zero, checkInterval);
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            timer?.Change(Timeout.Infinite, Timeout.Infinite);
            timer?.Dispose();
            timer = null;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Loest den Schlafenszeit-Reminder manuell aus (Testtrigger), unabhaengig von der
        /// konfigurierten Schlafenszeit und der Einmal-pro-Tag-Sperre.
        /// </summary>
        public async Task TriggerSleepReminderAsync(CancellationToken cancellationToken = default)
        {
            var settings = await userSettingsRepository.GetAsync(cancellationToken);
            await SendSleepReminderAsync(settings.SleepTime, cancellationToken);
        }

        private Task CheckDueAsync() => CheckDueAsync(DateTime.Now);

        internal async Task CheckDueAsync(DateTime now)
        {
            if (isChecking)
                return;

            isChecking = true;

            try
            {
                await CheckMovementReminderAsync(now);
                await CheckSleepReminderAsync(now);
            }
            catch
            {
                // ignore exceptions in background check
            }
            finally
            {
                isChecking = false;
            }
        }

        private async Task CheckMovementReminderAsync(DateTime now)
        {
            var intervalMinutes =
                await onboardingRepository.GetMovementReminderIntervalMinutesAsync();
            var reminderInterval = TimeSpan.FromMinutes(intervalMinutes);

            if (now - lastReminderAt < reminderInterval)
                return;

            var confirmed = await notificationAdapter.PromptAsync(
                "Bewegungs-Erinnerung",
                "Zeit fuer eine kurze Bewegungspause.",
                "Bestaetigen",
                "Ignorieren");

            lastReminderAt = now;
            await SaveMovementReminderEventAsync(confirmed, intervalMinutes);
        }

        private async Task CheckSleepReminderAsync(DateTime now)
        {
            var settings = await userSettingsRepository.GetAsync();

            if (!IsSleepReminderDue(settings.SleepTime, now, lastSleepReminderDate, lastSleepReminderSleepTime))
                return;

            await SendSleepReminderAsync(settings.SleepTime);
            lastSleepReminderDate = DateOnly.FromDateTime(now);
            lastSleepReminderSleepTime = settings.SleepTime;
        }

        /// <summary>
        /// Ermittelt, ob der Schlafenszeit-Reminder faellig ist: die aktuelle Uhrzeit hat die
        /// konfigurierte Schlafenszeit erreicht oder ueberschritten, und entweder wurde er an
        /// diesem Kalendertag noch nicht ausgeloest, oder die Schlafenszeit wurde seit dem
        /// letzten Ausloesen geaendert (z.B. weil der Reminder beim App-Start sofort fuer eine
        /// bereits verstrichene alte Schlafenszeit feuerte, bevor der Nutzer sie auf einen
        /// spaeteren, ebenfalls schon erreichten Wert aktualisiert hat).
        /// </summary>
        internal static bool IsSleepReminderDue(
            TimeSpan sleepTime,
            DateTime now,
            DateOnly? lastTriggeredDate,
            TimeSpan? lastTriggeredSleepTime)
        {
            var today = DateOnly.FromDateTime(now);
            var alreadyTriggeredForThisSleepTimeToday = lastTriggeredDate == today && lastTriggeredSleepTime == sleepTime;
            return now.TimeOfDay >= sleepTime && !alreadyTriggeredForThisSleepTimeToday;
        }

        private async Task SendSleepReminderAsync(TimeSpan sleepTime, CancellationToken cancellationToken = default)
        {
            var confirmed = await notificationAdapter.PromptAsync(
                "Schlafenszeit-Erinnerung",
                "Es ist Zeit, schlafen zu gehen.",
                "Bestaetigen",
                "Ignorieren",
                cancellationToken);

            await SaveSleepReminderEventAsync(confirmed, sleepTime, cancellationToken);
        }

        private Task SaveSleepReminderEventAsync(
            bool confirmed,
            TimeSpan sleepTime,
            CancellationToken cancellationToken)
        {
            var status = confirmed ? "confirmed" : "ignored";

            return notificationEventRepository.SaveAsync(
                new NotificationEvent
                {
                    Id = Guid.NewGuid(),
                    Time = DateTimeOffset.UtcNow,
                    Title = confirmed
                        ? "Schlafenszeit bestaetigt"
                        : "Schlafenszeit ignoriert",
                    Message = $"Schlafenszeit-Reminder um {sleepTime:hh\\:mm} Uhr wurde {status}.",
                    AppReference = $"return-to-monkee://sleep-reminder/{status}"
                },
                cancellationToken);
        }

        private Task SaveMovementReminderEventAsync(
            bool confirmed,
            int intervalMinutes)
        {
            var status = confirmed ? "confirmed" : "ignored";

            return notificationEventRepository.SaveAsync(
                new NotificationEvent
                {
                    Id = Guid.NewGuid(),
                    Time = DateTimeOffset.UtcNow,
                    Title = confirmed
                        ? "Bewegungspause bestaetigt"
                        : "Bewegungspause ignoriert",
                    Message = $"Bewegungs-Reminder nach {intervalMinutes} Minuten wurde {status}.",
                    AppReference = $"return-to-monkee://movement-reminder/{status}"
                });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    timer?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
