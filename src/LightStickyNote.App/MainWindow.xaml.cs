using System.Windows;
using LightStickyNote.App.ViewModels;

namespace LightStickyNote.App;

public partial class MainWindow : Window
{
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
}
