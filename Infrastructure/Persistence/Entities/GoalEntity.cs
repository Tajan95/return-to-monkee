using SQLite;

namespace ReturnToMonkee.Infrastructure.Persistence.Entities;

[Table("Goals")]
public class GoalEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;
}