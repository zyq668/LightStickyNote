using LightStickyNote.App.Models;
using LightStickyNote.App.Services;

namespace LightStickyNote.App.Tests;

public sealed class SettingsServiceTests : IDisposable
{
    private readonly string _rootDirectory;

    public SettingsServiceTests()
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
    public async Task LoadAsync_ReturnsDefaults_WhenFileMissing()
    {
        var settingsPath = Path.Combine(_rootDirectory, "appsettings.json");
        var service = new SettingsService(settingsPath);

        var settings = await service.LoadAsync();

        Assert.True(settings.AlwaysOnTop);
        Assert.True(File.Exists(settingsPath));
    }

    [Fact]
    public async Task SaveAsync_PersistsConfiguredValues()
    {
        var settingsPath = Path.Combine(_rootDirectory, "appsettings.json");
        var service = new SettingsService(settingsPath);
        var settings = new AppSettings
        {
            AlwaysOnTop = false,
            Opacity = 0.9
        };

        await service.SaveAsync(settings);
        var reloaded = await service.LoadAsync();

        Assert.False(reloaded.AlwaysOnTop);
        Assert.Equal(0.9, reloaded.Opacity);
    }

    [Theory]
    [InlineData(0.2, 0.45)]
    [InlineData(0.82, 0.82)]
    [InlineData(1.4, 1.0)]
    public void Opacity_ClampsToVisibleRange(double value, double expected)
    {
        var settings = new AppSettings
        {
            Opacity = value
        };

        Assert.Equal(expected, settings.Opacity);
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(3, 3)]
    [InlineData(3.4, 3)]
    [InlineData(20, 8)]
    public void EdgeRevealWidth_ClampsToUsableRange(double value, double expected)
    {
        var settings = new AppSettings
        {
            EdgeRevealWidth = value
        };

        Assert.Equal(expected, settings.EdgeRevealWidth);
    }

    [Theory]
    [InlineData(100, 300)]
    [InlineData(900, 900)]
    [InlineData(5000, 3000)]
    public void EdgeHideDelayMilliseconds_ClampsToUsableRange(int value, int expected)
    {
        var settings = new AppSettings
        {
            EdgeHideDelayMilliseconds = value
        };

        Assert.Equal(expected, settings.EdgeHideDelayMilliseconds);
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
