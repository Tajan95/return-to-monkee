using SQLite;

[Table("NotificationEvents")]
public class NotificationEvent : Event
{
    public string Message { get; set; } = string.Empty;

    // param type WIP
    public string AppReference { get; set; } = string.Empty;

    public void Post()
    {
        // to be implemented: post the notification to the user
    }
}