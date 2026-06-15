using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReturnToMonkee.Infrastructure.Persistence.Entities;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;
using System.Collections.ObjectModel;

namespace ReturnToMonkee.Onboarding;

public partial class GoalOrientationViewModel : ObservableObject
{
    private readonly IGoalsRepository repository;

    public ObservableCollection<GoalItem> Goals { get; } = new();

    public GoalOrientationViewModel(IGoalsRepository repository)
    {
        this.repository = repository;
    }

    public async Task LoadAsync()
    {
        await repository.SeedAsync();

        var goals = await repository.GetAllGoalsAsync();
        var selectedIds = await repository.GetSelectedGoalIdsAsync();

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

        await repository.SaveSelectedGoalsAsync(selected);

        //Todo: anpassen wenn die nächste Onboarding Seite da ist
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