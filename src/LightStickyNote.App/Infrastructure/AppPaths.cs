using System.IO;

namespace LightStickyNote.App.Infrastructure;

public sealed class AppPaths
{
    public required string BaseDirectory { get; init; }

    public required string ProjectRootDirectory { get; init; }

    public required string DataDirectory { get; init; }

    public required string DatabaseFilePath { get; init; }

    public required string SettingsFilePath { get; init; }

    public required string RunScriptPath { get; init; }

    public required string LauncherScriptPath { get; init; }

    public required string ExecutableFilePath { get; init; }

    public required string StartupDirectory { get; init; }

    public static AppPaths ForCurrentExecutable()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var dataDirectory = Path.Combine(baseDirectory, "user-data");
        var projectRootDirectory = FindProjectRoot(baseDirectory);

        return new AppPaths
        {
            BaseDirectory = baseDirectory,
            ProjectRootDirectory = projectRootDirectory,
            DataDirectory = dataDirectory,
            DatabaseFilePath = Path.Combine(dataDirectory, "lightstickynote.db"),
            SettingsFilePath = Path.Combine(dataDirectory, "appsettings.json"),
            RunScriptPath = Path.Combine(projectRootDirectory, "tools", "Run.ps1"),
            LauncherScriptPath = Path.Combine(projectRootDirectory, "Launch-LightStickyNote.cmd"),
            ExecutableFilePath = Environment.ProcessPath ?? Path.Combine(baseDirectory, "LightStickyNote.App.exe"),
            StartupDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Startup)
        };
    }

    private static string FindProjectRoot(string baseDirectory)
    {
        var current = new DirectoryInfo(baseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LightStickyNote.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return baseDirectory;
    }
}
