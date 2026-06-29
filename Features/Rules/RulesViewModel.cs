using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ReturnToMonkee.Infrastructure.Persistence;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.Maui.Controls;

namespace ReturnToMonkee.Features.Rules
{
    public class RulesViewModel : INotifyPropertyChanged
    {
        private readonly ILocalDatabase localDatabase;
        private ObservableCollection<RuleItem> rules = new();

        public ObservableCollection<RuleItem> Rules
        {
            get => rules;
            set { rules = value; OnPropertyChanged(); }
        }

        public RulesViewModel(ILocalDatabase localDatabase)
        {
            this.localDatabase = localDatabase;
        }

        public async Task LoadRulesAsync()
        {
            try
            {
                var conn = await localDatabase.GetConnectionAsync();
                // Ensure tables exist
                await conn.CreateTableAsync<TimeLimitRule>();
                await conn.CreateTableAsync<Reminder>();

                var timeLimitRules = await conn.Table<TimeLimitRule>().ToListAsync();
                var reminders = await conn.Table<Reminder>().ToListAsync();

                var list = new List<RuleItem>();

                foreach (var r in timeLimitRules)
                {
                    list.Add(new RuleItem
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Description = r.Description,
                        IsEnabled = r.IsEnabled,
                        Type = "TimeLimitRule"
                    });
                }

                foreach (var r in reminders)
                {
                    list.Add(new RuleItem
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Description = string.Empty,
                        IsEnabled = r.IsEnabled,
                        Type = "Reminder"
                    });
                }

                Rules = new ObservableCollection<RuleItem>(list);
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current?.MainPage?.DisplayAlert("Fehler", $"Regeln konnten nicht geladen werden: {ex.Message}", "OK");
                });
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
                else if (item.Type == "Reminder")
                {
                    await conn.ExecuteAsync("UPDATE Reminders SET IsEnabled = ? WHERE Id = ?", newState ? 1 : 0, item.Id.ToString());
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
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current?.MainPage?.DisplayAlert("Fehler", $"Status konnte nicht gespeichert werden: {ex.Message}", "OK");
                });
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class RuleItem : INotifyPropertyChanged
        {
            private bool isEnabled;

            public Guid Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;

            public bool IsEnabled
            {
                get => isEnabled;
                set { isEnabled = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabled))); }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
        }
    }
}
