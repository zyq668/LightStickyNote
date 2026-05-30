using System.IO;
using System.Windows;
using LightStickyNote.App.Data;
using LightStickyNote.App.Infrastructure;
using LightStickyNote.App.Repositories;
using LightStickyNote.App.Services;
using LightStickyNote.App.ViewModels;

namespace LightStickyNote.App;

public partial class App : System.Windows.Application
{
    private TrayIconService? _trayIconService;
    private MainViewModel? _mainViewModel;
    private MainWindow? _mainWindow;
    private bool _isExiting;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var appPaths = AppPaths.ForCurrentExecutable();
        Directory.CreateDirectory(appPaths.DataDirectory);

        var settingsService = new SettingsService(appPaths.SettingsFilePath);
        var settings = await settingsService.LoadAsync();
        var launcherScriptService = new LauncherScriptService(
            appPaths.LauncherScriptPath,
            appPaths.RunScriptPath,
            appPaths.ExecutableFilePath);
        var startupRegistrationService = new StartupRegistrationService(appPaths.StartupDirectory, appPaths.LauncherScriptPath);
        var databaseInitializer = new DatabaseInitializer(appPaths.DatabaseFilePath);

        try
        {
            await launcherScriptService.EnsureLauncherScriptAsync();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"启动脚本创建失败。\n\n路径: {appPaths.LauncherScriptPath}\n\n错误: {ex.Message}",
                "LightStickyNote 启动警告",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        try
        {
            await databaseInitializer.InitializeAsync();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"SQLite 初始化失败，程序不会覆盖现有数据。\n\n数据库文件: {appPaths.DatabaseFilePath}\n\n错误: {ex.Message}",
                "LightStickyNote 启动失败",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(-1);
            return;
        }

        var noteRepository = new NoteRepository(appPaths.DatabaseFilePath);
        var windowPlacementService = new WindowPlacementService();

        _mainViewModel = new MainViewModel(
            noteRepository,
            settingsService,
            startupRegistrationService,
            settings,
            windowPlacementService);
        await _mainViewModel.InitializeAsync();

        _mainWindow = new MainWindow(_mainViewModel);
        _trayIconService = new TrayIconService(ShowMainWindow, HideMainWindow, ExitApplication);

        MainWindow = _mainWindow;
        ShowMainWindow();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIconService?.Dispose();
        base.OnExit(e);
    }

    private void ShowMainWindow()
    {
        if (_mainWindow is null)
        {
            return;
        }

        if (!_mainWindow.IsVisible)
        {
            _mainWindow.Show();
        }

        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private void HideMainWindow()
    {
        _mainWindow?.Hide();
    }

    private async void ExitApplication()
    {
        if (_isExiting)
        {
            return;
        }

        _isExiting = true;

        if (_mainViewModel is not null)
        {
            await _mainViewModel.FlushPendingChangesAsync();
        }

        if (_mainWindow is not null)
        {
            _mainWindow.AllowClose = true;
            _mainWindow.Close();
        }

        ShutdownMode = ShutdownMode.OnMainWindowClose;
        Shutdown();
    }
}
