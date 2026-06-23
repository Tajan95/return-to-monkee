using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ReturnToMonkee.Infrastructure.Persistence;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using ReturnToMonkee.Infrastructure.Persistence.Entities;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;

namespace ReturnToMonkee.Features.BewegungsErinnerung
{
    public class BewegungsErinnerungViewModel : INotifyPropertyChanged
    {
        private bool isEditMode;
        private bool _isNewEntry;
        private readonly IBewegungsErinnerungsRepository repository;
        private ObservableCollection<BewegungsErinnerungsEntity> eintraege = new();
        private BewegungsErinnerungsEntity? selectedEntry;
        private string? titel;
        private string wochentagName = "Montag";
        private TimeSpan erinnerungszeitpunkt = new TimeSpan(9, 0, 0);
        private bool istAktiv = true;
        private bool istBestaetigt = false;
        private string? beschreibung = string.Empty;
        private int nextId = 1;
        public List<string> Wochentage { get; } = new()
        {
            "Montag", "Dienstag", "Mittwoch", "Donnerstag", "Freitag", "Samstag", "Sonntag"
        };

        public ICommand NewEntryCommand { get; }
        public ICommand SelectEntryCommand { get; }

        public bool IsEditMode
        {
            get => isEditMode;
            set => SetProperty(ref isEditMode, value);
        }

        public bool IsNewEntry
        {
            get => _isNewEntry;
            set
            {
                _isNewEntry = value;
                OnPropertyChanged(); // Oder wie auch immer dein PropertyChanged aufgerufen wird
            }
        }

        private readonly Dictionary<DayOfWeek, string> enumToGerman = new()
        {
            { DayOfWeek.Monday, "Montag" }, { DayOfWeek.Tuesday, "Dienstag" },
            { DayOfWeek.Wednesday, "Mittwoch" }, { DayOfWeek.Thursday, "Donnerstag" },
            { DayOfWeek.Friday, "Freitag" }, { DayOfWeek.Saturday, "Samstag" }, { DayOfWeek.Sunday, "Sonntag" }
        };

        public ObservableCollection<BewegungsErinnerungsEntity> Eintraege
        {
            get => eintraege;
            set => SetProperty(ref eintraege, value);
        }

        public BewegungsErinnerungsEntity? SelectedEntry
        {
            get => selectedEntry;
            set
            {
                SetProperty(ref selectedEntry, value);
                if (value != null)
                {
                    LoadFormFromEntry(value);
                }
            }
        }

        public string? Titel
        {
            get => titel;
            set => SetProperty(ref titel, value);
        }

        public string WochentagName
        {
            get => wochentagName;
            set => SetProperty(ref wochentagName, value);
        }

        public TimeSpan Erinnerungszeitpunkt
        {
            get => erinnerungszeitpunkt;
            set => SetProperty(ref erinnerungszeitpunkt, value);
        }

        public bool IstAktiv
        {
            get => istAktiv;
            set => SetProperty(ref istAktiv, value);
        }

        public bool IstBestaetigt
        {
            get => istBestaetigt;
            set => SetProperty(ref istBestaetigt, value);
        }

        public string? Beschreibung
        {
            get => beschreibung;
            set => SetProperty(ref beschreibung, value);
        }

        public int NextId
        {
            get => nextId;
            set => SetProperty(ref nextId, value);
        }

        public BewegungsErinnerungViewModel(IBewegungsErinnerungsRepository repository)
        {
            this.repository = repository;

            SelectEntryCommand = new Command<BewegungsErinnerungsEntity>(e =>
            {
                if (e == null) return;

                if (SelectedEntry != null)
                    SelectedEntry.IsSelected = false;

                e.IsSelected = true;
                SelectedEntry = e;

                // Sobald geladen, wechseln wir in den Edit-Mode
                IsEditMode = true;
            });

            // "Neu" Button leert das Formular und öffnet die Ansicht
            NewEntryCommand = new Command(() =>
            {
                ClearForm();
                IsEditMode = true;
            });
        }

