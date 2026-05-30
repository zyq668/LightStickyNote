using System.IO;
using System.Text;

namespace LightStickyNote.App.Services;

public sealed class StartupRegistrationService
{
    private const string StartupScriptName = "LightStickyNote Startup.cmd";

    private readonly string _startupDirectory;
    private readonly string _launcherScriptPath;

    public StartupRegistrationService(string startupDirectory, string launcherScriptPath)
    {
        _startupDirectory = startupDirectory;
        _launcherScriptPath = launcherScriptPath;
    }

    public bool IsEnabled()
    {
        return File.Exists(GetStartupScriptPath());
    }

    public async Task SetEnabledAsync(bool enabled)
    {
        Directory.CreateDirectory(_startupDirectory);
        var startupScriptPath = GetStartupScriptPath();

        if (!enabled)
        {
            if (File.Exists(startupScriptPath))
            {
                File.Delete(startupScriptPath);
            }

            return;
        }

        var content =
            $"""
            @echo off
            start "" "{_launcherScriptPath}"
            """;

        await File.WriteAllTextAsync(startupScriptPath, content, new UTF8Encoding(false));
    }

    private string GetStartupScriptPath()
    {
        return Path.Combine(_startupDirectory, StartupScriptName);
    }
}
