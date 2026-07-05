

namespace ferretDB.Core.Common;

/// <summary>
/// Defines the core data types supported by the ferretDB engine.
/// </summary>
public enum FerretDataType
{
    Null,
    Integer,
    Text,
    Boolean
}

/// <summary>
/// A utility class to translate raw SQL strings into our engine's internal types.
/// </summary>
public static class DataTypeMapper
{
    /// <summary>
    /// Parses a SQL keyword (like 'INT' or 'VARCHAR') into a FerretDataType.
    /// </summary>
    public static FerretDataType FromSqlString(string sqlTypeName)
    {
        // Using a C# switch expression for clean pattern matching
        return sqlTypeName.ToUpperInvariant() switch
        {
            "INT" or "INTEGER" => FerretDataType.Integer,
            "TEXT" or "VARCHAR" => FerretDataType.Text,
            "BOOL" or "BOOLEAN" => FerretDataType.Boolean,

            // We use a generic Exception here for now, but this will eventually 
            // be upgraded to a custom FerretException once we build that file.
            _ => throw new System.Exception($"Syntax Error: Unsupported data type '{sqlTypeName}'")
        };
    }
}