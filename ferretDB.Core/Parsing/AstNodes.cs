
using ferretDB.Core.Common;

namespace ferretDB.Core.Parsing;

/// <summary>
/// The base type for all nodes in the Abstract Syntax Tree.
/// </summary>
public abstract record AstNode;

/// <summary>
/// Represents a complete, executable SQL command (e.g., SELECT, INSERT, CREATE).
/// </summary>
public abstract record Statement : AstNode;

/// <summary>
/// Represents an evaluated value or condition (e.g., a number, a string, or a math operation).
/// </summary>
public abstract record Expression : AstNode;

// ==========================================
// DATA DEFINITION LANGUAGE (DDL)
// ==========================================

/// <summary>
/// e.g., CREATE TABLE users (id INT, name TEXT);
/// </summary>
public record CreateTableStatement(
    string TableName,
    IReadOnlyList<ColumnDefinition> Columns
) : Statement;

/// <summary>
/// Represents a single column in a CREATE TABLE statement.
/// </summary>
public record ColumnDefinition(
    string Name,
    FerretDataType DataType
) : AstNode;


// ==========================================
// DATA MANIPULATION LANGUAGE (DML)
// ==========================================

/// <summary>
/// e.g., SELECT id, name FROM users; 
/// (An empty list or a list containing "*" can represent 'SELECT *')
/// </summary>
public record SelectStatement(
    string TableName,
    IReadOnlyList<string> Columns
) : Statement;

/// <summary>
/// e.g., INSERT INTO users VALUES (1, 'Alice');
/// </summary>
public record InsertStatement(
    string TableName,
    IReadOnlyList<Expression> Values
) : Statement;


// ==========================================
// EXPRESSIONS
// ==========================================

/// <summary>
/// Represents a raw data value passed into a query.
/// </summary>
public record LiteralExpression(
    Token Token,
    FerretDataType DataType
) : Expression;