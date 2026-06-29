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
}
