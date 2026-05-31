using LightStickyNote.App.Services;

namespace LightStickyNote.App.Tests;

public sealed class EdgeAutoHideServiceTests
{
    private readonly EdgeAutoHideService _service = new();
    private static readonly DateTime Now = new(2026, 6, 1, 9, 0, 0);

    [Fact]
    public void ShouldHide_returns_false_while_window_is_active_for_editing()
    {
        var result = _service.ShouldHide(
            isEdgeSnapped: true,
            isPreviewMode: false,
            isMouseOver: false,
            isKeyboardFocusWithin: false,
            isWindowActive: true,
            isSettingsOpen: false,
            isDeleteConfirmationOpen: false,
            isDraggingWindow: false,
            now: Now,
            suppressUntil: DateTime.MinValue);

        Assert.False(result);
    }

    [Fact]
    public void ShouldHide_returns_false_while_keyboard_focus_is_within_window()
    {
        var result = _service.ShouldHide(
            isEdgeSnapped: true,
            isPreviewMode: false,
            isMouseOver: false,
            isKeyboardFocusWithin: true,
            isWindowActive: false,
            isSettingsOpen: false,
            isDeleteConfirmationOpen: false,
            isDraggingWindow: false,
            now: Now,
            suppressUntil: DateTime.MinValue);

        Assert.False(result);
    }

    [Fact]
    public void ShouldHide_returns_false_during_suppression_window()
    {
        var result = _service.ShouldHide(
            isEdgeSnapped: true,
            isPreviewMode: true,
            isMouseOver: false,
            isKeyboardFocusWithin: false,
            isWindowActive: false,
            isSettingsOpen: false,
            isDeleteConfirmationOpen: false,
            isDraggingWindow: false,
            now: Now,
            suppressUntil: Now.AddMilliseconds(800));

        Assert.False(result);
    }

    [Fact]
    public void ShouldHide_returns_true_for_preview_window_after_mouse_leaves()
    {
        var result = _service.ShouldHide(
            isEdgeSnapped: true,
            isPreviewMode: true,
            isMouseOver: false,
            isKeyboardFocusWithin: false,
            isWindowActive: false,
            isSettingsOpen: false,
            isDeleteConfirmationOpen: false,
            isDraggingWindow: false,
            now: Now,
            suppressUntil: DateTime.MinValue);

        Assert.True(result);
    }
}
