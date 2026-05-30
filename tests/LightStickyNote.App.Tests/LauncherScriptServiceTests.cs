using LightStickyNote.App.Services;

namespace LightStickyNote.App.Tests;

public sealed class LauncherScriptServiceTests : IDisposable
{
    private readonly string _rootDirectory;

    public LauncherScriptServiceTests()
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
    public async Task EnsureLauncherScriptAsync_CreatesDoubleClickScript()
    {
        var scriptPath = Path.Combine(_rootDirectory, "启动便签.cmd");
        var runScriptPath = Path.Combine(_rootDirectory, "tools", "Run.ps1");
        Directory.CreateDirectory(Path.GetDirectoryName(runScriptPath)!);
        await File.WriteAllTextAsync(runScriptPath, "Write-Host test");
        var service = new LauncherScriptService(scriptPath, runScriptPath, "LightStickyNote.App.exe");

        await service.EnsureLauncherScriptAsync();

        Assert.True(File.Exists(scriptPath));
        var content = await File.ReadAllTextAsync(scriptPath);
        Assert.Contains("Run.ps1", content);
        Assert.Contains("powershell", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EnsureLauncherScriptAsync_StartsExecutable_WhenRunScriptMissing()
    {
        var scriptPath = Path.Combine(_rootDirectory, "Launch-LightStickyNote.cmd");
        var runScriptPath = Path.Combine(_rootDirectory, "tools", "Run.ps1");
        var executablePath = Path.Combine(_rootDirectory, "LightStickyNote.exe");
        var service = new LauncherScriptService(scriptPath, runScriptPath, executablePath);

        await service.EnsureLauncherScriptAsync();

        var content = await File.ReadAllTextAsync(scriptPath);
        Assert.Contains("start", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(executablePath, content);
        Assert.DoesNotContain("powershell", content, StringComparison.OrdinalIgnoreCase);
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
