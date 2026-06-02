using LightStickyNote.App.Models;
using LightStickyNote.App.Services;

namespace LightStickyNote.App.Tests;

public sealed class ReminderPresentationTests
{
    [Fact]
    public void ReminderDisplayFormatter_returns_today_label_for_same_day_deadline()
    {
        var now = new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.FromHours(8));
        var reminderAt = new DateTimeOffset(2026, 6, 1, 18, 0, 0, TimeSpan.FromHours(8));

        var text = ReminderDisplayFormatter.FormatBadge(reminderAt, now);

        Assert.Equal("今天 18:00", text);
    }

    [Fact]
    public void ReminderDisplayFormatter_returns_tomorrow_label_for_next_day_deadline()
    {
        var now = new DateTimeOffset(2026, 6, 1, 22, 0, 0, TimeSpan.FromHours(8));
        var reminderAt = new DateTimeOffset(2026, 6, 2, 9, 0, 0, TimeSpan.FromHours(8));

        var text = ReminderDisplayFormatter.FormatBadge(reminderAt, now);

        Assert.Equal("明天 09:00", text);
    }

    [Fact]
    public void ReminderDisplayFormatter_returns_overdue_label_for_past_due_deadline()
    {
        var now = new DateTimeOffset(2026, 6, 1, 18, 31, 0, TimeSpan.FromHours(8));
        var reminderAt = new DateTimeOffset(2026, 6, 1, 18, 0, 0, TimeSpan.FromHours(8));

        var text = ReminderDisplayFormatter.FormatBadge(reminderAt, now);

        Assert.Equal("已逾期", text);
    }

    [Fact]
    public void ReminderScheduler_returns_only_due_unfinished_unnotified_items()
    {
        var now = new DateTimeOffset(2026, 6, 1, 18, 0, 0, TimeSpan.FromHours(8));
        var note = new Note { Id = "note-1" };
        var due = new NoteItem { NoteId = note.Id, Text = "due", ReminderAt = now.AddMinutes(-1) };
        var done = new NoteItem { NoteId = note.Id, Text = "done", ReminderAt = now.AddMinutes(-1), IsDone = true };
        var alreadyNotified = new NoteItem
        {
            NoteId = note.Id,
            Text = "notified",
            ReminderAt = now.AddMinutes(-1),
            ReminderNotifiedAt = now.AddMinutes(-1)
        };
        var future = new NoteItem { NoteId = note.Id, Text = "future", ReminderAt = now.AddMinutes(30) };

        var dueItems = ReminderScheduler.GetDueItems([due, done, alreadyNotified, future], now).ToList();

        var only = Assert.Single(dueItems);
        Assert.Equal("due", only.Text);
    }
}
