namespace ferretDB.Core.Common;

/// <summary>
/// The complete vocabulary that the ferretDB Lexer understands.
/// </summary>
public enum TokenType
{
    // DML & DDL Keywords
    Select,
    From,
    Where,
    Insert,
    Into,
    Values,
    Create,
    Table,

    // Data Type Keywords (Lexer spots these, DataTypeMapper converts them)
    IntKeyword,
    TextKeyword,
    BoolKeyword,

    // Symbols & Operators
    Asterisk,       // *
    Comma,          // ,
    OpenParen,      // (
    CloseParen,     // )
    Semicolon,      // ;
    Equals,         // =

    // Literals & Identifiers
    Identifier,     // e.g., table names like 'users' or column names like 'id'
    StringLiteral,  // e.g., 'John Doe'
    NumericLiteral, // e.g., 42

    // System
    EndOfFile       // Signals the parser that the query is finished
}