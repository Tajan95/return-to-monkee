using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ReturnToMonkee.Infrastructure.Persistence;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace ReturnToMonkee.Features.BewegungsErinnerungDemo
{
    public class BewegungsErinnerungViewModel : INotifyPropertyChanged
    {
        private readonly IBewegungsErinnerungsRepository repository;
        private ObservableCollection<BewegungsErinnerungsEntity> eintraege = new();
        private BewegungsErinnerungsEntity? selectedEntry;
        private string? titel;
        private DayOfWeek wochentag = DayOfWeek.Monday;
        private TimeSpan erinnerungszeitpunkt = new TimeSpan(9, 0, 0);
        private bool istAktiv = true;
        private bool istBestaetigt = false;
        private string? beschreibung = string.Empty;
        private int nextId = 1;
        public IEnumerable<DayOfWeek> Wochentage { get; } = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>();
        public ICommand SelectEntryCommand { get; }

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

        public DayOfWeek Wochentag
        {
            get => wochentag;
            set => SetProperty(ref wochentag, value);
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
                // deselect previous
                if (SelectedEntry != null)
                    SelectedEntry.IsSelected = false;

                // select new
                e.IsSelected = true;
                SelectedEntry = e;
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
                    Wochentag = Wochentag,
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
                    Wochentag = Wochentag,
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
            Wochentag = entry.Wochentag;
            Erinnerungszeitpunkt = entry.Erinnerungszeitpunkt;
            IstAktiv = entry.IstAktiv;
            IstBestaetigt = entry.IstBestaetigt;
            Beschreibung = entry.Beschreibung;
            System.Diagnostics.Debug.WriteLine($"LoadFormFromEntry: Titel={Titel}, Wochentag={Wochentag}, Zeit={Erinnerungszeitpunkt}, Aktiv={IstAktiv}, Bestaetigt={IstBestaetigt}, BeschreibungLen={Beschreibung?.Length}");
        }

        public void ClearForm()
        {
            Titel = null;
            Wochentag = DayOfWeek.Monday;
            Erinnerungszeitpunkt = new TimeSpan(9, 0, 0);
            IstAktiv = true;
            IstBestaetigt = false;
            Beschreibung = string.Empty;
            if (SelectedEntry != null)
                SelectedEntry.IsSelected = false;
            SelectedEntry = null;
        }

        private void UpdateNextId()
        {
            NextId = Eintraege.Count > 0 ? Eintraege.Max(e => e.Id) + 1 : 1;
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
