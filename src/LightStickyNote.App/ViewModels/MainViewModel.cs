using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using LightStickyNote.App.Infrastructure;
using LightStickyNote.App.Models;
using LightStickyNote.App.Repositories;
using LightStickyNote.App.Services;

namespace LightStickyNote.App.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly NoteRepository _noteRepository;
    private readonly SettingsService _settingsService;
    private readonly StartupRegistrationService _startupRegistrationService;
    private readonly WindowPlacementService _windowPlacementService;
    private readonly DispatcherTimer _saveTimer;
    private Note _note = new();
    private string _newTaskText = string.Empty;
    private string _statusMessage = "自动保存已开启";
    private bool _isInitialized;
    private bool _isSaving;

    public MainViewModel(
        NoteRepository noteRepository,
        SettingsService settingsService,
        StartupRegistrationService startupRegistrationService,
        AppSettings settings,
        WindowPlacementService windowPlacementService)
    {
        _noteRepository = noteRepository;
        _settingsService = settingsService;
        _startupRegistrationService = startupRegistrationService;
        _windowPlacementService = windowPlacementService;
        Settings = settings;
        Items.CollectionChanged += OnCollectionChanged;
        Settings.PropertyChanged += OnSettingsPropertyChanged;

        DeleteItemCommand = new RelayCommand(
            parameter =>
            {
                if (parameter is TaskItemViewModel item)
                {
                    _ = DeleteItemAsync(item);
                }
            });

        _saveTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _saveTimer.Tick += async (_, _) =>
        {
            _saveTimer.Stop();
            await PersistAsync();
        };
    }

    public ObservableCollection<TaskItemViewModel> Items { get; } = [];

    public RelayCommand DeleteItemCommand { get; }

    public AppSettings Settings { get; }

    public string NoteTitle => _note.Title;

    public int PendingCount => TaskSummary.FromItems(Items).PendingCount;

    public int CompletedCount => TaskSummary.FromItems(Items).CompletedCount;

    public int CompletionPercentage => TaskSummary.FromItems(Items).CompletionPercentage;

    public string FeedbackText => TaskSummary.FromItems(Items).FeedbackText;

    public string NewTaskText
    {
        get => _newTaskText;
        set => SetProperty(ref _newTaskText, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public async Task InitializeAsync()
    {
        _note = await _noteRepository.GetOrCreatePrimaryNoteAsync();
        Settings.LaunchAtStartup = _startupRegistrationService.IsEnabled();

        foreach (var item in _note.Items.OrderBy(x => x.SortOrder))
        {
            AddItemViewModel(item);
        }

        _isInitialized = true;
    }

    public async Task AddTaskFromInputAsync()
    {
        var text = NewTaskText.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var item = new NoteItem
        {
            NoteId = _note.Id,
            Text = text,
            SortOrder = Items.Count
        };

        AddItemViewModel(item);
        NewTaskText = string.Empty;
        await PersistAsync();
    }

    public void ApplyWindowToView(Window window)
    {
        _windowPlacementService.Apply(window, _note, Settings);
    }

    public void CaptureWindowPlacement(Window window)
    {
        _windowPlacementService.Capture(window, _note, Settings);
        QueueSave("窗口位置已保存");
    }

    public async Task FlushPendingChangesAsync()
    {
        _saveTimer.Stop();
        await PersistAsync();
        await _settingsService.SaveAsync(Settings);
    }

    public IEnumerable<TaskItemViewModel> GetDueReminderItems(DateTimeOffset now)
    {
        return Items
            .Where(item =>
                item.ReminderAt is { } reminderAt &&
                reminderAt <= now &&
                item.ReminderNotifiedAt is null &&
                !item.IsDone)
            .ToList();
    }

    public void RefreshReminderBadges(DateTimeOffset now)
    {
        foreach (var item in Items)
        {
            item.RefreshReminderDisplay(now);
        }
    }

    private async Task DeleteItemAsync(TaskItemViewModel item)
    {
        item.Changed -= OnTaskItemChanged;
        Items.Remove(item);
        await PersistAsync();
    }

    private void AddItemViewModel(NoteItem item)
    {
        var vm = new TaskItemViewModel(item);
        vm.Changed += OnTaskItemChanged;
        Items.Add(vm);
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        NotifyTaskSummaryChanged();

        if (_isInitialized)
        {
            QueueSave("任务列表已更新");
        }
    }

    private void OnTaskItemChanged(object? sender, EventArgs e)
    {
        NotifyTaskSummaryChanged();
        QueueSave("内容已自动保存");
    }

    private void NotifyTaskSummaryChanged()
    {
        OnPropertyChanged(nameof(PendingCount));
        OnPropertyChanged(nameof(CompletedCount));
        OnPropertyChanged(nameof(CompletionPercentage));
        OnPropertyChanged(nameof(FeedbackText));
    }

    private void QueueSave(string status)
    {
        if (!_isInitialized)
        {
            return;
        }

        StatusMessage = status;
        _saveTimer.Stop();
        _saveTimer.Start();
    }

    private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!_isInitialized)
        {
            return;
        }

        if (e.PropertyName == nameof(AppSettings.LaunchAtStartup))
        {
            _ = SyncStartupRegistrationAsync();
        }

        QueueSave("设置已保存");
    }

    private async Task PersistAsync()
    {
        if (!_isInitialized || _isSaving)
        {
            return;
        }

        try
        {
            _isSaving = true;
            _note.IsPinned = Settings.AlwaysOnTop;
            _note.Items = Items
                .Select((item, index) => item.ToModel(index))
                .ToList();

            await _noteRepository.SaveNoteAsync(_note);
            await _settingsService.SaveAsync(Settings);
            StatusMessage = $"最近保存: {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"保存失败: {ex.Message}";
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task SyncStartupRegistrationAsync()
    {
        try
        {
            await _startupRegistrationService.SetEnabledAsync(Settings.LaunchAtStartup);
            StatusMessage = Settings.LaunchAtStartup ? "已开启开机自启动" : "已关闭开机自启动";
        }
        catch (Exception ex)
        {
            StatusMessage = $"自启动设置失败: {ex.Message}";
        }
    }
}
