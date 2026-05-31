using LightStickyNote.App.Models;

namespace LightStickyNote.App.ViewModels;

public sealed record TaskSummary(int PendingCount, int CompletedCount, int CompletionPercentage)
{
    public string FeedbackText => (PendingCount, CompletedCount) switch
    {
        (0, 0) => "今天很轻松",
        (0, _) => "今天的任务都完成了",
        (_, 0) => "从第一件开始",
        _ => $"已经完成 {CompletedCount} 件"
    };

    public static TaskSummary FromModels(IEnumerable<NoteItem> items)
    {
        return FromCompletionStates(items.Select(item => item.IsDone));
    }

    public static TaskSummary FromItems(IEnumerable<TaskItemViewModel> items)
    {
        return FromCompletionStates(items.Select(item => item.IsDone));
    }

    private static TaskSummary FromCompletionStates(IEnumerable<bool> completionStates)
    {
        var states = completionStates.ToList();
        var completedCount = states.Count(isDone => isDone);
        var pendingCount = states.Count - completedCount;
        var completionPercentage = states.Count == 0
            ? 0
            : (int)Math.Round(completedCount * 100d / states.Count);

        return new TaskSummary(pendingCount, completedCount, completionPercentage);
    }
}
