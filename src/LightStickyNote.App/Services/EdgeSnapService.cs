namespace LightStickyNote.App.Services;

public sealed class EdgeSnapService
{
    private const double SnapThreshold = 18;

    public bool ShouldSnapRight(double windowLeft, double windowWidth, double workAreaRight)
    {
        return IsNearRightEdge(windowLeft + windowWidth, workAreaRight);
    }

    public bool ShouldSnapRightFromCursor(double cursorX, double workAreaRight)
    {
        return IsNearRightEdge(cursorX, workAreaRight);
    }

    public double GetHiddenLeft(double workAreaRight, double edgeRevealWidth)
    {
        return workAreaRight - edgeRevealWidth;
    }

    public double GetExpandedLeft(double workAreaRight, double windowWidth)
    {
        return workAreaRight - windowWidth;
    }

    private static bool IsNearRightEdge(double anchorX, double workAreaRight)
    {
        var distanceToRightEdge = Math.Abs(workAreaRight - anchorX);
        return distanceToRightEdge <= SnapThreshold;
    }
}
