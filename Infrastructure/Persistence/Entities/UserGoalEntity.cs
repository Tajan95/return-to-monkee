using SQLite;

namespace ReturnToMonkee.Infrastructure.Persistence.Entities;

[Table("UserGoals")]
public class UserGoalEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int GoalId { get; set; }
}