using LightStickyNote.App.Infrastructure;

namespace LightStickyNote.App.Tests;

public sealed class AppPathsTests
{
    [Fact]
    public void Create_UsesProjectRoot_WhenRunningFromDevelopmentBuildOutput()
    {
        var baseDirectory = "D:\\CodexProjects\\LightStickyNote\\src\\LightStickyNote.App\\bin\\Debug\\net8.0-windows\\";
        var startupDirectory = "C:\\Users\\zyq66\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup";

        var paths = AppPaths.Create(baseDirectory, "D:\\DevTools\\dotnet\\dotnet.exe", startupDirectory);

        Assert.Equal("D:\\CodexProjects\\LightStickyNote", paths.ProjectRootDirectory);
        Assert.Equal("D:\\CodexProjects\\LightStickyNote\\Launch-LightStickyNote.cmd", paths.LauncherScriptPath);
        Assert.Equal("D:\\CodexProjects\\LightStickyNote\\tools\\Run.ps1", paths.RunScriptPath);
    }

    [Fact]
    public void Create_UsesPortableDirectory_WhenRunningPackagedExeInsideArtifactsTree()
    {
        var baseDirectory = "D:\\CodexProjects\\LightStickyNote\\artifacts\\LightStickyNote-win-x64-portable\\";
        var startupDirectory = "C:\\Users\\zyq66\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup";
        var executablePath = "D:\\CodexProjects\\LightStickyNote\\artifacts\\LightStickyNote-win-x64-portable\\LightStickyNote.App.exe";

        var paths = AppPaths.Create(baseDirectory, executablePath, startupDirectory);

        Assert.Equal(baseDirectory.TrimEnd('\\'), paths.ProjectRootDirectory.TrimEnd('\\'));
        Assert.Equal(
            "D:\\CodexProjects\\LightStickyNote\\artifacts\\LightStickyNote-win-x64-portable\\Launch-LightStickyNote.cmd",
            paths.LauncherScriptPath);
        Assert.Equal(
            "D:\\CodexProjects\\LightStickyNote\\artifacts\\LightStickyNote-win-x64-portable\\tools\\Run.ps1",
            paths.RunScriptPath);
    }
}
