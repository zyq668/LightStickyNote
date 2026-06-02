using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using LightStickyNote.App.Services;
using LightStickyNote.App.ViewModels;

namespace LightStickyNote.App;

public partial class MainWindow : Window
{
    private static readonly TimeSpan PreviewAutoHideProtection = TimeSpan.FromMilliseconds(320);
    private static readonly TimeSpan DragAutoHideProtection = TimeSpan.FromMilliseconds(240);

    private double _displayedCompletionPercentage;
    private TaskItemViewModel? _pendingDeleteItem;
    private readonly EdgeAutoHideService _edgeAutoHideService = new();
    private readonly EdgeSnapService _edgeSnapService = new();
    private readonly DispatcherTimer _edgeHideTimer;
    private readonly EdgeRevealWindow _edgeRevealWindow;
    private bool _isEdgeSnapped;
    private bool _isDraggingWindow;
    private bool _isPreviewMode;
    private System.Windows.Point _dragCursorOffset;
    private double _snappedRevealTop;
    private double _snappedRevealHeight;
    private DateTime _suppressAutoHideUntil = DateTime.MinValue;

    public bool AllowClose { get; set; }

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += OnLoaded;
        LocationChanged += OnWindowPlacementChanged;
        SizeChanged += OnWindowPlacementChanged;
        Closing += OnClosing;
        Activated += OnActivated;
        Deactivated += OnDeactivated;
        MouseEnter += MainWindow_OnMouseEnter;
        MouseLeave += MainWindow_OnMouseLeave;
        SettingsPopup.Closed += SettingsPopup_OnClosed;
        ViewModel.Settings.PropertyChanged += Settings_OnPropertyChanged;

