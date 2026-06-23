using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;

namespace ReturnToMonkee.Services
{
    public class ReminderService : IReminderService, IDisposable
    {
        private readonly IBewegungsErinnerungsRepository repository;
        private Timer? timer;
        private readonly TimeSpan checkInterval = TimeSpan.FromSeconds(5);
        private bool disposedValue;

        public ReminderService(IBewegungsErinnerungsRepository repository)
        {
            this.repository = repository;
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
            try
            {
                var all = await repository.GetAllAsync();
                var now = DateTime.Now;

                foreach (var e in all)
                {
                    if (!e.IstAktiv) continue;
                    if (e.IstBestaetigt) continue; // already confirmed

                    // only for configured weekday
                    if (e.Wochentag != now.DayOfWeek)
                        continue;

                    var remindTimeToday = DateTime.Today.Add(e.Erinnerungszeitpunkt);
                    // time reached within last check interval
                    if (remindTimeToday <= now && (now - remindTimeToday) < checkInterval)
                    {
                        // prompt user on main thread
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            var page = Application.Current?.MainPage;
                            if (page == null) return;
                            var time = e.Erinnerungszeitpunkt.ToString("hh\\:mm");
                            var result = await page.DisplayAlert("Bewegungs-Erinnerung", $"{e.Titel}\n{e.Wochentag} {time}\nBestätigen?", "Bestätigen", "Ablehnen");
                            if (result)
                            {
                                e.IstBestaetigt = true;
                                await repository.SaveAsync(e);
                            }
                            else
                            {
                                // consider declining as deactivating
                                e.IstAktiv = false;
                                await repository.SaveAsync(e);
                            }
                        });
                    }
                }
            }
            catch
            {
                // ignore exceptions in background check
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
