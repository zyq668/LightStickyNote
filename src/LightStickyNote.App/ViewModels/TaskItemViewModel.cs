using LightStickyNote.App.Infrastructure;
using LightStickyNote.App.Models;

namespace LightStickyNote.App.ViewModels;

public sealed class TaskItemViewModel : ObservableObject
{
    private readonly NoteItem _model;
    private string _text;
    private bool _isDone;

    public TaskItemViewModel(NoteItem model)
    {
        _model = model;
        _text = model.Text;
        _isDone = model.IsDone;
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
            Touch();
        }
    }

    public DateTimeOffset CreatedAt => _model.CreatedAt;

    public NoteItem ToModel(int sortOrder)
    {
        _model.SortOrder = sortOrder;
        _model.Text = _text;
        _model.IsDone = _isDone;
        return _model;
    }

    private void Touch()
    {
        _model.UpdatedAt = DateTimeOffset.UtcNow;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
