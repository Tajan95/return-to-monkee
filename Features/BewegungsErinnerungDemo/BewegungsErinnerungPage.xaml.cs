namespace ReturnToMonkee.Features.BewegungsErinnerungDemo;

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

    private async void OnNewClicked(object sender, EventArgs e)
    {
        viewModel.SelectedEntry = null;
        viewModel.ClearForm();
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
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        await viewModel.DeleteEntryAsync();
    }
}
