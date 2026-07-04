using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReturnToMonkee.Models;
using ReturnToMonkee.Services;
using System.Collections.ObjectModel;

namespace ReturnToMonkee.Features.Statistics;

public partial class StatisticsViewModel : ObservableObject
{
    private readonly IStatisticsService statisticsService;

    [ObservableProperty]
    private DailyStatistics todayStatistics = new();

    [ObservableProperty]
    private ObservableCollection<DayStatisticsItem> sevenDayTrend = new();

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public StatisticsViewModel(IStatisticsService statisticsService)
    {
        this.statisticsService = statisticsService;
    }

    [RelayCommand]
    public async Task LoadStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            // Heute laden
            var todayStats = await statisticsService.GetDailyStatisticsAsync(
                DateTime.Today,
                cancellationToken);

            TodayStatistics = todayStats;

            // 7-Tage-Trend laden
            var trendStats = await statisticsService.GetSevenDayTrendAsync(cancellationToken);

            SevenDayTrend.Clear();
            foreach (var stat in trendStats)
            {
                SevenDayTrend.Add(new DayStatisticsItem(stat));
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Fehler beim Laden der Statistiken: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

/// <summary>
/// UI-Item für 7-Tage-Trend mit zusätzlichen Display-Properties.
/// </summary>
public partial class DayStatisticsItem : ObservableObject
{
    private readonly DailyStatistics statistics;

    public DayStatisticsItem(DailyStatistics statistics)
    {
        this.statistics = statistics;
    }

    public DateTime Date => statistics.Date;
    public string DayName => statistics.Date.ToString("ddd");
    public string DateFormatted => statistics.Date.ToString("d.M.");

    public int LimitsKept => statistics.LimitsKept;
    public int LimitsExceeded => statistics.LimitsExceeded;
    public int TotalReminders => statistics.TotalReminders;
    public double ReminderConfirmationRate => statistics.ReminderConfirmationRate;
    public double LimitKeptRate => statistics.LimitKeptRate;

    // Anzeigetexte
    public string LimitsKeptDisplay => $"{LimitsKept}";
    public string LimitsExceededDisplay => $"{LimitsExceeded}";
    public string ReminderRateDisplay => $"{ReminderConfirmationRate:F0}%";
    public string LimitRateDisplay => $"{LimitKeptRate:F0}%";
}