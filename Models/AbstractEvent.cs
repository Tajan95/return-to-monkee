public abstract class Event
{
    public Guid Id { get; set; }
    public DateTimeOffset Time { get; set; }
    public string Title { get; set; } = string.Empty;
}