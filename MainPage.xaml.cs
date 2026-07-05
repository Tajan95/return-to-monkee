using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using ReturnToMonkee.Infrastructure.Persistence;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;
using ReturnToMonkee.Services;

namespace ReturnToMonkee;

public partial class MainPage : ContentPage
{
    private readonly ILocalDatabase localDatabase;
    private readonly ILogger<MainPage> logger;
    private readonly IStatisticsService statisticsService;
    private readonly IUserSettingsRepository userSettingsRepository;
    private readonly IOnboardingRepository onboardingRepository;
    private readonly IReminderService reminderService;
    private readonly IGoalsRepository goalsRepository;

    private readonly ObservableCollection<DashboardRuleItem> activeRules = new();
    private readonly ObservableCollection<DashboardGoalItem> selectedGoals = new();

    public MainPage(
        ILocalDatabase localDatabase,
        ILogger<MainPage> logger,
        IStatisticsService statisticsService,
        IUserSettingsRepository userSettingsRepository,
        IOnboardingRepository onboardingRepository,
        IReminderService reminderService,
        IGoalsRepository goalsRepository)
    {
        InitializeComponent();

        this.localDatabase = localDatabase;
        this.logger = logger;
        this.statisticsService = statisticsService;
        this.userSettingsRepository = userSettingsRepository;
        this.onboardingRepository = onboardingRepository;
        this.reminderService = reminderService;
        this.goalsRepository = goalsRepository;

        ActiveRulesCollection.ItemsSource = activeRules;
        GoalsCollection.ItemsSource = selectedGoals;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshDashboardAsync();
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await RefreshDashboardAsync();
    }

    private async Task RefreshDashboardAsync()
    {
        try
        {
            RefreshButton.IsEnabled = false;
            ErrorLabel.IsVisible = false;
            ErrorLabel.Text = string.Empty;

            await LoadSelectedGoalsAsync();
            await LoadActiveRulesAsync();
            await LoadReminderStatusAsync();
            await LoadTodayStatisticsAsync();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to refresh dashboard");
            ErrorLabel.Text = $"Dashboard konnte nicht geladen werden: {exception.Message}";
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            RefreshButton.IsEnabled = true;
        }
    }

    private async Task LoadSelectedGoalsAsync()
    {
        var allGoals = await goalsRepository.GetAllGoalsAsync();
        var selectedIds = await goalsRepository.GetSelectedGoalIdsAsync();

        selectedGoals.Clear();

        foreach (var goal in allGoals.Where(goal => selectedIds.Contains(goal.Id)))
        {
            selectedGoals.Add(new DashboardGoalItem
            {
                Title = goal.Title,
                Glyph = ResolveGoalIcon(goal.Title)
            });
        }

        GoalsEmptyLabel.IsVisible = selectedGoals.Count == 0;
        GoalsCollection.IsVisible = selectedGoals.Count > 0;
    }

    // Gleiche Zuordnung Ziel-Titel -> FontAwesome-Glyph wie im Onboarding (GoalOrientationViewModel).
    private static string ResolveGoalIcon(string title) =>
        title switch
        {
            "Weniger Social Media" => char.ConvertFromUtf32(0xF3CF), // IconMobileScreen
            "Mehr Fokus" => char.ConvertFromUtf32(0xF140),           // IconBullseye
            "Besser schlafen" => char.ConvertFromUtf32(0xF186),      // IconMoon
            "Mehr Energie" => char.ConvertFromUtf32(0xF4D8),         // IconSeedling
            _ => char.ConvertFromUtf32(0xF140)                       // IconBullseye fallback
        };

    private async Task LoadActiveRulesAsync()
    {
        var connection = await localDatabase.GetConnectionAsync();

        await connection.CreateTableAsync<global::TimeLimitRule>();

        var timeLimitRules = await connection.Table<global::TimeLimitRule>()
            .ToListAsync();

        activeRules.Clear();

        foreach (var rule in timeLimitRules.Where(rule => rule.IsEnabled))
        {
            activeRules.Add(new DashboardRuleItem
            {
                Title = string.IsNullOrWhiteSpace(rule.Title)
                    ? $"{rule.TargetApplication} begrenzen"
                    : rule.Title,
                Description = $"{rule.TargetApplication}: {rule.TimeLimitMinutes} Minuten pro Tag",
                Kind = "Zeitlimit"
            });
        }

        ActiveRulesEmptyLabel.IsVisible = activeRules.Count == 0;
        ActiveRulesCollection.IsVisible = activeRules.Count > 0;
    }

