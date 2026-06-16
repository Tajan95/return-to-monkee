using SQLite;

[Table("TimeLimitRules")]
public class TimeLimitRule : AbstractRule
{
    public int TimeLimitMinutes { get; set; }

    public string TargetApplication { get; set; } = string.Empty;

    /// <summary>
    /// Gültig, wenn zusätzlich zum Titel (siehe <see cref="AbstractRule.IsValid"/>)
    /// ein positives Zeitlimit und eine Zielanwendung gesetzt sind.
    /// </summary>
    public override bool IsValid() =>
        base.IsValid() && TimeLimitMinutes > 0 && !string.IsNullOrWhiteSpace(TargetApplication);
}