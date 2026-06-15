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
            if (Application.Current?.MainPage is { } page)
                await page.DisplayAlert(title, message, "OK");
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
            if (Application.Current?.MainPage is { } page)
                return await page.DisplayAlert(title, message, confirmButton, dismissButton);
            return false;
        });
    }
}
