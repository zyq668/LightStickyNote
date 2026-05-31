namespace LightStickyNote.App.Services;

public sealed class EdgeAutoHideService
{
    public bool ShouldHide(
        bool isEdgeSnapped,
        bool isPreviewMode,
        bool isMouseOver,
        bool isKeyboardFocusWithin,
        bool isWindowActive,
        bool isSettingsOpen,
        bool isDeleteConfirmationOpen,
        bool isDraggingWindow,
        DateTime now,
        DateTime suppressUntil)
    {
        if (!isEdgeSnapped || now < suppressUntil)
        {
            return false;
        }

        if (isMouseOver ||
            isKeyboardFocusWithin ||
            isSettingsOpen ||
            isDeleteConfirmationOpen ||
            isDraggingWindow)
        {
            return false;
        }

        if (isWindowActive && !isPreviewMode)
        {
            return false;
        }

        return true;
    }
}
