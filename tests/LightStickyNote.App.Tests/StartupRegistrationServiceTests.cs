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
        var launcherPath = Path.Combine(_rootDirectory, "启动便签.cmd");
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
        var launcherPath = Path.Combine(_rootDirectory, "启动便签.cmd");
        await File.WriteAllTextAsync(launcherPath, "@echo off");
        var startupDirectory = Path.Combine(_rootDirectory, "Startup");
        var service = new StartupRegistrationService(startupDirectory, launcherPath);
        await service.SetEnabledAsync(true);

        await service.SetEnabledAsync(false);

        Assert.False(service.IsEnabled());
        Assert.False(File.Exists(Path.Combine(startupDirectory, "LightStickyNote Startup.cmd")));
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
