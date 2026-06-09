using ReturnToMonkee.Infrastructure.Persistence;

namespace ReturnToMonkee.Features.PersonTest;

public sealed class PersonRepository : IPersonRepository
{
    // WICHTIG! WENN ENTITÄT VERÄNDERT WURDE, DIESE ZAHL INKREMENTIEREN:
    private const int PersonSchemaVersion = 1;
    private bool personSchemaEnsured;

    private readonly ILocalDatabase localDatabase;

    public PersonRepository(ILocalDatabase localDatabase)
    {
        this.localDatabase = localDatabase;
    }






    // Holt eine Person anhand ihrer Id.
    public async Task<Person?> Get(Guid id)
    {
        var connection = await GetDBConnectionAsync();
        return await connection.FindAsync<Person>(id);
    }

    // Holt alle gespeicherten Personen.
    public async Task<IReadOnlyList<Person>> GetAll()
    {
        var connection = await GetDBConnectionAsync();
        return await connection.Table<Person>().ToListAsync();
    }






    // Legt eine neue Person an und erzeugt bei Bedarf eine Id.
    public async Task Create(Person person)
    {
        var connection = await GetDBConnectionAsync();

        if (person.Id == Guid.Empty)
        {
            person.Id = Guid.NewGuid();
        }

        await connection.InsertAsync(person);
    }

    // Aktualisiert eine vorhandene Person.
    public async Task Update(Person person)
    {
        var connection = await GetDBConnectionAsync();
        await connection.UpdateAsync(person);
    }

    // LÃ¶scht eine Person anhand ihrer Id.
    public async Task Delete(Guid id)
    {
        var connection = await GetDBConnectionAsync();
        await connection.DeleteAsync<Person>(id);
    }






    // Holt die Datenbankverbindung und stellt die Person-Tabelle bereit.
    private async Task<SQLite.SQLiteAsyncConnection> GetDBConnectionAsync()
    {
        var connection = await localDatabase.GetConnectionAsync();

        if (!personSchemaEnsured)
        {
            await EnsurePersonSchemaAsync(connection);
            personSchemaEnsured = true;
        }

        return connection;
    }




    // Setzt die Person-Tabelle neu auf, wenn sich die Schema-Version geändert hat.
    private static async Task EnsurePersonSchemaAsync(SQLite.SQLiteAsyncConnection connection)
    {
        var currentSchemaVersion = await connection.ExecuteScalarAsync<int>("PRAGMA user_version");

        if (currentSchemaVersion == PersonSchemaVersion)
        {
            await connection.CreateTableAsync<Person>();
            return;
        }

        // Wenn neues Schema (Entität verändert) Tabelle neu aufsetzten.
        await connection.DropTableAsync<Person>();
        await connection.CreateTableAsync<Person>();
        await connection.ExecuteAsync($"PRAGMA user_version = {PersonSchemaVersion}");
    }
}