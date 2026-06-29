using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReturnToMonkee.Infrastructure.Persistence.Entities;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;
using System.Collections.ObjectModel;

namespace ReturnToMonkee.Onboarding;

public partial class GoalOrientationViewModel : ObservableObject
{
    private readonly IGoalsRepository goalsRepository;
    private readonly IOnboardingRepository onboardingRepository;

    public ObservableCollection<GoalItem> Goals { get; } = new();
    public ObservableCollection<int> MovementReminderIntervals { get; } = new() { 30, 60, 90 };

    [ObservableProperty]
    private int selectedMovementReminderInterval = 60;

    public GoalOrientationViewModel(
        IGoalsRepository goalsRepository,
        IOnboardingRepository onboardingRepository)
    {
        this.goalsRepository = goalsRepository;
        this.onboardingRepository = onboardingRepository;
    }

    public async Task LoadAsync()
    {
        await goalsRepository.SeedAsync();

        var goals = await goalsRepository.GetAllGoalsAsync();
        var selectedIds = await goalsRepository.GetSelectedGoalIdsAsync();
        SelectedMovementReminderInterval =
            await onboardingRepository.GetMovementReminderIntervalMinutesAsync();

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
        var selected = Goals
            .Where(x => x.IsSelected)
            .Select(x => x.Id)
            .ToList();

        await goalsRepository.SaveSelectedGoalsAsync(selected);
        await onboardingRepository.SaveGoalOrientationAsync(
            selected.Count > 0 ? string.Join(",", selected) : "none");
        await onboardingRepository.SaveMovementReminderIntervalMinutesAsync(
            SelectedMovementReminderInterval);

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