        _edgeHideTimer = new DispatcherTimer();
        _edgeHideTimer.Tick += EdgeHideTimer_OnTick;
        _edgeRevealWindow = new EdgeRevealWindow();
        _edgeRevealWindow.RevealRequested += EdgeRevealWindow_OnRevealRequested;
    }

    private MainViewModel ViewModel => (MainViewModel)DataContext;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.ApplyWindowToView(this);
        CacheRevealBoundsFromCurrentWindow();
        _displayedCompletionPercentage = ViewModel.CompletionPercentage;
        NewTaskTextBox.Focus();
    }

    private void OnWindowPlacementChanged(object? sender, EventArgs e)
    {
        if (IsLoaded && !_isEdgeSnapped)
        {
            CacheRevealBoundsFromCurrentWindow();
            ViewModel.CaptureWindowPlacement(this);
        }
    }

    private async void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!AllowClose)
        {
            e.Cancel = true;
            HideToTray();
            await ViewModel.FlushPendingChangesAsync();
            return;
        }

        _edgeRevealWindow.Close();
        await ViewModel.FlushPendingChangesAsync();
    }

    private async void NewTaskTextBox_OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != System.Windows.Input.Key.Enter)
        {
            return;
        }

        e.Handled = true;
        await ViewModel.AddTaskFromInputAsync();
        NewTaskTextBox.Focus();
    }

    private void Header_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed ||
            sender is not UIElement header ||
            IsHeaderButtonClick(e.OriginalSource))
        {
            return;
        }

        ReleaseEdgeSnapForDragging();
        _dragCursorOffset = e.GetPosition(this);
        _isDraggingWindow = header.CaptureMouse();
        e.Handled = true;
    }

    private void Header_OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (!_isDraggingWindow || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var cursorPosition = GetCursorPositionInLogical();
        Left = cursorPosition.X - _dragCursorOffset.X;
        Top = cursorPosition.Y - _dragCursorOffset.Y;
    }

    private void Header_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not UIElement header)
        {
            return;
        }

        CompleteHeaderDrag(header);
    }

    private void Header_OnLostMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is not UIElement header)
        {
            return;
        }

        CompleteHeaderDrag(header);
    }

    private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        SettingsPopup.IsOpen = !SettingsPopup.IsOpen;
    }

    private void HideToTrayButton_OnClick(object sender, RoutedEventArgs e)
    {
        HideToTray();
    }

    private void MainWindow_OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        StopEdgeHideTimer();
    }

    private void MainWindow_OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (_isPreviewMode)
        {
            ScheduleEdgeHide();
        }
    }

    private void SettingsPopup_OnClosed(object? sender, EventArgs e)
    {
        ScheduleEdgeHide();
    }

    private void Settings_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.Settings.EdgeSnapEnabled) &&
            !ViewModel.Settings.EdgeSnapEnabled)
        {
            ReleaseEdgeSnap();
            ShowFromTray();
        }

        if (e.PropertyName == nameof(ViewModel.Settings.EdgeRevealWidth) &&
            _edgeRevealWindow.IsVisible)
        {
            ShowEdgeRevealWindow();
        }

        if (e.PropertyName == nameof(ViewModel.Settings.AlwaysOnTop))
        {
            _edgeRevealWindow.Topmost = ViewModel.Settings.AlwaysOnTop;
        }
    }

    private void OnActivated(object? sender, EventArgs e)
    {
        StopEdgeHideTimer();

        if (_isEdgeSnapped)
        {
            _isPreviewMode = false;
        }
    }

    private void OnDeactivated(object? sender, EventArgs e)
    {
        ScheduleEdgeHide();
    }

    private void TrySnapToRightEdge()
    {
        if (!ViewModel.Settings.EdgeSnapEnabled)
        {
            return;
        }

        if (!IsNearCurrentWorkAreaRight() && !IsCursorNearCurrentWorkAreaRight())
        {
            return;
        }

        var workArea = SystemParameters.WorkArea;
        CacheRevealBoundsFromCurrentWindow();
        Left = _edgeSnapService.GetExpandedLeft(workArea.Right, Width);
        _isEdgeSnapped = true;
        HideToEdge();
    }

    private void ScheduleEdgeHide()
    {
        if (!_isEdgeSnapped)
        {
            return;
        }

        RestartEdgeHideTimer(ViewModel.Settings.EdgeHideDelayMilliseconds);
    }

    private void StopEdgeHideTimer()
    {
        _edgeHideTimer.Stop();
    }

    private void EdgeHideTimer_OnTick(object? sender, EventArgs e)
    {
        StopEdgeHideTimer();

        if (!_edgeAutoHideService.ShouldHide(
                isEdgeSnapped: _isEdgeSnapped,
                isPreviewMode: _isPreviewMode,
                isMouseOver: IsMouseOver,
                isKeyboardFocusWithin: IsKeyboardFocusWithin,
                isWindowActive: IsActive,
                isSettingsOpen: SettingsPopup.IsOpen,
                isDeleteConfirmationOpen: DeleteConfirmationOverlay.Visibility == Visibility.Visible,
                isDraggingWindow: _isDraggingWindow,
                now: DateTime.Now,
                suppressUntil: _suppressAutoHideUntil))
        {
            if (ShouldKeepWaitingForAutoHide())
            {
                RestartEdgeHideTimer(180);
            }

            return;
        }

        HideToEdge();
    }

    private void ReleaseEdgeSnapForDragging()
    {
        ReleaseEdgeSnap();
    }

    private void ReleaseEdgeSnap()
    {
        StopEdgeHideTimer();
        _isEdgeSnapped = false;
        _isPreviewMode = false;
        _edgeRevealWindow.Hide();
    }

    private void HideToEdge()
    {
        if (!_isEdgeSnapped)
        {
            return;
        }

        SettingsPopup.IsOpen = false;
        ShowEdgeRevealWindow();
        Hide();
    }

    private void ShowEdgeRevealWindow()
    {
        var workArea = SystemParameters.WorkArea;
        var revealWidth = ViewModel.Settings.EdgeRevealWidth;
        var revealLeft = _edgeSnapService.GetHiddenLeft(workArea.Right, revealWidth);
        var revealHeight = _snappedRevealHeight > 0
            ? Math.Min(_snappedRevealHeight, workArea.Height)
            : Math.Min(GetCurrentWindowBoundsInLogical().Height, workArea.Height);
        var revealTop = Math.Clamp(
            _snappedRevealTop,
            workArea.Top,
            Math.Max(workArea.Top, workArea.Bottom - revealHeight));

        _edgeRevealWindow.ShowAt(
            revealLeft,
            revealTop,
            revealWidth,
            revealHeight,
            ViewModel.Settings.AlwaysOnTop);
    }

    private void EdgeRevealWindow_OnRevealRequested(object? sender, EventArgs e)
    {
        if (!_isEdgeSnapped)
        {
            return;
        }

        _edgeRevealWindow.Hide();
        _isPreviewMode = true;
        SuppressAutoHideFor(PreviewAutoHideProtection);
        if (!IsVisible)
        {
            Show();
        }
    }

    public void HideToTray()
    {
        SettingsPopup.IsOpen = false;
        ReleaseEdgeSnap();
        Hide();
    }

    public void ShowFromTray()
    {
        ReleaseEdgeSnap();
        if (!IsVisible)
        {
            Show();
        }

        WindowState = WindowState.Normal;
        Activate();
    }

    private bool IsNearCurrentWorkAreaRight()
    {
        var handle = new WindowInteropHelper(this).Handle;
        var screen = System.Windows.Forms.Screen.FromHandle(handle);
        GetWindowRect(handle, out var bounds);

        return _edgeSnapService.ShouldSnapRight(bounds.Left, bounds.Width, screen.WorkingArea.Right);
    }

    private bool IsCursorNearCurrentWorkAreaRight()
    {
        if (!GetCursorPos(out var nativePoint))
        {
            return false;
        }

        var screen = System.Windows.Forms.Screen.FromPoint(
            new System.Drawing.Point(nativePoint.X, nativePoint.Y));

        return _edgeSnapService.ShouldSnapRightFromCursor(nativePoint.X, screen.WorkingArea.Right);
    }

    private void CacheRevealBoundsFromCurrentWindow()
    {
        var bounds = GetCurrentWindowBoundsInLogical();
        _snappedRevealTop = bounds.Top;
        _snappedRevealHeight = bounds.Height;
    }

    private Rect GetCurrentWindowBoundsInLogical()
    {
        var fallbackWidth = ActualWidth > 0 ? ActualWidth : Width;
        var fallbackHeight = ActualHeight > 0 ? ActualHeight : Height;
        var handle = new WindowInteropHelper(this).Handle;

        if (handle == IntPtr.Zero || !GetWindowRect(handle, out var nativeBounds))
        {
            return new Rect(Left, Top, fallbackWidth, fallbackHeight);
        }

        if (PresentationSource.FromVisual(this)?.CompositionTarget is not { } target)
        {
            return new Rect(Left, Top, fallbackWidth, fallbackHeight);
        }

        var topLeft = target.TransformFromDevice.Transform(new System.Windows.Point(nativeBounds.Left, nativeBounds.Top));
        var bottomRight = target.TransformFromDevice.Transform(new System.Windows.Point(nativeBounds.Right, nativeBounds.Bottom));
        return new Rect(topLeft, bottomRight);
    }

    private void CompleteHeaderDrag(UIElement header)
    {
        if (!_isDraggingWindow)
        {
            return;
        }

        _isDraggingWindow = false;
        if (Mouse.Captured == header)
        {
            header.ReleaseMouseCapture();
        }

        SuppressAutoHideFor(DragAutoHideProtection);
        TrySnapToRightEdge();
    }

    private void SuppressAutoHideFor(TimeSpan duration)
    {
        _suppressAutoHideUntil = DateTime.Now.Add(duration);
    }

    private bool ShouldKeepWaitingForAutoHide()
    {
        return _isEdgeSnapped &&
               (_isPreviewMode || !IsActive) &&
               !IsMouseOver &&
               !IsKeyboardFocusWithin &&
               !SettingsPopup.IsOpen &&
               DeleteConfirmationOverlay.Visibility != Visibility.Visible &&
               !_isDraggingWindow;
    }

    private void RestartEdgeHideTimer(double intervalMilliseconds)
    {
        _edgeHideTimer.Stop();
        _edgeHideTimer.Interval = TimeSpan.FromMilliseconds(intervalMilliseconds);
        _edgeHideTimer.Start();
    }

    private static bool IsHeaderButtonClick(object? originalSource)
    {
        var current = originalSource as DependencyObject;
        while (current is not null)
        {
            if (current is System.Windows.Controls.Primitives.ButtonBase)
            {
                return true;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return false;
    }

    private System.Windows.Point GetCursorPositionInLogical()
    {
        var fallback = PointToScreen(Mouse.GetPosition(this));
        if (!GetCursorPos(out var nativePoint))
        {
            return fallback;
        }

        if (PresentationSource.FromVisual(this)?.CompositionTarget is not { } target)
        {
            return fallback;
        }

        return target.TransformFromDevice.Transform(new System.Windows.Point(nativePoint.X, nativePoint.Y));
    }

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr windowHandle, out NativeWindowBounds bounds);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out NativePoint point);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeWindowBounds
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public int Width => Right - Left;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }

    private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: TaskItemViewModel item })
        {
            return;
        }

        ShowDeleteConfirmation(item);
    }

    private void TaskMenuButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { ContextMenu: { } contextMenu })
        {
            return;
        }

        contextMenu.PlacementTarget = sender as UIElement;
        contextMenu.IsOpen = true;
        e.Handled = true;
    }

    private void ReminderMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: TaskItemViewModel item })
        {
            return;
        }

        var dialog = new ReminderEditWindow(item)
        {
            Owner = this
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        if (dialog.ReminderCleared)
        {
            item.ClearReminder();
            return;
        }

        if (dialog.ReminderAt is { } reminderAt)
        {
            item.SetReminder(reminderAt);
        }
    }

    private void DeleteMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: TaskItemViewModel item })
        {
            return;
        }

        ShowDeleteConfirmation(item);
    }

    private void ShowDeleteConfirmation(TaskItemViewModel item)
    {
        _pendingDeleteItem = item;
        DeleteTaskPreviewTextBlock.Text = item.Text;
        DeleteConfirmationOverlay.Visibility = Visibility.Visible;
        DeleteConfirmationOverlay.BeginAnimation(
            OpacityProperty,
            new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(140)));
        CancelDeleteButton.Focus();
    }

    private void CancelDeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        CloseDeleteConfirmation();
    }

    private void ConfirmDeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_pendingDeleteItem is { } item)
        {
            ViewModel.DeleteItemCommand.Execute(item);
        }

        CloseDeleteConfirmation();
    }

    private void CloseDeleteConfirmation()
    {
        DeleteConfirmationOverlay.Visibility = Visibility.Collapsed;
        DeleteConfirmationOverlay.BeginAnimation(OpacityProperty, null);
        DeleteTaskPreviewTextBlock.Text = string.Empty;
        _pendingDeleteItem = null;
    }

    private void TaskCheckBox_OnChecked(object sender, RoutedEventArgs e)
    {
        AnimateTaskProgress(celebrate: true);
    }

    private void TaskCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
    {
        AnimateTaskProgress(celebrate: false);
    }

    private void AnimateTaskProgress(bool celebrate)
    {
        Dispatcher.BeginInvoke(() =>
        {
            var nextPercentage = ViewModel.CompletionPercentage;
            var animation = new DoubleAnimation(
                _displayedCompletionPercentage,
                nextPercentage,
                TimeSpan.FromMilliseconds(260))
            {
                EasingFunction = new QuadraticEase
                {
                    EasingMode = EasingMode.EaseOut
                }
            };

            CompletionProgress.BeginAnimation(RangeBase.ValueProperty, animation);
            _displayedCompletionPercentage = nextPercentage;

            if (celebrate)
            {
                FeedbackTextBlock.BeginAnimation(
                    OpacityProperty,
                    new DoubleAnimation(0.35, 1, TimeSpan.FromMilliseconds(320)));
            }
        });
    }
}
