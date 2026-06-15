using SQLite;

[Table("Reminders")]
public class Reminder
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Interval { get; set; } = "* * * * *";
    public bool IsEnabled { get; set; }
}