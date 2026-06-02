namespace LightStickyNote.App.Models;

public sealed class NoteItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string NoteId { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public bool IsDone { get; set; }

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset? ReminderAt { get; set; }

    public DateTimeOffset? ReminderNotifiedAt { get; set; }
}
