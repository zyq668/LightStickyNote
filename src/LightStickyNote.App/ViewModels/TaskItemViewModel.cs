using LightStickyNote.App.Infrastructure;
using LightStickyNote.App.Models;
using LightStickyNote.App.Services;

namespace LightStickyNote.App.ViewModels;

public sealed class TaskItemViewModel : ObservableObject
{
    private readonly NoteItem _model;
    private string _text;
    private bool _isDone;
    private string _reminderDisplayText = string.Empty;
    private bool _isReminderOverdue;

    public TaskItemViewModel(NoteItem model)
    {
        _model = model;
        _text = model.Text;
        _isDone = model.IsDone;
        RefreshReminderDisplay(DateTimeOffset.Now);
    }

    public event EventHandler? Changed;

    public string Id => _model.Id;

    public string Text
    {
        get => _text;
        set
        {
            if (!SetProperty(ref _text, value))
            {
                return;
            }

            _model.Text = value;
            Touch();
        }
    }

    public bool IsDone
    {
        get => _isDone;
        set
        {
            if (!SetProperty(ref _isDone, value))
            {
                return;
            }

            _model.IsDone = value;
            _model.CompletedAt = value ? DateTimeOffset.UtcNow : null;
            if (value)
            {
                _model.ReminderAt = null;
                _model.ReminderNotifiedAt = null;
                OnPropertyChanged(nameof(HasReminder));
                OnPropertyChanged(nameof(ReminderMenuText));
            }

            RefreshReminderDisplay(DateTimeOffset.Now);
            Touch();
        }
    }

    public DateTimeOffset CreatedAt => _model.CreatedAt;

    public DateTimeOffset? ReminderAt => _model.ReminderAt;

    public DateTimeOffset? ReminderNotifiedAt => _model.ReminderNotifiedAt;

    public bool HasReminder => _model.ReminderAt is not null;

    public string ReminderDisplayText
    {
        get => _reminderDisplayText;
        private set => SetProperty(ref _reminderDisplayText, value);
    }

    public bool IsReminderOverdue
    {
        get => _isReminderOverdue;
        private set => SetProperty(ref _isReminderOverdue, value);
    }

    public string ReminderMenuText => HasReminder ? "编辑提醒" : "设置提醒";

    public NoteItem ToModel(int sortOrder)
    {
        _model.SortOrder = sortOrder;
        _model.Text = _text;
        _model.IsDone = _isDone;
        return _model;
    }

    public void SetReminder(DateTimeOffset reminderAt)
    {
        _model.ReminderAt = reminderAt;
        _model.ReminderNotifiedAt = null;
        RefreshReminderDisplay(DateTimeOffset.Now);
        OnPropertyChanged(nameof(HasReminder));
        OnPropertyChanged(nameof(ReminderMenuText));
        Touch();
    }

    public void ClearReminder()
    {
        if (_model.ReminderAt is null && _model.ReminderNotifiedAt is null)
        {
            return;
        }

        _model.ReminderAt = null;
        _model.ReminderNotifiedAt = null;
        RefreshReminderDisplay(DateTimeOffset.Now);
        OnPropertyChanged(nameof(HasReminder));
        OnPropertyChanged(nameof(ReminderMenuText));
        Touch();
    }

    public void MarkReminderNotified(DateTimeOffset notifiedAt)
    {
        _model.ReminderNotifiedAt = notifiedAt;
        RefreshReminderDisplay(notifiedAt);
        Touch();
    }

    public void RefreshReminderDisplay(DateTimeOffset now)
    {
        IsReminderOverdue = _model.ReminderAt is { } dueAt && dueAt <= now;
        ReminderDisplayText = _model.ReminderAt is { } reminderAt
            ? ReminderDisplayFormatter.FormatBadge(reminderAt, now)
            : string.Empty;
    }

    private void Touch()
    {
        _model.UpdatedAt = DateTimeOffset.UtcNow;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
