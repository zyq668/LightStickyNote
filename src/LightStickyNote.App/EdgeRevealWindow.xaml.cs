using System.Windows;
using System.Windows.Input;

namespace LightStickyNote.App;

public partial class EdgeRevealWindow : Window
{
    public event EventHandler? RevealRequested;

    public EdgeRevealWindow()
    {
        InitializeComponent();
        MouseEnter += OnMouseEnter;
    }

    public void ShowAt(double left, double top, double width, double height, bool topmost)
    {
        Width = width;
        Height = height;
        Topmost = topmost;

        if (!IsVisible)
        {
            Show();
        }

        UpdateLayout();
        ApplyPosition(left, top);
        Dispatcher.BeginInvoke(() => ApplyPosition(left, top));
    }

    private void ApplyPosition(double left, double top)
    {
        Left = left;
        Top = top;
    }

    private void OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        RevealRequested?.Invoke(this, EventArgs.Empty);
    }
}
