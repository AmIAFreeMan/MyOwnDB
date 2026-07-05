
using ferretDB.Core.Common;
using ferretDB.Core.Parsing;

namespace ferretDB.Core.Storage;

/// <summary>
/// Represents a physical database table, managing its schema, its physical file (via Pager), 
/// and tracking how many rows currently exist.
/// </summary>
public class Table : IDisposable
{
    public string Name { get; }
    public Pager Pager { get; }
    public IReadOnlyList<ColumnDefinition> Columns { get; }

    /// <summary>
    /// The exact number of bytes a single row takes up on disk.
    /// </summary>
    public uint RowSize { get; }

    /// <summary>
    /// The total number of rows currently stored in this table.
    /// </summary>
    public uint RowCount { get; set; }

    public Table(string name, IReadOnlyList<ColumnDefinition> columns)
    {
        Name = name;
        Columns = columns;

        // Every table gets its own physical file on disk (e.g., "users.db")
        Pager = new Pager($"{name}.db");

        // Calculate how much space a row takes up
        RowSize = CalculateRowSize(columns);

        // For this MVP, we calculate the existing row count based on the physical file size.
        // If the file is 1000 bytes and each row is 100 bytes, we have 10 rows.
        var fileInfo = new FileInfo($"{name}.db");
        if (fileInfo.Exists && RowSize > 0)
        {
            RowCount = (uint)(fileInfo.Length / RowSize);
        }
        else
        {
            RowCount = 0;
        }
    }

    /// <summary>
    /// Calculates the exact byte-width of a row based on its data types.
    /// </summary>
    private uint CalculateRowSize(IReadOnlyList<ColumnDefinition> columns)
    {
        uint size = 0;

        foreach (var col in columns)
        {
            size += col.DataType switch
            {
                FerretDataType.Boolean => 1,      // 1 byte
                FerretDataType.Integer => 4,      // 4 bytes (standard 32-bit int)

                // For a beginner MVP database, variable-length strings are incredibly complex.
                // We simplify this by allocating a fixed 255 bytes for all TEXT columns.
                FerretDataType.Text => 255,

                _ => throw new FerretException($"Unknown size for type {col.DataType}", FerretErrorCode.StorageError)
            };
        }

        return size;
    }

    /// <summary>
    /// Safely shuts down the table, forcing the Pager to save memory to disk.
    /// </summary>
    public void Dispose()
    {
        Pager.Dispose();
    }
}