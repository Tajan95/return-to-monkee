using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;
using System.Collections.ObjectModel;

namespace ReturnToMonkee.Onboarding;

public partial class GoalOrientationViewModel : ObservableObject
{
    private readonly IGoalsRepository goalsRepository;
    private readonly ITimeLimitRuleRepository timeLimitRuleRepository;

    public ObservableCollection<GoalItem> Goals { get; } = new();

    public ObservableCollection<string> TimeLimitCategories { get; } =
        new()
        {
            "Social Media",
            "Video/Streaming",
            "Gaming",
            "Sonstiges"
        };

    [ObservableProperty]
    private string selectedTimeLimitCategory = "Social Media";

    [ObservableProperty]
    private string timeLimitMinutesText = "30";

    [ObservableProperty]
    private string validationMessage = string.Empty;

    public GoalOrientationViewModel(
        IGoalsRepository goalsRepository,
        ITimeLimitRuleRepository timeLimitRuleRepository)
    {
        this.goalsRepository = goalsRepository;
        this.timeLimitRuleRepository = timeLimitRuleRepository;
    }

    public async Task LoadAsync()
    {
        await goalsRepository.SeedAsync();

        var goals = await goalsRepository.GetAllGoalsAsync();
        var selectedIds = await goalsRepository.GetSelectedGoalIdsAsync();

        Goals.Clear();

        foreach (var g in goals)
        {
            Goals.Add(new GoalItem
            {
                Id = g.Id,
                Title = g.Title,
                IsSelected = selectedIds.Contains(g.Id)
            });
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ValidationMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(SelectedTimeLimitCategory))
        {
            ValidationMessage = "Bitte wähle eine Kategorie für deine erste Zeitlimit-Regel aus.";
            return;
        }

        if (!int.TryParse(TimeLimitMinutesText, out var timeLimitMinutes) ||
            timeLimitMinutes <= 0)
        {
            ValidationMessage = "Bitte gib ein gültiges Zeitlimit in Minuten ein.";
            return;
        }

        var selectedGoalIds = Goals
            .Where(x => x.IsSelected)
            .Select(x => x.Id)
            .ToList();

        await goalsRepository.SaveSelectedGoalsAsync(selectedGoalIds);

        await timeLimitRuleRepository.SaveInitialTimeLimitRuleAsync(
            SelectedTimeLimitCategory,
            timeLimitMinutes);

        // Todo: anpassen wenn die nächste Onboarding-Seite da ist.
        await Shell.Current.GoToAsync("//home");
    }
}

public partial class GoalItem : ObservableObject
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    [ObservableProperty]
    private bool isSelected;
}