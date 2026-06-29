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
        private readonly INotificationAdapter notificationAdapter;
        private Timer? timer;
        private readonly TimeSpan checkInterval = TimeSpan.FromMinutes(1);
        private DateTime lastReminderAt = DateTime.Now;
        private bool isChecking;
        private bool disposedValue;

        public ReminderService(
            IOnboardingRepository onboardingRepository,
            INotificationAdapter notificationAdapter)
        {
            this.onboardingRepository = onboardingRepository;
            this.notificationAdapter = notificationAdapter;
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

        private async Task CheckDueAsync()
        {
            if (isChecking)
                return;

            isChecking = true;

            try
            {
                var now = DateTime.Now;
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

                lastReminderAt = DateTime.Now;

                if (!confirmed)
                {
                    return;
                }
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
