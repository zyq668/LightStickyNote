using System.IO;
using Microsoft.Data.Sqlite;

namespace LightStickyNote.App.Data;

public sealed class DatabaseInitializer
{
    private readonly string _databaseFilePath;

    public DatabaseInitializer(string databaseFilePath)
    {
        _databaseFilePath = databaseFilePath;
    }

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_databaseFilePath)!);

        await using var connection = new SqliteConnection($"Data Source={_databaseFilePath}");
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText =
            """
            PRAGMA journal_mode = WAL;
            PRAGMA foreign_keys = ON;

            CREATE TABLE IF NOT EXISTS notes (
                id TEXT PRIMARY KEY,
                title TEXT NOT NULL,
                created_at TEXT NOT NULL,
                updated_at TEXT NOT NULL,
                archived_at TEXT NULL,
                position_x REAL NULL,
                position_y REAL NULL,
                width REAL NOT NULL,
                height REAL NOT NULL,
                is_pinned INTEGER NOT NULL DEFAULT 1
            );

            CREATE TABLE IF NOT EXISTS note_items (
                id TEXT PRIMARY KEY,
                note_id TEXT NOT NULL,
                text TEXT NOT NULL,
                is_done INTEGER NOT NULL DEFAULT 0,
                sort_order INTEGER NOT NULL DEFAULT 0,
                created_at TEXT NOT NULL,
                updated_at TEXT NOT NULL,
                completed_at TEXT NULL,
                reminder_at TEXT NULL,
                reminder_notified_at TEXT NULL,
                FOREIGN KEY (note_id) REFERENCES notes(id) ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS idx_note_items_note_id_sort_order
                ON note_items(note_id, sort_order);

            CREATE TABLE IF NOT EXISTS note_history (
                id TEXT PRIMARY KEY,
                note_id TEXT NOT NULL,
                item_id TEXT NULL,
                action TEXT NOT NULL,
                old_value TEXT NULL,
                new_value TEXT NULL,
                created_at TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS tags (
                id TEXT PRIMARY KEY,
                name TEXT NOT NULL,
                color TEXT NULL
            );

            CREATE TABLE IF NOT EXISTS note_tags (
                note_id TEXT NOT NULL,
                tag_id TEXT NOT NULL,
                PRIMARY KEY (note_id, tag_id),
                FOREIGN KEY (note_id) REFERENCES notes(id) ON DELETE CASCADE,
                FOREIGN KEY (tag_id) REFERENCES tags(id) ON DELETE CASCADE
            );
            """;

        await command.ExecuteNonQueryAsync();
        await EnsureNoteItemsColumnsAsync(connection);
    }

    private static async Task EnsureNoteItemsColumnsAsync(SqliteConnection connection)
    {
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var command = connection.CreateCommand();
        command.CommandText = "PRAGMA table_info(note_items);";

        await using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                columns.Add(reader.GetString(1));
            }
        }

        if (!columns.Contains("reminder_at"))
        {
            var addReminderAt = connection.CreateCommand();
            addReminderAt.CommandText = "ALTER TABLE note_items ADD COLUMN reminder_at TEXT NULL;";
            await addReminderAt.ExecuteNonQueryAsync();
        }

        if (!columns.Contains("reminder_notified_at"))
        {
            var addReminderNotifiedAt = connection.CreateCommand();
            addReminderNotifiedAt.CommandText = "ALTER TABLE note_items ADD COLUMN reminder_notified_at TEXT NULL;";
            await addReminderNotifiedAt.ExecuteNonQueryAsync();
        }
    }
}
