using Microsoft.Extensions.Logging;
using SQLite;

namespace ReturnToMonkee.Infrastructure.Persistence;

public sealed class DemoDataSeeder
{
	private const int ExpectedSeedEntityCount = 3;
	private static readonly Guid TimeLimitRuleId = Guid.Parse("f4e8d4f7-7f6f-47d3-bdb1-4f2b60d5f01b");
	private static readonly Guid ReminderId = Guid.Parse("7d0ef1ef-6b75-4dc0-9d8f-0a2e8a8f1f6b");
	private static readonly Guid NotificationEventId = Guid.Parse("0a6a91b9-1d9b-4c21-9e36-2b4cb5f8d8d3");

	private readonly ILocalDatabase localDatabase;
	private readonly ILogger<DemoDataSeeder> logger;
	private bool seeded;

	public DemoDataSeeder(ILocalDatabase localDatabase, ILogger<DemoDataSeeder> logger)
	{
		this.localDatabase = localDatabase;
		this.logger = logger;
	}

	public async Task<int> EnsureSeedDataAsync(CancellationToken cancellationToken = default)
	{
		if (seeded)
		{
			return await GetSeedEntityCountAsync(cancellationToken);
		}

		var connection = await localDatabase.GetConnectionAsync(cancellationToken);
		await connection.CreateTableAsync<TimeLimitRule>();
		await connection.CreateTableAsync<Reminder>();
		await connection.CreateTableAsync<NotificationEvent>();

		var createdRows = 0;
		createdRows += await SeedTimeLimitRuleAsync(connection, cancellationToken);
		createdRows += await SeedReminderAsync(connection, cancellationToken);
		createdRows += await SeedNotificationEventAsync(connection, cancellationToken);

		seeded = true;
		logger.LogInformation("Seed data ready: {Count} entities, {Created} newly created", ExpectedSeedEntityCount, createdRows);
		return await GetSeedEntityCountAsync(cancellationToken);
	}

	public async Task<int> GetSeedEntityCountAsync(CancellationToken cancellationToken = default)
	{
		var connection = await localDatabase.GetConnectionAsync(cancellationToken);

		var ruleCount = await connection.Table<TimeLimitRule>().Where(rule => rule.Id == TimeLimitRuleId).CountAsync();
		var reminderCount = await connection.Table<Reminder>().Where(reminder => reminder.Id == ReminderId).CountAsync();
		var eventCount = await connection.Table<NotificationEvent>().Where(notificationEvent => notificationEvent.Id == NotificationEventId).CountAsync();

		return ruleCount + reminderCount + eventCount;
	}

	private async Task<int> SeedTimeLimitRuleAsync(SQLiteAsyncConnection connection, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var existing = await connection.Table<TimeLimitRule>()
			.Where(rule => rule.Id == TimeLimitRuleId)
			.CountAsync();

		if (existing > 0)
		{
			return 0;
		}

		var rule = new TimeLimitRule
		{
			Id = TimeLimitRuleId,
			Title = "Demo-Zeitlimit Social Media",
			Description = "Erste Demo-Regel fuer ein simples Zeitlimit.",
			IsEnabled = true,
			TimeLimitMinutes = 30,
			TargetApplication = "Social Media"
		};

		await connection.InsertAsync(rule);
		logger.LogInformation("Seed row created: TimeLimitRule {Id} {Title}", rule.Id, rule.Title);
		return 1;
	}

	private async Task<int> SeedReminderAsync(SQLiteAsyncConnection connection, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var existing = await connection.Table<Reminder>()
			.Where(reminder => reminder.Id == ReminderId)
			.CountAsync();

		if (existing > 0)
		{
			return 0;
		}

		var reminder = new Reminder
		{
			Id = ReminderId,
			Title = "Demo-Bewegungspause",
			Interval = "PT60M",
			IsEnabled = true
		};

		await connection.InsertAsync(reminder);
		logger.LogInformation("Seed row created: Reminder {Id} {Title}", reminder.Id, reminder.Title);
		return 1;
	}

	private async Task<int> SeedNotificationEventAsync(SQLiteAsyncConnection connection, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var existing = await connection.Table<NotificationEvent>()
			.Where(notificationEvent => notificationEvent.Id == NotificationEventId)
			.CountAsync();

		if (existing > 0)
		{
			return 0;
		}

		var notificationEvent = new NotificationEvent
		{
			Id = NotificationEventId,
			Time = DateTimeOffset.UtcNow,
			Title = "Demo-Benachrichtigung",
			Message = "Das ist eine Demo-Benachrichtigung fuer den ersten App-Start.",
			AppReference = "app://demo/start"
		};

		await connection.InsertAsync(notificationEvent);
		logger.LogInformation("Seed row created: NotificationEvent {Id} {Title}", notificationEvent.Id, notificationEvent.Title);
		return 1;
	}
}