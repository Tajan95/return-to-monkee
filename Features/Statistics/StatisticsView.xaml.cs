namespace ReturnToMonkee.Features.Statistics;

public partial class StatisticsView : ContentPage
{
    private readonly StatisticsViewModel viewModel;

    public StatisticsView(StatisticsViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.LoadStatisticsCommand.ExecuteAsync(null);
    }
}