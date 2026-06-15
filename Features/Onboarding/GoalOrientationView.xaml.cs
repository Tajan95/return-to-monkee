using ReturnToMonkee.Onboarding;

namespace ReturnToMonkee.Features.Onboarding;

public partial class GoalOrientationView : ContentPage
{
	public GoalOrientationView(GoalOrientationViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;

        _ = vm.LoadAsync();
    }
}