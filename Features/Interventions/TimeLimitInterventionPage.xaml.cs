namespace ReturnToMonkee.Features.Interventions;

public partial class TimeLimitInterventionPage : ContentPage
{
    private readonly TimeLimitInterventionViewModel viewModel;

    public TimeLimitInterventionPage(TimeLimitInterventionViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    private async void MarkExceeded_Clicked(object sender, EventArgs e)
    {
        await viewModel.MarkTimeLimitExceededAsync();
    }
}
