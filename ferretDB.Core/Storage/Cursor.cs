namespace ferretDB.Core.Storage;

/// <summary>
/// Represents a pointer to a specific location (row) within a table.
/// Used to read from or write to the physical storage layer.
/// </summary>
public class Cursor
{
    private readonly Table _table;

    /// <summary>
    /// The current row number the cursor is pointing to.
    /// </summary>
    public uint RowNum { get; private set; }

    /// <summary>
    /// True if the cursor has moved past the last row of the table.
    /// </summary>
    public bool EndOfTable { get; private set; }

    // Private constructor. We use static factory methods to create cursors.
    private Cursor(Table table, uint rowNum)
    {
        _table = table;
        RowNum = rowNum;
    }

    /// <summary>
    /// Creates a cursor pointing to the very first row of the table (used for SELECT).
    /// </summary>
    public static Cursor Start(Table table)
    {
        var cursor = new Cursor(table, 0);
        cursor.EndOfTable = (table.RowCount == 0);
        return cursor;
    }

    /// <summary>
    /// Creates a cursor pointing just past the last row (used for INSERT).
    /// </summary>
    public static Cursor End(Table table)
    {
        return new Cursor(table, table.RowCount)
        {
            EndOfTable = true
        };
    }

    /// <summary>
    /// Moves the cursor to the next row in the table.
    /// </summary>
    public void Advance()
    {
        if (EndOfTable) return;

        RowNum++;

        if (RowNum >= _table.RowCount)
        {
            EndOfTable = true;
        }
    }

    /// <summary>
    /// Calculates the exact physical byte offset for the current row.
    /// The Pager will use this to know where to seek in the physical file.
    /// </summary>
    public uint CursorByteOffset()
    {
        // Example: If each row is exactly 256 bytes long, and we are on row 3,
        // we need to tell the Pager to skip to byte 768 in the file.
        return RowNum * _table.RowSize;
    }
}