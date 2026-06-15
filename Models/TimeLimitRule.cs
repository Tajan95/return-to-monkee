using SQLite;

[Table("TimeLimitRules")]
public class TimeLimitRule : AbstractRule
{
    public int TimeLimitMinutes { get; set; }

    public string TargetApplication { get; set; } = string.Empty;
}