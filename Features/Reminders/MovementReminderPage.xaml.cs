namespace ReturnToMonkee.Features.Reminders;

public partial class MovementReminderPage : ContentPage
{
    private readonly MovementReminderViewModel viewModel;

    public MovementReminderPage(MovementReminderViewModel viewModel)
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
