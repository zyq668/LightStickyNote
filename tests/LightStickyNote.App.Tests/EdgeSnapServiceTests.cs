using LightStickyNote.App.Services;

namespace LightStickyNote.App.Tests;

public sealed class EdgeSnapServiceTests
{
    [Theory]
    [InlineData(650, 350, 1000, true)]
    [InlineData(632, 350, 1000, true)]
    [InlineData(631, 350, 1000, false)]
    [InlineData(680, 350, 1000, false)]
    public void ShouldSnapRight_detects_windows_close_to_the_right_edge(
        double windowLeft,
        double windowWidth,
        double workAreaRight,
        bool expected)
    {
        var service = new EdgeSnapService();

        var result = service.ShouldSnapRight(windowLeft, windowWidth, workAreaRight);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(998, 1000, true)]
    [InlineData(982, 1000, true)]
    [InlineData(981, 1000, false)]
    public void ShouldSnapRightFromCursor_detects_cursor_close_to_the_right_edge(
        double cursorX,
        double workAreaRight,
        bool expected)
    {
        var service = new EdgeSnapService();

        var result = service.ShouldSnapRightFromCursor(cursorX, workAreaRight);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetHiddenLeft_leaves_only_the_configured_edge_tab_visible()
    {
        var service = new EdgeSnapService();

        var result = service.GetHiddenLeft(workAreaRight: 1000, edgeRevealWidth: 3);

        Assert.Equal(997, result);
    }

    [Fact]
    public void GetExpandedLeft_aligns_the_window_to_the_right_edge()
    {
        var service = new EdgeSnapService();

        var result = service.GetExpandedLeft(workAreaRight: 1000, windowWidth: 350);

        Assert.Equal(650, result);
    }
}
