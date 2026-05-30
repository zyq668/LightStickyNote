using System.Windows;
using LightStickyNote.App.Models;

namespace LightStickyNote.App.Services;

public sealed class WindowPlacementService
{
    public void Apply(Window window, Note note, AppSettings settings)
    {
        window.Width = note.Width > 0 ? note.Width : settings.DefaultWidth;
        window.Height = note.Height > 0 ? note.Height : settings.DefaultHeight;
        window.SetCurrentValue(Window.TopmostProperty, settings.AlwaysOnTop);

        var workArea = SystemParameters.WorkArea;
        var left = note.PositionX ?? Math.Max(workArea.Left, workArea.Right - window.Width - settings.RightMargin);
        var top = note.PositionY ?? Math.Max(workArea.Top, settings.TopMargin);

        window.Left = Clamp(left, workArea.Left, workArea.Right - 120);
        window.Top = Clamp(top, workArea.Top, workArea.Bottom - 120);
    }

    public void Capture(Window window, Note note, AppSettings settings)
    {
        note.PositionX = window.Left;
        note.PositionY = window.Top;
        note.Width = window.Width;
        note.Height = window.Height;
        note.IsPinned = settings.AlwaysOnTop;
    }

    private static double Clamp(double value, double min, double max)
    {
        if (max < min)
        {
            return min;
        }

        return Math.Min(Math.Max(value, min), max);
    }
}
