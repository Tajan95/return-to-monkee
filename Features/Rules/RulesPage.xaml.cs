using Microsoft.Maui.Controls;
using ReturnToMonkee.Infrastructure.Persistence;

namespace ReturnToMonkee.Features.Rules;

public partial class RulesPage : ContentPage
{
    private readonly RulesViewModel viewModel;

    public RulesPage(RulesViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.LoadRulesAsync();
    }

    private async void Switch_Toggled(object sender, ToggledEventArgs e)
    {
        try
        {
            if (sender is Switch sw && sw.BindingContext is RulesViewModel.RuleItem item)
            {
                await viewModel.ToggleRuleAsync(item, e.Value);
            }
        }
        catch (Exception)
        {
            // ignore for now
        }
    }

    private async void TestTimeLimit_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is Button button && button.BindingContext is RulesViewModel.RuleItem item)
            {
                await viewModel.TestTimeLimitRuleAsync(item);
            }
        }
        catch (Exception)
        {
            // ignore for now
        }
    }

    private async void AddRule_Clicked(object sender, EventArgs e)
    {
        var repository = IPlatformApplication.Current.Services.GetService<ReturnToMonkee.Infrastructure.Persistence.Repositories.ITimeLimitRuleRepository>();
        await Navigation.PushModalAsync(new NavigationPage(new EditTimeLimitRulePage(repository)));
    }

    private async void EditRule_Clicked(object sender, EventArgs e)
    {
        if (sender is BindableObject bindable && bindable.BindingContext is RulesViewModel.RuleItem item)
        {
            if (item.IsTimeLimitRule)
            {
                var repository = IPlatformApplication.Current.Services.GetService<ReturnToMonkee.Infrastructure.Persistence.Repositories.ITimeLimitRuleRepository>();
                var rule = new TimeLimitRule
                {
                    Id = item.Id,
                    Title = item.Title,
                    Description = item.Description,
                    TargetApplication = item.TargetApplication,
                    TimeLimitMinutes = item.TimeLimitMinutes,
                    IsEnabled = item.IsEnabled
                };
                await Navigation.PushModalAsync(new NavigationPage(new EditTimeLimitRulePage(repository, rule)));
            }
            else
            {
                await DisplayAlert("Hinweis", "Diese Regel kann hier nicht bearbeitet werden.", "OK");
            }
        }
    }
}
