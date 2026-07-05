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

    private readonly ObservableCollection<DashboardRuleItem> activeRules = new();

    public MainPage(
        ILocalDatabase localDatabase,
        ILogger<MainPage> logger,
        IStatisticsService statisticsService,
        IUserSettingsRepository userSettingsRepository,
        IOnboardingRepository onboardingRepository,
        IReminderService reminderService)
    {
        InitializeComponent();

        this.localDatabase = localDatabase;
        this.logger = logger;
        this.statisticsService = statisticsService;
        this.userSettingsRepository = userSettingsRepository;
        this.onboardingRepository = onboardingRepository;
        this.reminderService = reminderService;

        ActiveRulesCollection.ItemsSource = activeRules;
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

        SleepReminderLabel.Text = userSettings.SleepReminderEnabled
            ? $"Schlafenszeit: {sleepTime:hh\\:mm} Uhr · nächster Reminder: {FormatReminderDate(nextSleepReminder, now)}"
            : $"Schlafenszeit: {sleepTime:hh\\:mm} Uhr · Reminder deaktiviert";

        var movementIntervalMinutes =
            await onboardingRepository.GetMovementReminderIntervalMinutesAsync();
        var movementReminderEnabled =
            await onboardingRepository.GetMovementReminderEnabledAsync();

        // Echten naechsten Zeitpunkt vom ReminderService holen (letzter Reminder + Intervall),
        // statt faelschlich immer "jetzt + Intervall" anzuzeigen.
        var nextMovementReminder = await reminderService.GetNextMovementReminderTimeAsync();

        MovementReminderLabel.Text = movementReminderEnabled
            ? $"Bewegung: alle {movementIntervalMinutes} Minuten · nächster Reminder {FormatReminderDate(nextMovementReminder, now)}"
            : $"Bewegung: alle {movementIntervalMinutes} Minuten · Reminder deaktiviert";
    }

    private async Task LoadTodayStatisticsAsync()
    {
        var todayStatistics =
            await statisticsService.GetDailyStatisticsAsync(DateTime.Today);

        TodaySummaryLabel.Text =
            $"Heute: {todayStatistics.TotalReminders} Reminder, {todayStatistics.LimitsExceeded} Zeitlimit-Überschreitung(en)";

        ReminderStatsLabel.Text =
            $"Reminder bestätigt: {todayStatistics.MovementRemindersConfirmed + todayStatistics.SleepRemindersConfirmed} · " +
            $"ignoriert: {todayStatistics.MovementRemindersIgnored + todayStatistics.SleepRemindersIgnored} · " +
            $"Bestätigungsrate: {todayStatistics.ReminderConfirmationRate:F0}%";

        LimitStatsLabel.Text =
            $"Zeitlimits eingehalten: {todayStatistics.LimitsKept} · " +
            $"überschritten: {todayStatistics.LimitsExceeded} · " +
            $"Einhaltungsrate: {todayStatistics.LimitKeptRate:F0}%";
    }

    private static string FormatReminderDate(DateTime reminderTime, DateTime now)
    {
        if (reminderTime.Date == now.Date)
        {
            return $"heute um {reminderTime:HH:mm} Uhr";
        }

        if (reminderTime.Date == now.Date.AddDays(1))
        {
            return $"morgen um {reminderTime:HH:mm} Uhr";
        }

        return reminderTime.ToString("dd.MM.yyyy HH:mm");
    }

    private sealed class DashboardRuleItem
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Kind { get; set; } = string.Empty;
    }
}
