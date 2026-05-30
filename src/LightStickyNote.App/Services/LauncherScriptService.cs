using System.IO;
using System.Text;

namespace LightStickyNote.App.Services;

public sealed class LauncherScriptService
{
    private readonly string _launcherScriptPath;
    private readonly string _runScriptPath;
    private readonly string _executableFilePath;

    public LauncherScriptService(string launcherScriptPath, string runScriptPath, string executableFilePath)
    {
        _launcherScriptPath = launcherScriptPath;
        _runScriptPath = runScriptPath;
        _executableFilePath = executableFilePath;
    }

    public async Task EnsureLauncherScriptAsync()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_launcherScriptPath)!);

        var content = File.Exists(_runScriptPath)
            ? $"""
              @echo off
              setlocal
              powershell -NoProfile -ExecutionPolicy Bypass -File "{_runScriptPath}"
              endlocal
              """
            : $"""
              @echo off
              start "" "{_executableFilePath}"
              """;

        await File.WriteAllTextAsync(_launcherScriptPath, content, new UTF8Encoding(false));
    }
}
