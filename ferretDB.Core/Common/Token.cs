namespace ferretDB.Core.Common;

/// <summary>
/// Represents a single, meaningful chunk of a SQL query.
/// </summary>
/// <param name="Type">The categorized type of the token.</param>
/// <param name="Lexeme">The exact string value extracted from the query.</param>
/// <param name="Line">The line number where the token was found (useful for error messages).</param>
/// <param name="Column">The column position where the token starts.</param>
public record Token(TokenType Type, string Lexeme, int Line, int Column);