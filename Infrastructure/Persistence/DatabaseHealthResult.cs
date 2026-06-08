namespace ReturnToMonkee.Infrastructure.Persistence;

public sealed record DatabaseHealthResult(bool IsReady, string Message, string? Details)
{
	public static DatabaseHealthResult Ready() =>
		new(true, "DB bereit", null);

	public static DatabaseHealthResult Unavailable(string details) =>
		new(false, "DB nicht verfuegbar", details);
}
