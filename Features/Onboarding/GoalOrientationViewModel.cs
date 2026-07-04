using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReturnToMonkee.Features.Onboarding;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;
using System.Collections.ObjectModel;

namespace ReturnToMonkee.Onboarding;

public partial class GoalOrientationViewModel : ObservableObject
{
    private readonly IGoalsRepository goalsRepository;
    private readonly IOnboardingRepository onboardingRepository;
    private readonly ITimeLimitRuleRepository timeLimitRuleRepository;

    public ObservableCollection<GoalItem> Goals { get; } = new();
    public ObservableCollection<int> MovementReminderIntervals { get; } = new() { 30, 60, 90 };

    public ObservableCollection<string> TimeLimitCategories { get; } =
        new()
        {
            "Social Media",
            "Video/Streaming",
            "Gaming",
            "Sonstiges"
        };

    [ObservableProperty]
    private int selectedMovementReminderInterval = 60;

    [ObservableProperty]
    private string selectedTimeLimitCategory = "Social Media";

    [ObservableProperty]
    private string timeLimitMinutesText = "30";

    [ObservableProperty]
    private string validationMessage = string.Empty;

    public GoalOrientationViewModel(
        IGoalsRepository goalsRepository,
        IOnboardingRepository onboardingRepository,
        ITimeLimitRuleRepository timeLimitRuleRepository)
    {
        this.goalsRepository = goalsRepository;
        this.onboardingRepository = onboardingRepository;
        this.timeLimitRuleRepository = timeLimitRuleRepository;
    }

    public async Task LoadAsync()
    {
        await goalsRepository.SeedAsync();

        var goals = await goalsRepository.GetAllGoalsAsync();
        var selectedIds = await goalsRepository.GetSelectedGoalIdsAsync();
        SelectedMovementReminderInterval =
            await onboardingRepository.GetMovementReminderIntervalMinutesAsync();

        Goals.Clear();

        foreach (var goal in goals)
        {
            Goals.Add(new GoalItem
            {
                Id = goal.Id,
                Title = goal.Title,
                IsSelected = selectedIds.Contains(goal.Id)
            });
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ValidationMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(SelectedTimeLimitCategory))
        {
            ValidationMessage = "Bitte waehle eine Kategorie fuer deine erste Zeitlimit-Regel aus.";
            return;
        }

        if (!int.TryParse(TimeLimitMinutesText, out var timeLimitMinutes) ||
            timeLimitMinutes <= 0)
        {
            ValidationMessage = "Bitte gib ein gueltiges Zeitlimit in Minuten ein.";
            return;
        }

        var selectedGoalIds = Goals
            .Where(goal => goal.IsSelected)
            .Select(goal => goal.Id)
            .ToList();

        await goalsRepository.SaveSelectedGoalsAsync(selectedGoalIds);
        await onboardingRepository.SaveGoalOrientationAsync(
            selectedGoalIds.Count > 0 ? string.Join(",", selectedGoalIds) : "none");
        await onboardingRepository.SaveMovementReminderIntervalMinutesAsync(
            SelectedMovementReminderInterval);
        await timeLimitRuleRepository.SaveInitialTimeLimitRuleAsync(
            SelectedTimeLimitCategory,
            timeLimitMinutes);

        await Shell.Current.GoToAsync(nameof(OnboardingStep2Page));
    }
}

public partial class GoalItem : ObservableObject
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    [ObservableProperty]
    private bool isSelected;
}
