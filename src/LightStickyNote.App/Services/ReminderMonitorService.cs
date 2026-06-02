using System.Windows.Threading;
using LightStickyNote.App.ViewModels;

namespace LightStickyNote.App.Services;

public sealed class ReminderMonitorService : IDisposable
{
    private readonly MainViewModel _viewModel;
    private readonly TrayIconService _trayIconService;
    private readonly DispatcherTimer _timer;

    public ReminderMonitorService(MainViewModel viewModel, TrayIconService trayIconService)
    {
        _viewModel = viewModel;
        _trayIconService = trayIconService;
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(20)
        };
        _timer.Tick += OnTick;
    }

    public void Start()
    {
        CheckDueReminders();
        _timer.Start();
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= OnTick;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        CheckDueReminders();
    }

    private void CheckDueReminders()
    {
        var now = DateTimeOffset.Now;
        _viewModel.RefreshReminderBadges(now);

        foreach (var item in _viewModel.GetDueReminderItems(now))
        {
            _trayIconService.ShowReminder(item.Text);
            item.MarkReminderNotified(now);
        }
    }
}
