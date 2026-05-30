using LightStickyNote.App.Data;
using Microsoft.Data.Sqlite;

namespace LightStickyNote.App.Tests;

public sealed class DatabaseInitializerTests : IDisposable
{
    private readonly string _rootDirectory;

    public DatabaseInitializerTests()
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
    public async Task InitializeAsync_CreatesExpectedTables()
    {
        var databasePath = Path.Combine(_rootDirectory, "init.db");
        var initializer = new DatabaseInitializer(databasePath);

        await initializer.InitializeAsync();

        await using var connection = new SqliteConnection($"Data Source={databasePath}");
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY name;";

        var tables = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }

        Assert.Contains("notes", tables);
        Assert.Contains("note_items", tables);
        Assert.Contains("note_history", tables);
        Assert.Contains("tags", tables);
        Assert.Contains("note_tags", tables);
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
