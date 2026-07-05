using ReturnToMonkee.Onboarding;

namespace ReturnToMonkee.Features.Onboarding;

public partial class GoalOrientationView : ContentPage
{
	public GoalOrientationView(GoalOrientationViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
        SizeChanged += (_, _) => UpdateToggleRowWidths();

        _ = vm.LoadAsync();
    }

    private void UpdateToggleRowWidths()
    {
        var contentWidth = Width - 48;

        if (contentWidth <= 0)
        {
            return;
        }

        MovementReminderToggleRow.WidthRequest = contentWidth;
        SleepReminderToggleRow.WidthRequest = contentWidth;
    }
}
