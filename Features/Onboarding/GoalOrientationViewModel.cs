using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReturnToMonkee.Features.Onboarding;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;
using System.Collections.ObjectModel;

namespace ReturnToMonkee.Onboarding;

public partial class GoalOrientationViewModel : ObservableObject
{
    private const int WelcomeStep = 0;
    private const int GoalsStep = 1;
    private const int MovementStep = 2;
    private const int TimeLimitStep = 3;
    private const int SleepStep = 4;
    private const int CompletedStep = 5;
    private const int SetupStepCount = 4;

    private readonly IGoalsRepository goalsRepository;
    private readonly IOnboardingRepository onboardingRepository;
    private readonly ITimeLimitRuleRepository timeLimitRuleRepository;
    private readonly IUserSettingsRepository userSettingsRepository;

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

    private int currentStepIndex;

    [ObservableProperty]
    private int selectedMovementReminderInterval = 60;

    [ObservableProperty]
    private string selectedTimeLimitCategory = "Social Media";

    [ObservableProperty]
    private string timeLimitMinutesText = "30";

    private TimeSpan sleepTime = TimeSpan.FromHours(22);

    [ObservableProperty]
    private string validationMessage = string.Empty;

    public GoalOrientationViewModel(
        IGoalsRepository goalsRepository,
        IOnboardingRepository onboardingRepository,
        ITimeLimitRuleRepository timeLimitRuleRepository,
        IUserSettingsRepository userSettingsRepository)
    {
        this.goalsRepository = goalsRepository;
        this.onboardingRepository = onboardingRepository;
        this.timeLimitRuleRepository = timeLimitRuleRepository;
        this.userSettingsRepository = userSettingsRepository;
    }

    public bool IsWelcomeStep => CurrentStepIndex == WelcomeStep;
    public bool IsGoalsStep => CurrentStepIndex == GoalsStep;
    public bool IsMovementStep => CurrentStepIndex == MovementStep;
    public bool IsTimeLimitStep => CurrentStepIndex == TimeLimitStep;
    public bool IsSleepStep => CurrentStepIndex == SleepStep;
    public bool IsCompletedStep => CurrentStepIndex == CompletedStep;
    public bool IsBackVisible => CurrentStepIndex is > WelcomeStep and < CompletedStep;
    public bool IsProgressVisible => CurrentStepIndex != WelcomeStep;

    public double StepProgress =>
        CurrentStepIndex switch
        {
            WelcomeStep => 0,
            CompletedStep => 1,
            _ => Math.Clamp((double)CurrentStepIndex / SetupStepCount, 0, 1)
        };

    public string StepCounterText =>
        CurrentStepIndex switch
        {
            WelcomeStep => $"Bereit für Schritt 1 von {SetupStepCount}",
            CompletedStep => "Onboarding abgeschlossen",
            _ => $"Schritt {CurrentStepIndex} von {SetupStepCount}"
        };

    public string NextButtonText =>
        CurrentStepIndex switch
        {
            WelcomeStep => "Einrichtung starten",
            SleepStep => "Onboarding abschließen",
            CompletedStep => "Zum Dashboard",
            _ => "Weiter"
        };

    public int CurrentStepIndex
    {
        get => currentStepIndex;
        set
        {
            if (!SetProperty(ref currentStepIndex, value))
            {
                return;
            }

            OnPropertyChanged(nameof(IsWelcomeStep));
            OnPropertyChanged(nameof(IsGoalsStep));
            OnPropertyChanged(nameof(IsMovementStep));
            OnPropertyChanged(nameof(IsTimeLimitStep));
            OnPropertyChanged(nameof(IsSleepStep));
            OnPropertyChanged(nameof(IsCompletedStep));
            OnPropertyChanged(nameof(IsBackVisible));
            OnPropertyChanged(nameof(IsProgressVisible));
            OnPropertyChanged(nameof(StepProgress));
            OnPropertyChanged(nameof(StepCounterText));
            OnPropertyChanged(nameof(NextButtonText));
        }
    }

    public TimeSpan SleepTime
    {
        get => sleepTime;
        set => SetProperty(ref sleepTime, value);
    }

    public async Task LoadAsync()
    {
        await goalsRepository.SeedAsync();

        var goals = await goalsRepository.GetAllGoalsAsync();
        var selectedIds = await goalsRepository.GetSelectedGoalIdsAsync();
        var settings = await userSettingsRepository.GetAsync();

        SelectedMovementReminderInterval =
            await onboardingRepository.GetMovementReminderIntervalMinutesAsync();
        SleepTime = settings.SleepTime;

        Goals.Clear();

        foreach (var goal in goals)
        {
            Goals.Add(new GoalItem
            {
                Id = goal.Id,
                Title = goal.Title,
                Icon = ResolveGoalIcon(goal.Title),
                IsSelected = selectedIds.Contains(goal.Id)
            });
        }
    }

    private static string ResolveGoalIcon(string title) =>
        title switch
        {
            "Weniger Social Media" => char.ConvertFromUtf32(0xF3CF), // IconMobileScreen
            "Mehr Fokus" => char.ConvertFromUtf32(0xF140),           // IconBullseye
            "Besser schlafen" => char.ConvertFromUtf32(0xF186),      // IconMoon
            "Mehr Energie" => char.ConvertFromUtf32(0xF4D8),         // IconSeedling
            _ => char.ConvertFromUtf32(0xF140)                       // IconBullseye fallback
        };

    [RelayCommand]
    private static void ToggleGoal(GoalItem? goal)
    {
        if (goal is not null)
        {
            goal.IsSelected = !goal.IsSelected;
        }
    }

    [RelayCommand]
    private async Task NextAsync()
    {
        ValidationMessage = string.Empty;

        if (CurrentStepIndex == CompletedStep)
        {
            await Shell.Current.GoToAsync("//home");
            return;
        }

        if (CurrentStepIndex == TimeLimitStep && !TryReadTimeLimitMinutes(out _))
        {
            return;
        }

        if (CurrentStepIndex == SleepStep)
        {
            if (await SaveOnboardingAsync())
            {
                CurrentStepIndex = CompletedStep;
            }

            return;
        }

        CurrentStepIndex++;
    }

    [RelayCommand]
    private void Back()
    {
        ValidationMessage = string.Empty;

        if (CurrentStepIndex is > WelcomeStep and < CompletedStep)
        {
            CurrentStepIndex--;
        }
    }

    private async Task<bool> SaveOnboardingAsync()
    {
        if (!TryReadTimeLimitMinutes(out var timeLimitMinutes))
        {
            CurrentStepIndex = TimeLimitStep;
            return false;
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

        var settings = await userSettingsRepository.GetAsync();
        settings.SleepTime = SleepTime;
        settings.ShowOnboardingOnStartup = false;
        await userSettingsRepository.SaveAsync(settings);

        await onboardingRepository.SaveGoalOrientationAsync("completed");
        return true;
    }

    private bool TryReadTimeLimitMinutes(out int timeLimitMinutes)
    {
        timeLimitMinutes = 0;

        if (string.IsNullOrWhiteSpace(SelectedTimeLimitCategory))
        {
            ValidationMessage = "Bitte wähle eine Kategorie für deine erste Zeitlimit-Regel aus.";
            return false;
        }

        if (!int.TryParse(TimeLimitMinutesText, out timeLimitMinutes) ||
            timeLimitMinutes <= 0)
        {
            ValidationMessage = "Bitte gib ein gültiges Zeitlimit in Minuten ein.";
            return false;
        }

        return true;
    }
}

public partial class GoalItem : ObservableObject
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    [ObservableProperty]
    private bool isSelected;
}
