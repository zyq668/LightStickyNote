using LightStickyNote.App.Services;

namespace LightStickyNote.App.Tests;

public sealed class StartupRegistrationServiceTests : IDisposable
{
    private readonly string _rootDirectory;

    public StartupRegistrationServiceTests()
    {
        _rootDirectory = Path.Combine(
            "D:\\CodexProjects\\LightStickyNote",
            "tests",
            "LightStickyNote.App.Tests",
            "test-output",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_rootDirectory);
    }

    [Fact]
    public async Task SetEnabledAsync_True_CreatesStartupScript()
    {
        var launcherPath = Path.Combine(_rootDirectory, "Launch-LightStickyNote.cmd");
        await File.WriteAllTextAsync(launcherPath, "@echo off");
        var startupDirectory = Path.Combine(_rootDirectory, "Startup");
        var service = new StartupRegistrationService(startupDirectory, launcherPath);

        await service.SetEnabledAsync(true);

        var startupScriptPath = Path.Combine(startupDirectory, "LightStickyNote Startup.cmd");
        Assert.True(File.Exists(startupScriptPath));
        Assert.True(service.IsEnabled());
        var content = await File.ReadAllTextAsync(startupScriptPath);
        Assert.Contains(launcherPath, content);
    }

    [Fact]
    public async Task SetEnabledAsync_False_RemovesStartupScript()
    {
        var launcherPath = Path.Combine(_rootDirectory, "Launch-LightStickyNote.cmd");
        await File.WriteAllTextAsync(launcherPath, "@echo off");
        var startupDirectory = Path.Combine(_rootDirectory, "Startup");
        var service = new StartupRegistrationService(startupDirectory, launcherPath);
        await service.SetEnabledAsync(true);

        await service.SetEnabledAsync(false);

        Assert.False(service.IsEnabled());
        Assert.False(File.Exists(Path.Combine(startupDirectory, "LightStickyNote Startup.cmd")));
    }

    [Fact]
    public async Task IsEnabled_ReturnsFalse_WhenStartupScriptTargetsDifferentLauncher()
    {
        var launcherPath = Path.Combine(_rootDirectory, "Launch-LightStickyNote.cmd");
        var differentLauncherPath = Path.Combine(_rootDirectory, "other", "Launch-LightStickyNote.cmd");
        await File.WriteAllTextAsync(launcherPath, "@echo off");
        Directory.CreateDirectory(Path.GetDirectoryName(differentLauncherPath)!);
        await File.WriteAllTextAsync(differentLauncherPath, "@echo off");

        var startupDirectory = Path.Combine(_rootDirectory, "Startup");
        Directory.CreateDirectory(startupDirectory);
        var startupScriptPath = Path.Combine(startupDirectory, "LightStickyNote Startup.cmd");
        await File.WriteAllTextAsync(
            startupScriptPath,
            $$"""
            @echo off
            start "" "{{differentLauncherPath}}"
            """);

        var service = new StartupRegistrationService(startupDirectory, launcherPath);

        Assert.False(service.IsEnabled());
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_rootDirectory))
            {
                Directory.Delete(_rootDirectory, recursive: true);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
