using System.IO;
using System.Text;

namespace LightStickyNote.App.Services;

public sealed class LauncherScriptService
{
    private readonly string _launcherScriptPath;
    private readonly string _runScriptPath;

    public LauncherScriptService(string launcherScriptPath, string runScriptPath)
    {
        _launcherScriptPath = launcherScriptPath;
        _runScriptPath = runScriptPath;
    }

    public async Task EnsureLauncherScriptAsync()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_launcherScriptPath)!);

        var content =
            $"""
            @echo off
            setlocal
            powershell -NoProfile -ExecutionPolicy Bypass -File "{_runScriptPath}"
            endlocal
            """;

        await File.WriteAllTextAsync(_launcherScriptPath, content, new UTF8Encoding(false));
    }
}
