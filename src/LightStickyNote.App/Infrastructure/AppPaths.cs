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
        return Create(
            AppContext.BaseDirectory,
            Environment.ProcessPath,
            Environment.GetFolderPath(Environment.SpecialFolder.Startup));
    }

    public static AppPaths Create(string baseDirectory, string? processPath, string startupDirectory)
    {
        var dataDirectory = Path.Combine(baseDirectory, "user-data");
        var executableFilePath = processPath ?? Path.Combine(baseDirectory, "LightStickyNote.App.exe");
        var projectRootDirectory = ShouldUseProjectRoot(baseDirectory, executableFilePath)
            ? FindProjectRoot(baseDirectory)
            : baseDirectory;

        return new AppPaths
        {
            BaseDirectory = baseDirectory,
            ProjectRootDirectory = projectRootDirectory,
            DataDirectory = dataDirectory,
            DatabaseFilePath = Path.Combine(dataDirectory, "lightstickynote.db"),
            SettingsFilePath = Path.Combine(dataDirectory, "appsettings.json"),
            RunScriptPath = Path.Combine(projectRootDirectory, "tools", "Run.ps1"),
            LauncherScriptPath = Path.Combine(projectRootDirectory, "Launch-LightStickyNote.cmd"),
            ExecutableFilePath = executableFilePath,
            StartupDirectory = startupDirectory
        };
    }

    private static bool ShouldUseProjectRoot(string baseDirectory, string executableFilePath)
    {
        var processFileName = Path.GetFileName(executableFilePath);
        if (string.Equals(processFileName, "dotnet.exe", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var normalizedBaseDirectory = Path.GetFullPath(baseDirectory)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var projectBuildSegment = Path.Combine("src", "LightStickyNote.App", "bin") + Path.DirectorySeparatorChar;

        return normalizedBaseDirectory.Contains(projectBuildSegment, StringComparison.OrdinalIgnoreCase);
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
