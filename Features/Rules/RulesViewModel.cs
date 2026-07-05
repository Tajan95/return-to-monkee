using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ReturnToMonkee.Infrastructure.Persistence;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.Maui.Controls;
using ReturnToMonkee.Infrastructure.Notifications;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;

namespace ReturnToMonkee.Features.Rules
{
    public class RulesViewModel : INotifyPropertyChanged
    {
        public const string TimeLimitInterventionTitle = "Zeitlimit erreicht";

        private readonly ILocalDatabase localDatabase;
        private readonly INotificationAdapter notificationAdapter;
        private readonly INotificationEventRepository notificationEventRepository;
        private ObservableCollection<RuleItem> rules = new();

        public ObservableCollection<RuleItem> Rules
        {
            get => rules;
            set { rules = value; OnPropertyChanged(); }
        }

        public RulesViewModel(
            ILocalDatabase localDatabase,
            INotificationAdapter notificationAdapter,
            INotificationEventRepository notificationEventRepository)
        {
            this.localDatabase = localDatabase;
            this.notificationAdapter = notificationAdapter;
            this.notificationEventRepository = notificationEventRepository;
        }

        public async Task LoadRulesAsync()
        {
            try
            {
                var conn = await localDatabase.GetConnectionAsync();
                // Regel-Tab verwaltet nur Zeitlimit-Regeln (FR-02). Bewegungs-/Schlaf-Reminder
                // haben eigene Pages; der frueher hier gelistete Reminder war ein toter Toggle.
                await conn.CreateTableAsync<TimeLimitRule>();

                var timeLimitRules = await conn.Table<TimeLimitRule>().ToListAsync();

                var list = new List<RuleItem>();

                foreach (var r in timeLimitRules)
                {
                    list.Add(new RuleItem
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Description = r.Description,
                        IsEnabled = r.IsEnabled,
                        Type = "TimeLimitRule",
                        TimeLimitMinutes = r.TimeLimitMinutes,
                        TargetApplication = r.TargetApplication,
                        StatusMessage = "Bereit zum Testen."
                    });
                }

                Rules = new ObservableCollection<RuleItem>(list);
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Regeln konnten nicht geladen werden: {ex.Message}");
            }
        }

        public async Task ToggleRuleAsync(RuleItem item, bool newState)
        {
            if (item == null) return;

            try
            {
                var conn = await localDatabase.GetConnectionAsync();

                if (item.Type == "TimeLimitRule")
                {
                    await conn.ExecuteAsync("UPDATE TimeLimitRules SET IsEnabled = ? WHERE Id = ?", newState ? 1 : 0, item.Id.ToString());
                }

                // Update local state
                var existing = Rules.FirstOrDefault(r => r.Id == item.Id && r.Type == item.Type);
                if (existing != null)
                {
                    existing.IsEnabled = newState;
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Status konnte nicht gespeichert werden: {ex.Message}");
            }
        }

        public async Task TestTimeLimitRuleAsync(RuleItem item, CancellationToken cancellationToken = default)
        {
            if (item == null || !item.IsTimeLimitRule || item.IsTesting)
            {
                return;
            }

            try
            {
                item.IsTesting = true;
                item.IsSoftInterventionVisible = true;
                item.StatusMessage = "Wird gespeichert…";

                var message = BuildTimeLimitInterventionMessage(item);

                await notificationAdapter.SendAsync(
                    TimeLimitInterventionTitle,
                    message,
                    cancellationToken);

                var notificationEvent = new NotificationEvent
                {
                    Id = Guid.NewGuid(),
                    Time = DateTimeOffset.UtcNow,
                    Title = TimeLimitInterventionTitle,
                    Message = message,
                    AppReference = $"app://rules/time-limit/{item.Id}/exceeded"
                };

                await notificationEventRepository.SaveAsync(notificationEvent, cancellationToken);
                item.InterventionMessage = message;
                item.StatusMessage = "Überschreitung gespeichert.";
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Intervention konnte nicht getestet werden: {ex.Message}");
            }
            finally
            {
                item.IsTesting = false;
            }
        }

        private static string BuildTimeLimitInterventionMessage(RuleItem item)
        {
            var target = string.IsNullOrWhiteSpace(item.TargetApplication)
                ? item.Title
                : item.TargetApplication;

            return $"Zeitlimit für {target} erreicht ({item.TimeLimitMinutes} Min). Willst du wirklich weitermachen?";
        }

        private static Task ShowErrorAsync(string message)
        {
            return MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var page = Application.Current?.Windows.FirstOrDefault()?.Page;
                if (page != null)
                {
                    await page.DisplayAlertAsync("Fehler", message, "OK");
                }
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class RuleItem : INotifyPropertyChanged
        {
            private bool isEnabled;
            private bool isSoftInterventionVisible;
            private bool isTesting;
            private string statusMessage = string.Empty;
            private string interventionMessage = string.Empty;

            public Guid Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public int TimeLimitMinutes { get; set; }
            public string TargetApplication { get; set; } = string.Empty;
            public bool IsTimeLimitRule => Type == "TimeLimitRule";

            // Kompaktes Limit-Badge fuer die Karte, z. B. "30 Min/Tag".
            public string LimitBadge => $"{TimeLimitMinutes} Min/Tag";

            public bool IsEnabled
            {
                get => isEnabled;
                set { isEnabled = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabled))); }
            }

            public bool IsSoftInterventionVisible
            {
                get => isSoftInterventionVisible;
                set { isSoftInterventionVisible = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSoftInterventionVisible))); }
            }

            public bool IsTesting
            {
                get => isTesting;
                set { isTesting = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTesting))); }
            }

            public string StatusMessage
            {
                get => statusMessage;
                set { statusMessage = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusMessage))); }
            }

            public string InterventionMessage
            {
                get => interventionMessage;
                set { interventionMessage = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InterventionMessage))); }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
        }
    }
}
