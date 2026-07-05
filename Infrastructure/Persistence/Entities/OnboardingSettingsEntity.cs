using SQLite;

namespace ReturnToMonkee.Infrastructure.Persistence.Entities;

[Table("OnboardingSettings")]
public sealed class OnboardingSettingsEntity
{
    [PrimaryKey]
    public int Id { get; set; } = 1;

    public string GoalOrientation { get; set; } = string.Empty;

    public int MovementReminderIntervalMinutes { get; set; } = 60;

    public bool MovementReminderEnabled { get; set; } = true;
}
