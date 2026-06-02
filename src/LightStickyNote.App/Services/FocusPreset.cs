namespace LightStickyNote.App.Services;

public sealed record FocusPreset(string Label, int Minutes)
{
    public static FocusPreset Pomodoro { get; } = new("番茄专注", 25);

    public static FocusPreset DeepFocus { get; } = new("深度专注", 50);
}
