using ReturnToMonkee.Infrastructure.Persistence;

namespace ReturnToMonkee.Features.PersonTest;

public sealed class PersonRepository : IPersonRepository
{
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

    // Löscht eine Person anhand ihrer Id.
    public async Task Delete(Guid id)
    {
        var connection = await GetDBConnectionAsync();
        await connection.DeleteAsync<Person>(id);
    }






    // Holt die Datenbankverbindung und stellt die Person-Tabelle bereit.
    private async Task<SQLite.SQLiteAsyncConnection> GetDBConnectionAsync()
    {
        var connection = await localDatabase.GetConnectionAsync();
        await connection.CreateTableAsync<Person>();

        return connection;
    }
}
