
using System.Text;
using ferretDB.Core.Common;
using ferretDB.Core.Parsing;

namespace ferretDB.Core.Execution;

/// <summary>
/// Takes a parsed Abstract Syntax Tree (AST) statement and executes it against the data storage.
/// </summary>
public class Executor
{
    // ==========================================
    // TEMPORARY IN-MEMORY STORAGE
    // (To be replaced by the Pager/Storage Engine)
    // ==========================================
    private readonly Dictionary<string, List<ColumnDefinition>> _tableSchemas = new();
    private readonly Dictionary<string, List<IReadOnlyList<Expression>>> _tableData = new();

    /// <summary>
    /// Routes the AST node to the correct execution logic using C# pattern matching.
    /// </summary>
    public string Execute(Statement statement)
    {
        return statement switch
        {
            CreateTableStatement createStmt => ExecuteCreateTable(createStmt),
            InsertStatement insertStmt => ExecuteInsert(insertStmt),
            SelectStatement selectStmt => ExecuteSelect(selectStmt),

            _ => throw new FerretException("Unsupported execution statement.", FerretErrorCode.GenericError)
        };
    }

    private string ExecuteCreateTable(CreateTableStatement stmt)
    {
        if (_tableSchemas.ContainsKey(stmt.TableName))
        {
            throw new FerretException($"Table '{stmt.TableName}' already exists.", FerretErrorCode.GenericError);
        }

        // Save the schema and initialize an empty data list
        _tableSchemas[stmt.TableName] = stmt.Columns.ToList();
        _tableData[stmt.TableName] = new List<IReadOnlyList<Expression>>();

        return $"Success: Table '{stmt.TableName}' created with {stmt.Columns.Count} column(s).";
    }

    private string ExecuteInsert(InsertStatement stmt)
    {
        if (!_tableSchemas.ContainsKey(stmt.TableName))
        {
            throw new FerretException($"Table '{stmt.TableName}' does not exist.", FerretErrorCode.TableNotFound);
        }

        var columns = _tableSchemas[stmt.TableName];

        // Validate column count
        if (stmt.Values.Count != columns.Count)
        {
            throw new FerretException(
                $"Column count mismatch. Expected {columns.Count}, got {stmt.Values.Count}.",
                FerretErrorCode.ConstraintViolation);
        }

        // Note: A full database would validate data types here (e.g., ensuring you don't put Text into an Int column)
        _tableData[stmt.TableName].Add(stmt.Values);

        return $"Success: 1 row inserted into '{stmt.TableName}'.";
    }

    private string ExecuteSelect(SelectStatement stmt)
    {
        if (!_tableSchemas.ContainsKey(stmt.TableName))
        {
            throw new FerretException($"Table '{stmt.TableName}' does not exist.", FerretErrorCode.TableNotFound);
        }

        var columns = _tableSchemas[stmt.TableName];
        var rows = _tableData[stmt.TableName];

        var sb = new StringBuilder();
        sb.AppendLine($"--- {stmt.TableName} ({rows.Count} rows) ---");

        // 1. Print Headers
        if (stmt.Columns.Count == 1 && stmt.Columns[0] == "*")
        {
            sb.AppendLine(string.Join(" | ", columns.Select(c => c.Name)));
        }
        else
        {
            sb.AppendLine(string.Join(" | ", stmt.Columns));
        }

        sb.AppendLine(new string('-', 40));

        // 2. Print Data Rows
        foreach (var row in rows)
        {
            var rowStrings = row.Select(val =>
                val is LiteralExpression lit ? lit.Token.Lexeme : "NULL"
            );

            sb.AppendLine(string.Join(" | ", rowStrings));
        }

        return sb.ToString().TrimEnd();
    }
}