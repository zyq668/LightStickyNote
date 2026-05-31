using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using LightStickyNote.App.ViewModels;

namespace LightStickyNote.App;

public partial class MainWindow : Window
{
    private double _displayedCompletionPercentage;
    private TaskItemViewModel? _pendingDeleteItem;

    public bool AllowClose { get; set; }

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += OnLoaded;
        LocationChanged += OnWindowPlacementChanged;
        SizeChanged += OnWindowPlacementChanged;
        Closing += OnClosing;
    }

    private MainViewModel ViewModel => (MainViewModel)DataContext;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.ApplyWindowToView(this);
        _displayedCompletionPercentage = ViewModel.CompletionPercentage;
        NewTaskTextBox.Focus();
    }

    private void OnWindowPlacementChanged(object? sender, EventArgs e)
    {
        if (IsLoaded)
        {
            ViewModel.CaptureWindowPlacement(this);
        }
    }

    private async void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!AllowClose)
        {
            e.Cancel = true;
            Hide();
            await ViewModel.FlushPendingChangesAsync();
            return;
        }

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
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        SettingsPopup.IsOpen = !SettingsPopup.IsOpen;
    }

    private void HideToTrayButton_OnClick(object sender, RoutedEventArgs e)
    {
        SettingsPopup.IsOpen = false;
        Hide();
    }

    private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: TaskItemViewModel item })
        {
            return;
        }

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
