using ReturnToMonkee.Infrastructure.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReturnToMonkee.Infrastructure.Persistence.Repositories
{
    public interface IBewegungsErinnerungsRepository
    {
        /// <summary>
        /// Initialisiert die Tabelle (erstellt sie bei Bedarf).
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Liest alle Einträge aus der Tabelle.
        /// </summary>
        Task<IEnumerable<BewegungsErinnerungsEntity>> GetAllAsync();

        /// <summary>
        /// Liest einen Datensatz anhand seiner ID aus der Datenbank.
        /// </summary>
        Task<BewegungsErinnerungsEntity?> LoadAsync(int id);

        /// <summary>
        /// Speichert oder aktualisiert einen vollständigen Eintrag.
        /// </summary>
        Task SaveAsync(BewegungsErinnerungsEntity entry);

        /// <summary>
        /// Loescht einen Datensatz anhand seiner ID.
        /// </summary>
        Task DeleteAsync(int id);
    }
}
