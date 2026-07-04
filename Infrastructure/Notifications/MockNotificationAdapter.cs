namespace ReturnToMonkee.Infrastructure.Notifications;

/// <summary>
/// MVP-Mock fuer INotificationAdapter. Zeigt Benachrichtigungen als
/// in-App-Dialoge (DisplayAlert) statt als echte Platform-Notifications.
/// Kein Zugriff auf native Push-Notification-APIs.
/// </summary>
public sealed class MockNotificationAdapter : INotificationAdapter
{
    private readonly Func<string, string, Task>? displayOverride;
    private readonly Func<string, string, string, string, Task<bool>>? promptOverride;

    /// <summary>
    /// Konstruktor fuer die App (DI). Nutzt MainThread + DisplayAlert.
    /// </summary>
    public MockNotificationAdapter() { }

    /// <summary>
    /// Konstruktor fuer Tests. Ersetzt DisplayAlert durch injizierte Funktionen.
    /// </summary>
    public MockNotificationAdapter(
        Func<string, string, Task> display,
        Func<string, string, string, string, Task<bool>> prompt)
    {
        displayOverride = display;
        promptOverride = prompt;
    }

    /// <inheritdoc/>
    public async Task SendAsync(string title, string message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (displayOverride is not null)
        {
            await displayOverride(title, message);
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            // Nur anzeigen, wenn eine gerootete Page (gueltiger XamlRoot) existiert.
            // Andernfalls still ueberspringen statt auf WinUI zu crashen (#59).
            if (GetActivePage() is { } page)
                await page.DisplayAlertAsync(title, message, "OK");
        });
    }

    /// <inheritdoc/>
    public async Task<bool> PromptAsync(
        string title,
        string message,
        string confirmButton,
        string dismissButton,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (promptOverride is not null)
            return await promptOverride(title, message, confirmButton, dismissButton);

        return await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            // Kein gerooteter XamlRoot (z.B. faelliger Reminder direkt beim App-Start,
            // bevor das Fenster aufgebaut ist) -> Dialog ueberspringen statt crashen (#59).
            // Der Reminder bleibt faellig und wird beim naechsten Tick erneut versucht.
            if (GetActivePage() is { } page)
                return await page.DisplayAlertAsync(title, message, confirmButton, dismissButton);

            return false;
        });
    }

    /// <summary>
    /// Liefert die aktuell sichtbare, an die Plattform gebundene Page oder null.
    /// Bewusst ohne das deprecatete <c>Application.MainPage</c>: es kann waehrend des
    /// Starts eine Page ohne gueltigen <c>XamlRoot</c> liefern, was auf WinUI beim
    /// <c>ContentDialog.ShowAsync</c> eine ArgumentException wirft. Ein vorhandener
    /// <c>Handler</c> ist der Indikator, dass die Page tatsaechlich im Visual Tree haengt.
    /// </summary>
    private static Page? GetActivePage()
    {
        var window = Application.Current?.Windows.FirstOrDefault(w => w.Page?.Handler is not null);
        return window?.Page;
    }
}
