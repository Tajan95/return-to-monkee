using Microsoft.Extensions.Logging;
using ReturnToMonkee.Infrastructure.Persistence;

namespace ReturnToMonkee;

public partial class MainPage : ContentPage
{
	private readonly ILocalDatabase localDatabase;
	private readonly DemoDataSeeder demoDataSeeder;
	private readonly ILogger<MainPage> logger;

	public MainPage(ILocalDatabase localDatabase, DemoDataSeeder demoDataSeeder, ILogger<MainPage> logger)
	{
		InitializeComponent();
		this.localDatabase = localDatabase;
		this.demoDataSeeder = demoDataSeeder;
		this.logger = logger;
	}



    // Wird aufgerufen, wenn die Seite sichtbar wird, und startet dann die Datenbankprüfung.
    protected override async void OnAppearing()
	{
		base.OnAppearing();
		await UpdateDatabaseStatusAsync();
	}



    // Fragt den Datenbankstatus ab, zeigt ihn im Label an und loggt Fehler für Entwickler.
    private async Task UpdateDatabaseStatusAsync()
	{
		try
		{
			var seedEntityCount = await demoDataSeeder.EnsureSeedDataAsync();
			var health = await localDatabase.CheckHealthAsync();

			DatabaseStatusLabel.Text = $"{health.Message}\n{seedEntityCount} seed entities ready";

			if (!health.IsReady)
			{
				logger.LogWarning("SQLite healthcheck failed: {Details}", health.Details);
			}
		}
		catch (Exception exception)
		{
			logger.LogError(exception, "Failed to refresh database status");
			DatabaseStatusLabel.Text = "Datenbankstatus konnte nicht geladen werden.";
		}
	}
}
