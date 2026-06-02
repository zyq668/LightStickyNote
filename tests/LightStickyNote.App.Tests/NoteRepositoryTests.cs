using LightStickyNote.App.Data;
using LightStickyNote.App.Models;
using LightStickyNote.App.Repositories;

namespace LightStickyNote.App.Tests;

public sealed class NoteRepositoryTests : IDisposable
{
    private readonly string _rootDirectory;

    public NoteRepositoryTests()
    {
        _rootDirectory = Path.Combine(
            "D:\\CodexProjects\\LightStickyNote",
            "tests",
            "LightStickyNote.App.Tests",
            "test-output",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_rootDirectory);
    }

    [Fact]
    public async Task SaveNoteAsync_PersistsItemsAndWindowPlacement()
    {
        var databasePath = Path.Combine(_rootDirectory, "repo.db");
        var initializer = new DatabaseInitializer(databasePath);
        await initializer.InitializeAsync();

        var repository = new NoteRepository(databasePath);
        var note = await repository.GetOrCreatePrimaryNoteAsync();
        note.PositionX = 120;
        note.PositionY = 80;
        note.Width = 380;
        note.Height = 620;
        note.IsPinned = true;
        note.Items =
        [
            new NoteItem
            {
                NoteId = note.Id,
                Text = "first task",
                SortOrder = 0
            },
            new NoteItem
            {
                NoteId = note.Id,
                Text = "done task",
                SortOrder = 1,
                IsDone = true,
                CompletedAt = DateTimeOffset.UtcNow
            }
        ];

        await repository.SaveNoteAsync(note);
        var reloaded = await repository.GetOrCreatePrimaryNoteAsync();

        Assert.Equal(120, reloaded.PositionX);
        Assert.Equal(80, reloaded.PositionY);
        Assert.Equal(2, reloaded.Items.Count);
        Assert.Equal("done task", reloaded.Items[1].Text);
        Assert.True(reloaded.Items[1].IsDone);
    }

    [Fact]
    public async Task SaveNoteAsync_PersistsReminderMetadata()
    {
        var databasePath = Path.Combine(_rootDirectory, "reminders.db");
        var initializer = new DatabaseInitializer(databasePath);
        await initializer.InitializeAsync();

        var repository = new NoteRepository(databasePath);
        var note = await repository.GetOrCreatePrimaryNoteAsync();
        var reminderAt = new DateTimeOffset(2026, 6, 1, 18, 30, 0, TimeSpan.FromHours(8));
        var reminderNotifiedAt = reminderAt.AddMinutes(5);
        note.Items =
        [
            new NoteItem
            {
                NoteId = note.Id,
                Text = "pay invoice",
                SortOrder = 0,
                ReminderAt = reminderAt,
                ReminderNotifiedAt = reminderNotifiedAt
            }
        ];

        await repository.SaveNoteAsync(note);
        var reloaded = await repository.GetOrCreatePrimaryNoteAsync();
        var item = Assert.Single(reloaded.Items);

        Assert.Equal(reminderAt, item.ReminderAt);
        Assert.Equal(reminderNotifiedAt, item.ReminderNotifiedAt);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_rootDirectory))
            {
                Directory.Delete(_rootDirectory, recursive: true);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
