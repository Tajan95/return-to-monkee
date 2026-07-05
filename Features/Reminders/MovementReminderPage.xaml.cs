namespace ReturnToMonkee.Features.Reminders;

public partial class MovementReminderPage : ContentPage
{
    private readonly MovementReminderViewModel viewModel;

    public MovementReminderPage(MovementReminderViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
        SizeChanged += (_, _) => UpdateToggleRowWidth();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        UpdateToggleRowWidth();
        await viewModel.LoadAsync();
    }

    private void UpdateToggleRowWidth()
    {
        var contentWidth = Width - 48;

        if (contentWidth > 0)
        {
            ReminderToggleRow.WidthRequest = contentWidth;
        }
    }
}
