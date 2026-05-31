using LightStickyNote.App.Infrastructure;

namespace LightStickyNote.App.Models;

public sealed class AppSettings : ObservableObject
{
    private bool _alwaysOnTop = true;
    private bool _launchAtStartup;
    private double _opacity = 0.97;
    private bool _minimizeToTrayOnClose = true;
    private double _defaultWidth = 360;
    private double _defaultHeight = 640;
    private double _rightMargin = 18;
    private double _topMargin = 90;
    private bool _edgeSnapEnabled = true;
    private double _edgeRevealWidth = 3;
    private int _edgeHideDelayMilliseconds = 900;

    public bool AlwaysOnTop
    {
        get => _alwaysOnTop;
        set => SetProperty(ref _alwaysOnTop, value);
    }

    public bool LaunchAtStartup
    {
        get => _launchAtStartup;
        set => SetProperty(ref _launchAtStartup, value);
    }

    public double Opacity
    {
        get => _opacity;
        set => SetProperty(ref _opacity, Math.Clamp(value, 0.45, 1.0));
    }

    public bool MinimizeToTrayOnClose
    {
        get => _minimizeToTrayOnClose;
        set => SetProperty(ref _minimizeToTrayOnClose, value);
    }

    public double DefaultWidth
    {
        get => _defaultWidth;
        set => SetProperty(ref _defaultWidth, value);
    }

    public double DefaultHeight
    {
        get => _defaultHeight;
        set => SetProperty(ref _defaultHeight, value);
    }

    public double RightMargin
    {
        get => _rightMargin;
        set => SetProperty(ref _rightMargin, value);
    }

    public double TopMargin
    {
        get => _topMargin;
        set => SetProperty(ref _topMargin, value);
    }

    public bool EdgeSnapEnabled
    {
        get => _edgeSnapEnabled;
        set => SetProperty(ref _edgeSnapEnabled, value);
    }

    public double EdgeRevealWidth
    {
        get => _edgeRevealWidth;
        set => SetProperty(ref _edgeRevealWidth, Math.Round(Math.Clamp(value, 2, 8)));
    }

    public int EdgeHideDelayMilliseconds
    {
        get => _edgeHideDelayMilliseconds;
        set => SetProperty(ref _edgeHideDelayMilliseconds, Math.Clamp(value, 300, 3000));
    }
}