    private async Task LoadReminderStatusAsync()
    {
        var now = DateTime.Now;

        var userSettings = await userSettingsRepository.GetAsync();
        var sleepTime = userSettings.SleepTime;
        // Naechsten Reminder-Zeitpunkt inkl. Vorlauf zentral vom ReminderService holen.
        var nextSleepReminder = await reminderService.GetNextSleepReminderTimeAsync();

        SleepReminderDetailLabel.Text = userSettings.SleepReminderLeadMinutes > 0
            ? $"Zu Bett um {sleepTime:hh\\:mm} Uhr · {userSettings.SleepReminderLeadMinutes} Min Vorlauf"
            : $"Zu Bett um {sleepTime:hh\\:mm} Uhr";
        SetReminderBadge(
            SleepReminderBadge,
            SleepReminderBadgeLabel,
            userSettings.SleepReminderEnabled,
            FormatReminderBadge(nextSleepReminder, now));

        var movementIntervalMinutes =
            await onboardingRepository.GetMovementReminderIntervalMinutesAsync();
        var movementReminderEnabled =
            await onboardingRepository.GetMovementReminderEnabledAsync();

        // Echten naechsten Zeitpunkt vom ReminderService holen (letzter Reminder + Intervall),
        // statt faelschlich immer "jetzt + Intervall" anzuzeigen.
        var nextMovementReminder = await reminderService.GetNextMovementReminderTimeAsync();

        MovementReminderDetailLabel.Text = $"Alle {movementIntervalMinutes} Minuten";
        SetReminderBadge(
            MovementReminderBadge,
            MovementReminderBadgeLabel,
            movementReminderEnabled,
            FormatReminderBadge(nextMovementReminder, now));
    }

    private async Task LoadTodayStatisticsAsync()
    {
        var todayStatistics =
            await statisticsService.GetDailyStatisticsAsync(DateTime.Today);

        StatReminderConfirmed.Text =
            (todayStatistics.MovementRemindersConfirmed + todayStatistics.SleepRemindersConfirmed).ToString();
        StatReminderIgnored.Text =
            (todayStatistics.MovementRemindersIgnored + todayStatistics.SleepRemindersIgnored).ToString();
        StatReminderRate.Text = $"{todayStatistics.ReminderConfirmationRate:F0}%";

        StatLimitsKept.Text = todayStatistics.LimitsKept.ToString();
        StatLimitsExceeded.Text = todayStatistics.LimitsExceeded.ToString();
        StatLimitRate.Text = $"{todayStatistics.LimitKeptRate:F0}%";
    }

    // Kompaktes Badge-Format fuer den naechsten Reminder-Zeitpunkt.
    private static string FormatReminderBadge(DateTime reminderTime, DateTime now)
    {
        if (reminderTime.Date == now.Date)
        {
            return $"heute {reminderTime:HH:mm}";
        }

        if (reminderTime.Date == now.Date.AddDays(1))
        {
            return $"morgen {reminderTime:HH:mm}";
        }

        return reminderTime.ToString("dd.MM. HH:mm");
    }

    // Aktiver Reminder: Accent-Pille mit Zeit. Deaktiviert: neutrale "Aus"-Pille.
    private static void SetReminderBadge(Border badge, Label label, bool enabled, string time)
    {
        if (enabled)
        {
            badge.BackgroundColor = GetColor("PrimaryDark");
            label.TextColor = GetColor("OffBlack");
            label.Text = time;
        }
        else
        {
            badge.BackgroundColor = GetColor("Gray600");
            label.TextColor = GetColor("Gray200");
            label.Text = "Aus";
        }
    }

    private static Color GetColor(string key) =>
        Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color
            ? color
            : Colors.Transparent;

    private sealed class DashboardRuleItem
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Kind { get; set; } = string.Empty;
    }

    private sealed class DashboardGoalItem
    {
        public string Title { get; set; } = string.Empty;

        public string Glyph { get; set; } = string.Empty;
    }
}
