namespace LightStickyNote.App.Models;

public sealed class Note
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Title { get; set; } = "今日任务";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ArchivedAt { get; set; }

    public double? PositionX { get; set; }

    public double? PositionY { get; set; }

    public double Width { get; set; } = 360;

    public double Height { get; set; } = 640;

    public bool IsPinned { get; set; } = true;

    public List<NoteItem> Items { get; set; } = [];
}
