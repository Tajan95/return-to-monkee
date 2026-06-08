using Microsoft.Extensions.Logging;
using ReturnToMonkee.Infrastructure.Persistence;

namespace ReturnToMonkee;

public partial class MainPage : ContentPage
{
	private readonly ILocalDatabase localDatabase;
	private readonly ILogger<MainPage> logger;

	public MainPage(ILocalDatabase localDatabase, ILogger<MainPage> logger)
	{
		InitializeComponent();
		this.localDatabase = localDatabase;
		this.logger = logger;
	}

	/// <summary>
	/// Wird aufgerufen, wenn die Seite sichtbar wird, und startet dann die Datenbankpruefung.
	/// </summary>
	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await UpdateDatabaseStatusAsync();
	}

	/// <summary>
	/// Fragt den Datenbankstatus ab, zeigt ihn im Label an und loggt Fehler fuer Entwickler.
	/// </summary>
	private async Task UpdateDatabaseStatusAsync()
	{
		var health = await localDatabase.CheckHealthAsync();
		DatabaseStatusLabel.Text = health.Message;

		if (!health.IsReady)
		{
			logger.LogWarning("SQLite healthcheck failed: {Details}", health.Details);
		}
	}
}
