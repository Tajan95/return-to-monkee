using System;
using Microsoft.Maui.Controls;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;

namespace ReturnToMonkee.Features.Rules;

public partial class EditTimeLimitRulePage : ContentPage
{
    private readonly ITimeLimitRuleRepository repository;
    private TimeLimitRule rule;
    private bool isNew;
    private bool isClosing;

    public EditTimeLimitRulePage(ITimeLimitRuleRepository repository, TimeLimitRule rule = null)
    {
        InitializeComponent();
        this.repository = repository;

        if (rule == null)
        {
            this.rule = new TimeLimitRule { Id = Guid.NewGuid(), IsEnabled = true };
            isNew = true;
            Title = "Neue Regel";
            DeleteButton.IsVisible = false;
        }
        else
        {
            this.rule = rule;
            isNew = false;
            Title = "Regel bearbeiten";
            DeleteButton.IsVisible = true;

            TargetApplicationEntry.Text = rule.TargetApplication;
            TimeLimitEntry.Text = rule.TimeLimitMinutes.ToString();
            IsEnabledSwitch.IsToggled = rule.IsEnabled;
        }
    }

    private async void Save_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TargetApplicationEntry.Text))
        {
            await DisplayAlert("Fehler", "Bitte eine Aktivität/Kategorie eingeben.", "OK");
            return;
        }

        if (!int.TryParse(TimeLimitEntry.Text, out int minutes) || minutes <= 0)
        {
            await DisplayAlert("Fehler", "Bitte ein gültiges Zeitlimit (größer 0) eingeben.", "OK");
            return;
        }

        rule.Title = $"{TargetApplicationEntry.Text} begrenzen";
        rule.Description = $"Tägliches Zeitlimit für {TargetApplicationEntry.Text}: {minutes} Minuten";
        rule.TargetApplication = TargetApplicationEntry.Text;
        rule.TimeLimitMinutes = minutes;
        rule.IsEnabled = IsEnabledSwitch.IsToggled;

        if (isNew)
        {
            await repository.AddAsync(rule);
        }
        else
        {
            await repository.UpdateAsync(rule);
        }

        await CloseModalAsync();
    }

    private async void Cancel_Clicked(object sender, EventArgs e)
    {
        await CloseModalAsync();
    }

    private async void Delete_Clicked(object sender, EventArgs e)
    {
        if (isNew) return;

        bool confirm = await DisplayAlert("Löschen", "Soll diese Regel wirklich gelöscht werden?", "Ja", "Nein");
        if (confirm)
        {
            await repository.DeleteAsync(rule);
            await CloseModalAsync();
        }
    }

    private async Task CloseModalAsync()
    {
        if (isClosing)
        {
            return;
        }

        isClosing = true;
        IsEnabledSwitch.Unfocus();
        IsEnabledSwitch.IsEnabled = false;

        await Task.Yield();
        await Navigation.PopModalAsync();
    }
}
