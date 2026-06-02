using LightStickyNote.App.Models;

namespace LightStickyNote.App.Services;

public static class ReminderScheduler
{
    public static IEnumerable<NoteItem> GetDueItems(IEnumerable<NoteItem> items, DateTimeOffset now)
    {
        return items.Where(item =>
            item.ReminderAt is { } reminderAt &&
            reminderAt <= now &&
            item.ReminderNotifiedAt is null &&
            !item.IsDone);
    }
}
