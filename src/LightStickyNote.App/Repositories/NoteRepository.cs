using LightStickyNote.App.Models;
using Microsoft.Data.Sqlite;

namespace LightStickyNote.App.Repositories;

public sealed class NoteRepository
{
    private readonly string _databaseFilePath;

    public NoteRepository(string databaseFilePath)
    {
        _databaseFilePath = databaseFilePath;
    }

    public async Task<Note> GetOrCreatePrimaryNoteAsync()
    {
        await using var connection = await OpenConnectionAsync();
        var existing = await LoadFirstNoteAsync(connection);
        if (existing is not null)
        {
            existing.Items = await LoadItemsAsync(connection, existing.Id);
            return existing;
        }

        var note = new Note();
        await SaveNoteAsync(note);
        return note;
    }

    public async Task SaveNoteAsync(Note note)
    {
        note.UpdatedAt = DateTimeOffset.UtcNow;

        await using var connection = await OpenConnectionAsync();
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync();

        var upsertNote = connection.CreateCommand();
        upsertNote.Transaction = transaction;
        upsertNote.CommandText =
            """
            INSERT INTO notes (id, title, created_at, updated_at, archived_at, position_x, position_y, width, height, is_pinned)
            VALUES (@id, @title, @created_at, @updated_at, @archived_at, @position_x, @position_y, @width, @height, @is_pinned)
            ON CONFLICT(id) DO UPDATE SET
                title = excluded.title,
                updated_at = excluded.updated_at,
                archived_at = excluded.archived_at,
                position_x = excluded.position_x,
                position_y = excluded.position_y,
                width = excluded.width,
                height = excluded.height,
                is_pinned = excluded.is_pinned;
            """;
        BindNote(upsertNote, note);
        await upsertNote.ExecuteNonQueryAsync();

        var deleteItems = connection.CreateCommand();
        deleteItems.Transaction = transaction;
        deleteItems.CommandText = "DELETE FROM note_items WHERE note_id = @note_id;";
        deleteItems.Parameters.AddWithValue("@note_id", note.Id);
        await deleteItems.ExecuteNonQueryAsync();

        foreach (var item in note.Items.OrderBy(x => x.SortOrder))
        {
            item.NoteId = note.Id;
            item.UpdatedAt = DateTimeOffset.UtcNow;

            var insertItem = connection.CreateCommand();
            insertItem.Transaction = transaction;
            insertItem.CommandText =
                """
                INSERT INTO note_items (id, note_id, text, is_done, sort_order, created_at, updated_at, completed_at)
                VALUES (@id, @note_id, @text, @is_done, @sort_order, @created_at, @updated_at, @completed_at);
                """;
            BindItem(insertItem, item);
            await insertItem.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }

    private async Task<SqliteConnection> OpenConnectionAsync()
    {
        var connection = new SqliteConnection($"Data Source={_databaseFilePath}");
        await connection.OpenAsync();
        return connection;
    }

    private static async Task<Note?> LoadFirstNoteAsync(SqliteConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT id, title, created_at, updated_at, archived_at, position_x, position_y, width, height, is_pinned
            FROM notes
            WHERE archived_at IS NULL
            ORDER BY created_at
            LIMIT 1;
            """;

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new Note
        {
            Id = reader.GetString(0),
            Title = reader.GetString(1),
            CreatedAt = DateTimeOffset.Parse(reader.GetString(2)),
            UpdatedAt = DateTimeOffset.Parse(reader.GetString(3)),
            ArchivedAt = reader.IsDBNull(4) ? null : DateTimeOffset.Parse(reader.GetString(4)),
            PositionX = reader.IsDBNull(5) ? null : reader.GetDouble(5),
            PositionY = reader.IsDBNull(6) ? null : reader.GetDouble(6),
            Width = reader.GetDouble(7),
            Height = reader.GetDouble(8),
            IsPinned = reader.GetInt32(9) == 1
        };
    }

    private static async Task<List<NoteItem>> LoadItemsAsync(SqliteConnection connection, string noteId)
    {
        var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT id, note_id, text, is_done, sort_order, created_at, updated_at, completed_at
            FROM note_items
            WHERE note_id = @note_id
            ORDER BY sort_order, created_at;
            """;
        command.Parameters.AddWithValue("@note_id", noteId);

        var items = new List<NoteItem>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new NoteItem
            {
                Id = reader.GetString(0),
                NoteId = reader.GetString(1),
                Text = reader.GetString(2),
                IsDone = reader.GetInt32(3) == 1,
                SortOrder = reader.GetInt32(4),
                CreatedAt = DateTimeOffset.Parse(reader.GetString(5)),
                UpdatedAt = DateTimeOffset.Parse(reader.GetString(6)),
                CompletedAt = reader.IsDBNull(7) ? null : DateTimeOffset.Parse(reader.GetString(7))
            });
        }

        return items;
    }

    private static void BindNote(SqliteCommand command, Note note)
    {
        command.Parameters.AddWithValue("@id", note.Id);
        command.Parameters.AddWithValue("@title", note.Title);
        command.Parameters.AddWithValue("@created_at", note.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("@updated_at", note.UpdatedAt.ToString("O"));
        command.Parameters.AddWithValue("@archived_at", (object?)note.ArchivedAt?.ToString("O") ?? DBNull.Value);
        command.Parameters.AddWithValue("@position_x", (object?)note.PositionX ?? DBNull.Value);
        command.Parameters.AddWithValue("@position_y", (object?)note.PositionY ?? DBNull.Value);
        command.Parameters.AddWithValue("@width", note.Width);
        command.Parameters.AddWithValue("@height", note.Height);
        command.Parameters.AddWithValue("@is_pinned", note.IsPinned ? 1 : 0);
    }

    private static void BindItem(SqliteCommand command, NoteItem item)
    {
        command.Parameters.AddWithValue("@id", item.Id);
        command.Parameters.AddWithValue("@note_id", item.NoteId);
        command.Parameters.AddWithValue("@text", item.Text);
        command.Parameters.AddWithValue("@is_done", item.IsDone ? 1 : 0);
        command.Parameters.AddWithValue("@sort_order", item.SortOrder);
        command.Parameters.AddWithValue("@created_at", item.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("@updated_at", item.UpdatedAt.ToString("O"));
        command.Parameters.AddWithValue("@completed_at", (object?)item.CompletedAt?.ToString("O") ?? DBNull.Value);
    }
}
