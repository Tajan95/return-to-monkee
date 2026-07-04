namespace ReturnToMonkee.Features.Reminders;

public partial class SleepReminderPage : ContentPage
{
    private readonly SleepReminderViewModel viewModel;

    public SleepReminderPage(SleepReminderViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.LoadAsync();
    }
}
