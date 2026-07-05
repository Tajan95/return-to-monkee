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
        // Erster Tick bewusst verzoegert: beim App-Start ist die UI (XamlRoot) noch nicht
        // gerootet. Ein sofort faelliger Reminder wuerde sonst DisplayAlert/ContentDialog
        // ohne XamlRoot aufrufen und auf WinUI crashen (#59). Der NotificationAdapter
        // ueberspringt zusaetzlich, falls trotzdem noch keine Page bereitsteht.
        private readonly TimeSpan startupDelay = TimeSpan.FromSeconds(3);
        private DateTime lastReminderAt = DateTime.Now;
        private DateOnly? lastSleepReminderDate;
        // Effektiver Faelligkeitszeitpunkt (SleepTime - Vorlauf), zu dem zuletzt erinnert wurde.
        private TimeSpan? lastSleepReminderTime;
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
            timer = new Timer(async _ => await CheckDueAsync(), null, startupDelay, checkInterval);
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

        /// <summary>
        /// Loest den Bewegungs-Reminder manuell aus (Testtrigger) und setzt <see cref="lastReminderAt"/>
        /// zurueck, sodass der naechste automatische Reminder erst ein volles Intervall spaeter kommt.
        /// </summary>
        public async Task TriggerMovementReminderAsync(CancellationToken cancellationToken = default)
        {
            var intervalMinutes =
                await onboardingRepository.GetMovementReminderIntervalMinutesAsync(cancellationToken);

            var confirmed = await notificationAdapter.PromptAsync(
                "Bewegungs-Erinnerung",
                "Zeit für eine kurze Bewegungspause.",
                "Bestätigen",
                "Ignorieren",
                cancellationToken);

            lastReminderAt = DateTime.Now;
            await SaveMovementReminderEventAsync(confirmed, intervalMinutes);
        }

        /// <inheritdoc/>
        public async Task<DateTime> GetNextMovementReminderTimeAsync(CancellationToken cancellationToken = default)
        {
            var intervalMinutes =
                await onboardingRepository.GetMovementReminderIntervalMinutesAsync(cancellationToken);

            return lastReminderAt + TimeSpan.FromMinutes(intervalMinutes);
        }

        /// <inheritdoc/>
        public async Task<DateTime> GetNextSleepReminderTimeAsync(CancellationToken cancellationToken = default)
        {
            var settings = await userSettingsRepository.GetAsync(cancellationToken);
            var reminderTime = ComputeSleepReminderTime(settings.SleepTime, settings.SleepReminderLeadMinutes);

            var now = DateTime.Now;
            var todayOccurrence = now.Date.Add(reminderTime);
            return todayOccurrence >= now ? todayOccurrence : todayOccurrence.AddDays(1);
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
            if (!await onboardingRepository.GetMovementReminderEnabledAsync())
                return;

            var intervalMinutes =
                await onboardingRepository.GetMovementReminderIntervalMinutesAsync();
            var reminderInterval = TimeSpan.FromMinutes(intervalMinutes);

            if (now - lastReminderAt < reminderInterval)
                return;

            var confirmed = await notificationAdapter.PromptAsync(
                "Bewegungs-Erinnerung",
                "Zeit für eine kurze Bewegungspause.",
                "Bestätigen",
                "Ignorieren");

            lastReminderAt = now;
            await SaveMovementReminderEventAsync(confirmed, intervalMinutes);
        }

        private async Task CheckSleepReminderAsync(DateTime now)
        {
            var settings = await userSettingsRepository.GetAsync();

            if (!settings.SleepReminderEnabled)
                return;

            if (!IsSleepReminderDue(settings.SleepTime, settings.SleepReminderLeadMinutes, now, lastSleepReminderDate, lastSleepReminderTime))
                return;

            await SendSleepReminderAsync(settings.SleepTime);
            lastSleepReminderDate = DateOnly.FromDateTime(now);
            lastSleepReminderTime = ComputeSleepReminderTime(settings.SleepTime, settings.SleepReminderLeadMinutes);
        }

        /// <summary>
        /// Effektiver Reminder-Zeitpunkt: Schlafenszeit minus Vorlauf (FR-04 / §8.4). Bei einem
        /// Vorlauf, der ueber Mitternacht zuruecklaeuft, wird auf den Vortag umgeschlagen.
        /// </summary>
        internal static TimeSpan ComputeSleepReminderTime(TimeSpan sleepTime, int leadMinutes)
        {
            var reminderTime = sleepTime - TimeSpan.FromMinutes(leadMinutes);
            if (reminderTime < TimeSpan.Zero)
                reminderTime += TimeSpan.FromDays(1);
            return reminderTime;
        }

        /// <summary>
        /// Ermittelt, ob der Schlafenszeit-Reminder faellig ist: die aktuelle Uhrzeit hat den
        /// effektiven Reminder-Zeitpunkt (Schlafenszeit minus Vorlauf) erreicht oder
        /// ueberschritten, und entweder wurde er an diesem Kalendertag noch nicht fuer genau
        /// diesen Zeitpunkt ausgeloest, oder Schlafenszeit/Vorlauf wurden seit dem letzten
        /// Ausloesen geaendert (z.B. weil der Reminder beim App-Start sofort fuer einen bereits
        /// verstrichenen alten Zeitpunkt feuerte, bevor der Nutzer ihn auf einen spaeteren,
        /// ebenfalls schon erreichten Wert aktualisiert hat).
        /// </summary>
        internal static bool IsSleepReminderDue(
            TimeSpan sleepTime,
            int leadMinutes,
            DateTime now,
            DateOnly? lastTriggeredDate,
            TimeSpan? lastTriggeredReminderTime)
        {
            var reminderTime = ComputeSleepReminderTime(sleepTime, leadMinutes);
            var today = DateOnly.FromDateTime(now);
            var alreadyTriggeredForThisReminderTimeToday =
                lastTriggeredDate == today && lastTriggeredReminderTime == reminderTime;
            return now.TimeOfDay >= reminderTime && !alreadyTriggeredForThisReminderTimeToday;
        }

        private async Task SendSleepReminderAsync(TimeSpan sleepTime, CancellationToken cancellationToken = default)
        {
            var confirmed = await notificationAdapter.PromptAsync(
                "Schlafenszeit-Erinnerung",
                "Es ist Zeit, schlafen zu gehen.",
                "Bestätigen",
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
                        ? "Schlafenszeit bestätigt"
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
                        ? "Bewegungspause bestätigt"
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