        public async Task LoadEntriesAsync()
        {
            try
            {
                // Stelle sicher, dass die Tabelle existiert
                await repository.InitializeAsync();

                var all = await repository.GetAllAsync();
                Eintraege = new System.Collections.ObjectModel.ObservableCollection<BewegungsErinnerungsEntity>(all);
                // ensure no selection state remains
                foreach (var it in Eintraege)
                    it.IsSelected = false;
                System.Diagnostics.Debug.WriteLine($"BewegungsErinnerung: geladen {Eintraege.Count} Einträge");
                UpdateNextId();
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Application.Current?.MainPage?.DisplayAlert("Fehler", $"Laden fehlgeschlagen: {ex.Message}", "OK");
                });
            }
        }

        public async Task SaveEntryAsync()
        {
            if (string.IsNullOrWhiteSpace(Titel))
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Application.Current?.MainPage?.DisplayAlert("Fehler", "Titel ist erforderlich", "OK");
                });
                return;
            }

            try
            {
                var newEntry = new BewegungsErinnerungsEntity
                {
                    Id = NextId,
                    Titel = Titel,
                    // Übersetzt den deutschen String aus dem Picker ("Montag") zurück ins Enum (DayOfWeek.Monday)
                    Wochentag = GetDayOfWeekFromGerman(WochentagName),
                    Erinnerungszeitpunkt = Erinnerungszeitpunkt,
                    IstAktiv = IstAktiv,
                    IstBestaetigt = IstBestaetigt,
                    Beschreibung = Beschreibung ?? string.Empty,
                };

                await repository.SaveAsync(newEntry);

                Eintraege.Add(newEntry);
                ClearForm();
                UpdateNextId();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Application.Current?.MainPage?.DisplayAlert("Erfolg", "Eintrag erstellt", "OK");
                });
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Application.Current?.MainPage?.DisplayAlert("Fehler", $"Speichern fehlgeschlagen: {ex.Message}", "OK");
                });
            }
        }

        public async Task UpdateEntryAsync()
        {
            if (SelectedEntry == null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Application.Current?.MainPage?.DisplayAlert("Fehler", "Kein Eintrag ausgewählt", "OK");
                });
                return;
            }

            if (string.IsNullOrWhiteSpace(Titel))
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Application.Current?.MainPage?.DisplayAlert("Fehler", "Titel ist erforderlich", "OK");
                });
                return;
            }

            try
            {
                var updatedEntry = new BewegungsErinnerungsEntity
                {
                    Id = SelectedEntry.Id,
                    Titel = Titel,
                    // Übersetzt auch beim Update den Picker-String zurück ins Enum für die DB
                    Wochentag = GetDayOfWeekFromGerman(WochentagName),
                    Erinnerungszeitpunkt = Erinnerungszeitpunkt,
                    IstAktiv = IstAktiv,
                    IstBestaetigt = IstBestaetigt,
                    Beschreibung = Beschreibung ?? string.Empty,
                };

                await repository.SaveAsync(updatedEntry);

                var index = Eintraege.IndexOf(SelectedEntry);
                if (index >= 0)
                {
                    Eintraege[index] = updatedEntry;
                    SelectedEntry = null;
                    ClearForm();
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Application.Current?.MainPage?.DisplayAlert("Erfolg", "Eintrag aktualisiert", "OK");
                });
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Application.Current?.MainPage?.DisplayAlert("Fehler", $"Aktualisieren fehlgeschlagen: {ex.Message}", "OK");
                });
            }
        }

        public async Task DeleteEntryAsync()
        {
            if (SelectedEntry == null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Application.Current?.MainPage?.DisplayAlert("Fehler", "Kein Eintrag ausgewählt", "OK");
                });
                return;
            }

            try
            {
                var result = await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Application.Current?.MainPage?.DisplayAlert("Bestätigung", 
                        "Möchten Sie diesen Eintrag wirklich löschen?", "Ja", "Nein"));

                if (!result)
                    return;

                await repository.DeleteAsync(SelectedEntry.Id);
                Eintraege.Remove(SelectedEntry);
                SelectedEntry = null;
                ClearForm();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Application.Current?.MainPage?.DisplayAlert("Erfolg", "Eintrag gelöscht", "OK");
                });
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Application.Current?.MainPage?.DisplayAlert("Fehler", $"Löschen fehlgeschlagen: {ex.Message}", "OK");
                });
            }
        }

        private void LoadFormFromEntry(BewegungsErinnerungsEntity entry)
        {
            Titel = entry.Titel;

            // Übersetze Enum in deutschen String für das Formular
            WochentagName = enumToGerman.TryGetValue(entry.Wochentag, out var de) ? de : "Montag";

            Erinnerungszeitpunkt = entry.Erinnerungszeitpunkt;
            IstAktiv = entry.IstAktiv;
            IstBestaetigt = entry.IstBestaetigt;
            Beschreibung = entry.Beschreibung;
        }

        public void ClearForm()
        {
            Titel = null;
            WochentagName = "Montag"; // Standard
            Erinnerungszeitpunkt = new TimeSpan(9, 0, 0);
            IstAktiv = true;
            IstBestaetigt = false;
            Beschreibung = string.Empty;

            if (SelectedEntry != null)
                SelectedEntry.IsSelected = false;
            SelectedEntry = null;
            IsEditMode = false;
        }

        private void UpdateNextId()
        {
            NextId = Eintraege.Count > 0 ? Eintraege.Max(e => e.Id) + 1 : 1;
        }

        // Hilfsmethode, um den deutschen String wieder in das DayOfWeek-Enum zu verwandeln
        private DayOfWeek GetDayOfWeekFromGerman(string germanDay)
        {
            return enumToGerman.FirstOrDefault(x => x.Value == germanDay).Key;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
