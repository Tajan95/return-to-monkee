using SQLite;

namespace ReturnToMonkee.Features.PersonTest;

public sealed class Person
{
    [PrimaryKey]
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;
}
