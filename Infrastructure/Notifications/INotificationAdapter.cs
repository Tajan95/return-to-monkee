namespace ReturnToMonkee.Infrastructure.Notifications;

/// <summary>
/// Kapselt das Senden von Benachrichtigungen so, dass der Rest der App
/// keine direkte Abhaengigkeit auf MAUI-Platform-APIs hat.
/// </summary>
public interface INotificationAdapter
{
    /// <summary>
    /// Zeigt eine einfache Benachrichtigung ohne Rueckmeldung des Nutzers.
    /// </summary>
    Task SendAsync(string title, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Zeigt eine Benachrichtigung mit zwei Auswahlmoeglichkeiten und gibt
    /// true zurueck, wenn der Nutzer den Bestaetigen-Button gedrueckt hat.
    /// </summary>
    Task<bool> PromptAsync(
        string title,
        string message,
        string confirmButton,
        string dismissButton,
        CancellationToken cancellationToken = default);
}
