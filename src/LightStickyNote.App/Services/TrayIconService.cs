using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace LightStickyNote.App.Services;

public sealed class TrayIconService : IDisposable
{
    private readonly Forms.NotifyIcon _notifyIcon;
    private readonly Action _showWindow;
    private readonly Action _hideWindow;

    public TrayIconService(Action showWindow, Action hideWindow, Action exitApplication)
    {
        _showWindow = showWindow;
        _hideWindow = hideWindow;

        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("显示便签", null, (_, _) => _showWindow());
        menu.Items.Add("隐藏便签", null, (_, _) => _hideWindow());
        menu.Items.Add("-");
        menu.Items.Add("退出", null, (_, _) => exitApplication());

        _notifyIcon = new Forms.NotifyIcon
        {
            Text = "Light Sticky Note",
            Icon = Drawing.SystemIcons.Information,
            Visible = true,
            ContextMenuStrip = menu
        };

        _notifyIcon.DoubleClick += (_, _) => _showWindow();
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }
}
