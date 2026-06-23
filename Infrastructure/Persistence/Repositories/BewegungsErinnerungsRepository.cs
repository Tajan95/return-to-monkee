using System;
using System.Collections.Generic;
using System.Text;
using ReturnToMonkee.Infrastructure.Persistence;
using ReturnToMonkee.Infrastructure.Persistence.Entities;
using SQLite;

namespace ReturnToMonkee.Infrastructure.Persistence.Repositories
{
    public sealed class BewegungsErinnerungsRepository : IBewegungsErinnerungsRepository
    {
        private readonly ILocalDatabase localDatabase;

        public BewegungsErinnerungsRepository(ILocalDatabase localDatabase)
        {
            this.localDatabase = localDatabase;
        }

        public async Task InitializeAsync()
        {
            var connection = await GetReadyConnectionAsync();
        }

        public async Task<BewegungsErinnerungsEntity?> LoadAsync(int id)
        {
            var connection = await GetReadyConnectionAsync();
            var entry = await connection.FindAsync<BewegungsErinnerungsEntity>(id);

            return entry;
        }

        public async Task<IEnumerable<BewegungsErinnerungsEntity>> GetAllAsync()
        {
            var connection = await GetReadyConnectionAsync();
            var list = await connection.Table<BewegungsErinnerungsEntity>().ToListAsync();
            // Migration helper: ältere Datensätze könnten noch eine TimeSpan als Ticks
            // in der Spalte 'Erinnerungszeitpunkt' enthalten. Erkenne große Werte
            // und konvertiere sie in Minuten in das neue Feld ErinnerungszeitpunktMinutes.
            foreach (var e in list)
            {
                if (e.ErinnerungszeitpunktMinutes == 0)
                {
                    try
                    {
                        // Versuche alten Wert auszulesen (64-bit integer)
                        var old = await connection.ExecuteScalarAsync<long?>("SELECT Erinnerungszeitpunkt FROM BewegungsErinnerungsEntity WHERE Id = ?", e.Id);
                        if (old.HasValue && old.Value > 24 * 60)
                        {
                            var ts = TimeSpan.FromTicks(old.Value);
                            e.ErinnerungszeitpunktMinutes = (int)ts.TotalMinutes;
                            await connection.UpdateAsync(e);
                        }
                    }
                    catch
                    {
                        // ignore if column doesn't exist or query fails
                    }
                }
            }

            return list;
        }

        public async Task SaveAsync(BewegungsErinnerungsEntity entry)
        {
            var connection = await GetReadyConnectionAsync();
            await connection.InsertOrReplaceAsync(entry);
        }

        public async Task DeleteAsync(int id)
        {
            var connection = await GetReadyConnectionAsync();
            await connection.DeleteAsync<BewegungsErinnerungsEntity>(id);
        }

        private async Task<SQLiteAsyncConnection> GetReadyConnectionAsync()
        {
            var connection = await localDatabase.GetConnectionAsync();
            await connection.CreateTableAsync<BewegungsErinnerungsEntity>();

            return connection;
        }
    }
}
