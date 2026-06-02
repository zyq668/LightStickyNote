using LightStickyNote.App.Services;

namespace LightStickyNote.App.Tests;

public sealed class FocusPresetTests
{
    [Fact]
    public void Pomodoro_uses_25_minutes()
    {
        Assert.Equal(25, FocusPreset.Pomodoro.Minutes);
    }

    [Fact]
    public void DeepFocus_uses_50_minutes()
    {
        Assert.Equal(50, FocusPreset.DeepFocus.Minutes);
    }
}
