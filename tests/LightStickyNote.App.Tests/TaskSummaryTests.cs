using LightStickyNote.App.Models;
using LightStickyNote.App.ViewModels;

namespace LightStickyNote.App.Tests;

public sealed class TaskSummaryTests
{
    [Fact]
    public void From_returns_zero_counts_for_an_empty_list()
    {
        var summary = TaskSummary.FromModels([]);

        Assert.Equal(0, summary.PendingCount);
        Assert.Equal(0, summary.CompletedCount);
        Assert.Equal(0, summary.CompletionPercentage);
    }

    [Fact]
    public void From_counts_pending_completed_and_percentage()
    {
        var items = new[]
        {
            new NoteItem { Text = "One", IsDone = true },
            new NoteItem { Text = "Two", IsDone = false },
            new NoteItem { Text = "Three", IsDone = false }
        };

        var summary = TaskSummary.FromModels(items);

        Assert.Equal(2, summary.PendingCount);
        Assert.Equal(1, summary.CompletedCount);
        Assert.Equal(33, summary.CompletionPercentage);
    }

    [Theory]
    [InlineData(0, 0, "今天很轻松")]
    [InlineData(3, 0, "从第一件开始")]
    [InlineData(2, 1, "已经完成 1 件")]
    [InlineData(0, 3, "今天的任务都完成了")]
    public void FeedbackText_matches_the_current_progress(
        int pendingCount,
        int completedCount,
        string expected)
    {
        var summary = new TaskSummary(pendingCount, completedCount, 0);

        Assert.Equal(expected, summary.FeedbackText);
    }
}
