using System;
using Microsoft.Maui.Controls;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;

namespace ReturnToMonkee.Features.Rules;

public partial class EditTimeLimitRulePage : ContentPage
{
    private static readonly List<string> DefaultCategories = new()
    {
        "Social Media",
        "Video/Streaming",
        "Gaming",
        "Sonstiges"
    };

    private readonly ITimeLimitRuleRepository repository;
    private readonly List<string> categories;
    private TimeLimitRule rule;
    private bool isNew;
    private bool isClosing;

    public EditTimeLimitRulePage(ITimeLimitRuleRepository repository, TimeLimitRule rule = null)
    {
        InitializeComponent();
        this.repository = repository;
        categories = new List<string>(DefaultCategories);
        TargetApplicationPicker.ItemsSource = categories;

        if (rule == null)
        {
            this.rule = new TimeLimitRule { Id = Guid.NewGuid(), IsEnabled = true };
            isNew = true;
            FormHeadingLabel.Text = "Neue Regel anlegen";
            FormSubtitleLabel.Text = "Erstelle eine Regel, um für eine Aktivität ein tägliches Limit zu setzen.";
            TargetApplicationPicker.SelectedItem = categories[0];
            DeleteButton.IsVisible = false;
        }
        else
        {
            this.rule = rule;
            isNew = false;
            FormHeadingLabel.Text = "Regel bearbeiten";
            FormSubtitleLabel.Text = "Passe Kategorie, Limit und Status dieser Regel an.";
            FormHeadingIcon.Glyph = "\uf303";
            DeleteButton.IsVisible = true;

            if (!string.IsNullOrWhiteSpace(rule.TargetApplication) &&
                !categories.Contains(rule.TargetApplication))
            {
                categories.Add(rule.TargetApplication);
                TargetApplicationPicker.ItemsSource = null;
                TargetApplicationPicker.ItemsSource = categories;
            }

            TargetApplicationPicker.SelectedItem = rule.TargetApplication;
            TimeLimitEntry.Text = rule.TimeLimitMinutes.ToString();
            IsEnabledSwitch.IsToggled = rule.IsEnabled;
        }
    }

    private async void Save_Clicked(object sender, EventArgs e)
    {
        var targetApplication = TargetApplicationPicker.SelectedItem as string;

        if (string.IsNullOrWhiteSpace(targetApplication))
        {
            await DisplayAlert("Fehler", "Bitte eine Aktivität/Kategorie eingeben.", "OK");
            return;
        }

        if (!int.TryParse(TimeLimitEntry.Text, out int minutes) || minutes <= 0)
        {
            await DisplayAlert("Fehler", "Bitte ein gültiges Zeitlimit (größer 0) eingeben.", "OK");
            return;
        }

        rule.Title = $"{targetApplication} begrenzen";
        rule.Description = $"Tägliches Zeitlimit für {targetApplication}: {minutes} Minuten";
        rule.TargetApplication = targetApplication;
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
