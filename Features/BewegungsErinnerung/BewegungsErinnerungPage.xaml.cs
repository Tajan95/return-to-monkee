using ReturnToMonkee.Features.BewegungsErinnerung;

namespace ReturnToMonkee.Features.BewegungsErinnerung;

public partial class BewegungsErinnerungPage : ContentPage
{
    private readonly BewegungsErinnerungViewModel viewModel;

    public BewegungsErinnerungPage(BewegungsErinnerungViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.LoadEntriesAsync();
    }

    private void OnNewClicked(object sender, EventArgs e)
    {
        // 1. Logik für den "Neu"-Modus setzen
        viewModel.IsEditMode = true;
        viewModel.IsNewEntry = true;
        viewModel.SelectedEntry = null;

        // 2. Bestehenden Command im ViewModel ausführen
        viewModel.NewEntryCommand.Execute(null);
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (viewModel.SelectedEntry == null)
        {
            await viewModel.SaveEntryAsync();
        }
        else
        {
            await viewModel.UpdateEntryAsync();
        }

        // Nach dem Speichern den Edit-Modus wieder beenden
        viewModel.IsEditMode = false;
        viewModel.IsNewEntry = false;
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        await viewModel.DeleteEntryAsync();

        // Nach dem Löschen Formular einklappen
        viewModel.IsEditMode = false;
        viewModel.IsNewEntry = false;
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        // Wenn abgebrochen wird, einfach den Modus beenden und Formular verstecken
        viewModel.IsEditMode = false;
        viewModel.IsNewEntry = false;
    }
}