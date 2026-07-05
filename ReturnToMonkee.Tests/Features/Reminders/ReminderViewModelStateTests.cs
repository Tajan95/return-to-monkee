using ReturnToMonkee.Features.Reminders;
using ReturnToMonkee.Infrastructure.Persistence.Entities;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;
using ReturnToMonkee.Services;

namespace ReturnToMonkee.Tests.Features.Reminders;

public sealed class ReminderViewModelStateTests
{
    [Fact]
    public async Task MovementReminder_DisablesSaveAndTest_WhenToggleIsOff()
    {
        var repository = new StubOnboardingRepository
        {
            MovementReminderEnabled = true,
            MovementReminderIntervalMinutes = 60
        };
        var viewModel = new MovementReminderViewModel(repository, new StubReminderService());

        await viewModel.LoadAsync();

        Assert.False(viewModel.SaveCommand.CanExecute(null));
        Assert.True(viewModel.TestCommand.CanExecute(null));

        viewModel.IsMovementReminderEnabled = false;

        await WaitUntilAsync(() => repository.MovementReminderEnabled == false);
        Assert.False(viewModel.SaveCommand.CanExecute(null));
        Assert.False(viewModel.TestCommand.CanExecute(null));
    }

    [Fact]
    public async Task MovementReminder_EnablesSaveOnlyWhenEnabledValueChanges()
    {
        var repository = new StubOnboardingRepository
        {
            MovementReminderEnabled = true,
            MovementReminderIntervalMinutes = 60
        };
        var viewModel = new MovementReminderViewModel(repository, new StubReminderService());

        await viewModel.LoadAsync();

        viewModel.SelectedMovementReminderInterval = "90 Minuten";

        Assert.True(viewModel.SaveCommand.CanExecute(null));

        viewModel.IsMovementReminderEnabled = false;

        await WaitUntilAsync(() => repository.MovementReminderEnabled == false);
        Assert.False(viewModel.SaveCommand.CanExecute(null));

        viewModel.IsMovementReminderEnabled = true;

        await WaitUntilAsync(() => repository.MovementReminderEnabled == true);
        Assert.True(viewModel.SaveCommand.CanExecute(null));
    }

    [Fact]
    public async Task SleepReminder_DisablesSaveAndTest_WhenToggleIsOff()
    {
        var repository = new StubUserSettingsRepository
        {
            Settings =
            {
                SleepReminderEnabled = true,
                SleepTime = TimeSpan.FromHours(22)
            }
        };
        var viewModel = new SleepReminderViewModel(repository, new StubReminderService());

        await viewModel.LoadAsync();

        Assert.False(viewModel.SaveCommand.CanExecute(null));
        Assert.True(viewModel.TestCommand.CanExecute(null));

        viewModel.IsSleepReminderEnabled = false;

        await WaitUntilAsync(() => repository.Settings.SleepReminderEnabled == false);
        Assert.False(viewModel.SaveCommand.CanExecute(null));
        Assert.False(viewModel.TestCommand.CanExecute(null));
    }

    [Fact]
    public async Task SleepReminder_EnablesSaveOnlyWhenEnabledTimeChanges()
    {
        var repository = new StubUserSettingsRepository
        {
            Settings =
            {
                SleepReminderEnabled = true,
                SleepTime = TimeSpan.FromHours(22)
            }
        };
        var viewModel = new SleepReminderViewModel(repository, new StubReminderService());

        await viewModel.LoadAsync();

        viewModel.SleepTime = TimeSpan.FromHours(23);

        Assert.True(viewModel.SaveCommand.CanExecute(null));

        viewModel.IsSleepReminderEnabled = false;

        await WaitUntilAsync(() => repository.Settings.SleepReminderEnabled == false);
        Assert.False(viewModel.SaveCommand.CanExecute(null));

        viewModel.IsSleepReminderEnabled = true;

        await WaitUntilAsync(() => repository.Settings.SleepReminderEnabled == true);
        Assert.True(viewModel.SaveCommand.CanExecute(null));
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(10);
        }

        Assert.True(condition());
    }

    private sealed class StubOnboardingRepository : IOnboardingRepository
    {
        public bool MovementReminderEnabled { get; set; } = true;
        public int MovementReminderIntervalMinutes { get; set; } = 60;

        public Task<string?> GetGoalOrientationAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);

        public Task<int> GetMovementReminderIntervalMinutesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(MovementReminderIntervalMinutes);

        public Task<bool> GetMovementReminderEnabledAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(MovementReminderEnabled);

        public Task SaveGoalOrientationAsync(string goalOrientation, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveMovementReminderIntervalMinutesAsync(
            int intervalMinutes,
            CancellationToken cancellationToken = default)
        {
            MovementReminderIntervalMinutes = intervalMinutes;
            return Task.CompletedTask;
        }

        public Task SaveMovementReminderEnabledAsync(bool isEnabled, CancellationToken cancellationToken = default)
        {
            MovementReminderEnabled = isEnabled;
            return Task.CompletedTask;
        }

        public Task<bool> IsOnboardingCompletedAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(true);
    }

    private sealed class StubUserSettingsRepository : IUserSettingsRepository
    {
        public UserSettingsEntity Settings { get; set; } = new();

        public Task<UserSettingsEntity> GetAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Settings);

        public Task SaveAsync(UserSettingsEntity settings, CancellationToken cancellationToken = default)
        {
            Settings = settings;
            return Task.CompletedTask;
        }
    }

    private sealed class StubReminderService : IReminderService
    {
        public Task StartAsync() => Task.CompletedTask;

        public Task StopAsync() => Task.CompletedTask;

        public Task TriggerSleepReminderAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task TriggerMovementReminderAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<DateTime> GetNextMovementReminderTimeAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(DateTime.Now);

        public Task<DateTime> GetNextSleepReminderTimeAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(DateTime.Now);
    }
}
