using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Transactions;
using SQLite;

namespace ReturnToMonkee.Infrastructure.Persistence.Entities
{
    public sealed class BewegungsErinnerungsEntity
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string Titel { get; set; } = string.Empty;

        public DayOfWeek Wochentag { get; set; }

        // Hilfs-Property für die Anzeige in der Liste auf Deutsch ---
        [Ignore]
        public string WochentagDeutsch => CultureInfo.GetCultureInfo("de-DE").DateTimeFormat.GetDayName(Wochentag);

        // Persistiere die Zeit als Minuten seit Mitternacht (int) – übersichtlicher in der DB
        public int ErinnerungszeitpunktMinutes { get; set; } = 9 * 60;

        [Ignore]
        public TimeSpan Erinnerungszeitpunkt
        {
            get => TimeSpan.FromMinutes(ErinnerungszeitpunktMinutes);
            set => ErinnerungszeitpunktMinutes = (int)value.TotalMinutes;
        }

        // Hilfs-Property für das saubere 24h-Format (z.B. "17:00") ohne Sekunden ---
        [Ignore]
        public string FormatierteZeit => Erinnerungszeitpunkt.ToString(@"hh\:mm");

        public bool IstAktiv { get; set; } = true;

        public bool IstBestaetigt { get; set; } = false;

        public string Beschreibung { get; set; } = string.Empty;

        // Hilfsfeld für UI-Auswahl — nicht in die DB schreiben
        [Ignore]
        public bool IsSelected { get; set; } = false;
    }
}