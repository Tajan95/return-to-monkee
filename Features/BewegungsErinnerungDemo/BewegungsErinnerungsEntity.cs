using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using SQLite;

namespace ReturnToMonkee.Features.BewegungsErinnerungDemo
{
    public sealed class BewegungsErinnerungsEntity
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string Titel { get; set; } = string.Empty;

        public DayOfWeek Wochentag { get; set; }

        // Persistiere die Zeit als Minuten seit Mitternacht (int) – übersichtlicher in der DB
        public int ErinnerungszeitpunktMinutes { get; set; } = 9 * 60;

        [Ignore]
        public TimeSpan Erinnerungszeitpunkt
        {
            get => TimeSpan.FromMinutes(ErinnerungszeitpunktMinutes);
            set => ErinnerungszeitpunktMinutes = (int)value.TotalMinutes;
        }

        public bool IstAktiv { get; set; } = true;

        public bool IstBestaetigt { get; set; } = false;

        public string Beschreibung { get; set; } = string.Empty;

        // Hilfsfeld für UI-Auswahl — nicht in die DB schreiben
        [Ignore]
        public bool IsSelected { get; set; } = false;

    }
}
