namespace ReturnToMonkee.Models;

/// <summary>
/// Aggregierte Tagesstatistiken basierend auf erfassten Events.
/// </summary>
public class DailyStatistics
{
    public DateTime Date { get; set; }

    // Limits
    public int LimitsKept { get; set; }
    public int LimitsExceeded { get; set; }

    // Movement Reminders
    public int MovementRemindersConfirmed { get; set; }
    public int MovementRemindersIgnored { get; set; }

    // Sleep Reminders
    public int SleepRemindersConfirmed { get; set; }
    public int SleepRemindersIgnored { get; set; }

    // Berechnete Properties
    public int TotalReminders =>
        MovementRemindersConfirmed + MovementRemindersIgnored +
        SleepRemindersConfirmed + SleepRemindersIgnored;

    public double ReminderConfirmationRate =>
        TotalReminders > 0
            ? Math.Round(100.0 * (MovementRemindersConfirmed + SleepRemindersConfirmed) / TotalReminders, 1)
            : 0;

    public int TotalLimitsChecked => LimitsKept + LimitsExceeded;

    public double LimitKeptRate =>
        TotalLimitsChecked > 0
            ? Math.Round(100.0 * LimitsKept / TotalLimitsChecked, 1)
            : 0;
}