using Microsoft.Extensions.Logging;

namespace ReturnToMonkee.Features.TestStringDemo;

public partial class TestStringCrudPage : ContentPage
{
	private readonly ITestStringRepository repository;
	private readonly ILogger<TestStringCrudPage> logger;

	public TestStringCrudPage(ITestStringRepository repository, ILogger<TestStringCrudPage> logger)
	{
		InitializeComponent();
		this.repository = repository;
		this.logger = logger;
	}

	/// <summary>
	/// Laedt beim Anzeigen der Seite den gespeicherten Wert aus SQLite.
	/// </summary>
	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await LoadStoredValueAsync("Read: gespeicherten Wert geladen.");
	}

	/// <summary>
	/// Create: Speichert den Eingabetext als Demo-Datensatz.
	/// </summary>
	private async void OnCreateClicked(object sender, EventArgs e)
	{
		await SaveInputAsync("Create: Wert gespeichert.");
	}

	/// <summary>
	/// Read: Liest den aktuell gespeicherten Wert erneut aus SQLite.
	/// </summary>
	private async void OnReadClicked(object sender, EventArgs e)
	{
		await LoadStoredValueAsync("Read: gespeicherten Wert geladen.");
	}

	/// <summary>
	/// Update: Speichert den Eingabetext erneut auf demselben Demo-Datensatz.
	/// </summary>
	private async void OnUpdateClicked(object sender, EventArgs e)
	{
		await SaveInputAsync("Update: Wert aktualisiert.");
	}

	/// <summary>
	/// Delete: Entfernt den Demo-Datensatz aus SQLite und leert die Anzeige.
	/// </summary>
	private async void OnDeleteClicked(object sender, EventArgs e)
	{
		try
		{
			await repository.DeleteAsync();
			TestStringEntry.Text = string.Empty;
			StoredValueLabel.Text = "Kein Wert gespeichert.";
			StatusLabel.Text = "Delete: Wert geloescht.";
		}
		catch (Exception exception)
		{
			logger.LogError(exception, "TestString delete failed.");
			StatusLabel.Text = "Fehler beim Loeschen.";
		}
	}

	private async Task SaveInputAsync(string successMessage)
	{
		try
		{
			var value = TestStringEntry.Text?.Trim() ?? string.Empty;
			await repository.SaveAsync(value);
			await LoadStoredValueAsync(successMessage);
		}
		catch (Exception exception)
		{
			logger.LogError(exception, "TestString save failed.");
			StatusLabel.Text = "Fehler beim Speichern.";
		}
	}

	private async Task LoadStoredValueAsync(string successMessage)
	{
		try
		{
			var value = await repository.LoadAsync();
			StoredValueLabel.Text = string.IsNullOrWhiteSpace(value)
				? "Kein Wert gespeichert."
				: value;
			TestStringEntry.Text = value ?? string.Empty;
			StatusLabel.Text = successMessage;
		}
		catch (Exception exception)
		{
			logger.LogError(exception, "TestString read failed.");
			StatusLabel.Text = "Fehler beim Laden.";
		}
	}
}
