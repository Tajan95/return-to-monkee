namespace ReturnToMonkee.Features.PersonTest;

public interface IPersonRepository
{
    /// <summary>
    /// Holt eine Person anhand ihrer Id.
    /// </summary>
    Task<Person?> Get(Guid id);

    /// <summary>
    /// Holt alle gespeicherten Personen.
    /// </summary>
    Task<IReadOnlyList<Person>> GetAll();

    /// <summary>
    /// Legt eine neue Person an und erzeugt bei Bedarf eine Id.
    /// </summary>
    Task Create(Person person);

    /// <summary>
    /// Aktualisiert eine vorhandene Person.
    /// </summary>
    Task Update(Person person);

    /// <summary>
    /// Löscht eine Person anhand ihrer Id.
    /// </summary>
    Task Delete(Guid id);
}
