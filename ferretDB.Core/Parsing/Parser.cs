
using ferretDB.Core.Common;

namespace ferretDB.Core.Parsing;

/// <summary>
/// Converts a flat sequence of Tokens into a structured Abstract Syntax Tree (AST).
/// </summary>
public class Parser
{
    private readonly IReadOnlyList<Token> _tokens;
    private int _position;

    public Parser(IReadOnlyList<Token> tokens)
    {
        _tokens = tokens;
    }

    /// <summary>
    /// Begins the parsing process. For this MVP, we parse a single statement.
    /// </summary>
    public Statement Parse()
    {
        try
        {
            return ParseStatement();
        }
        catch (System.Exception ex) when (ex is not FerretException)
        {
            // Wrap any unexpected C# errors into our custom engine exception
            throw new FerretException("An unexpected error occurred during parsing.", ex, FerretErrorCode.SyntaxError);
        }
    }

    // ==========================================
    // STATEMENT ROUTING
    // ==========================================

    private Statement ParseStatement()
    {
        if (Match(TokenType.Create)) return ParseCreateTable();
        if (Match(TokenType.Select)) return ParseSelect();
        if (Match(TokenType.Insert)) return ParseInsert();

        throw ParseError(Peek(), "Expected a valid SQL statement (CREATE, SELECT, or INSERT).");
    }

    // ==========================================
    // DDL & DML PARSERS
    // ==========================================

    // Parses: CREATE TABLE <name> (<col1> <type>, <col2> <type>);
    private Statement ParseCreateTable()
    {
        Consume(TokenType.Table, "Expect 'TABLE' after 'CREATE'.");

        Token tableNameToken = Consume(TokenType.Identifier, "Expect table name.");
        Consume(TokenType.OpenParen, "Expect '(' before table columns.");

        var columns = new List<ColumnDefinition>();

        do
        {
            Token columnName = Consume(TokenType.Identifier, "Expect column name.");

            // The next token should be a data type keyword
            Token typeToken = Advance();
            FerretDataType dataType = DataTypeMapper.FromSqlString(typeToken.Lexeme);

            columns.Add(new ColumnDefinition(columnName.Lexeme, dataType));

        } while (Match(TokenType.Comma));

        Consume(TokenType.CloseParen, "Expect ')' after column definitions.");
        Consume(TokenType.Semicolon, "Expect ';' at the end of the statement.");

        return new CreateTableStatement(tableNameToken.Lexeme, columns);
    }

    // Parses: SELECT <col1>, <col2> FROM <name>;
    private Statement ParseSelect()
    {
        var columns = new List<string>();

        if (Match(TokenType.Asterisk))
        {
            columns.Add("*");
        }
        else
        {
            do
            {
                columns.Add(Consume(TokenType.Identifier, "Expect column name in SELECT clause.").Lexeme);
            } while (Match(TokenType.Comma));
        }

        Consume(TokenType.From, "Expect 'FROM' after SELECT columns.");
        Token tableNameToken = Consume(TokenType.Identifier, "Expect table name after FROM.");
        Consume(TokenType.Semicolon, "Expect ';' at the end of the statement.");

        return new SelectStatement(tableNameToken.Lexeme, columns);
    }

    // Parses: INSERT INTO <name> VALUES (<val1>, <val2>);
    private Statement ParseInsert()
    {
        Consume(TokenType.Into, "Expect 'INTO' after 'INSERT'.");
        Token tableNameToken = Consume(TokenType.Identifier, "Expect table name.");

        Consume(TokenType.Values, "Expect 'VALUES' after table name.");
        Consume(TokenType.OpenParen, "Expect '(' before insert values.");

        var values = new List<Expression>();

        do
        {
            values.Add(ParseExpression());
        } while (Match(TokenType.Comma));

        Consume(TokenType.CloseParen, "Expect ')' after insert values.");
        Consume(TokenType.Semicolon, "Expect ';' at the end of the statement.");

        return new InsertStatement(tableNameToken.Lexeme, values);
    }

    // ==========================================
    // EXPRESSION PARSER
    // ==========================================

    private Expression ParseExpression()
    {
        if (Match(TokenType.StringLiteral))
            return new LiteralExpression(Previous(), FerretDataType.Text);

        if (Match(TokenType.NumericLiteral))
            return new LiteralExpression(Previous(), FerretDataType.Integer);

        throw ParseError(Peek(), "Expected an expression (string or number).");
    }

    // ==========================================
    // PARSER HELPERS
    // ==========================================

    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }
        return false;
    }

    private Token Consume(TokenType type, string errorMessage)
    {
        if (Check(type)) return Advance();
        throw ParseError(Peek(), errorMessage);
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) _position++;
        return Previous();
    }

    private bool IsAtEnd() => Peek().Type == TokenType.EndOfFile;

    private Token Peek() => _tokens[_position];

    private Token Previous() => _tokens[_position - 1];

    private FerretException ParseError(Token token, string message)
    {
        string location = token.Type == TokenType.EndOfFile
            ? "at end of query"
            : $"at '{token.Lexeme}' (Line {token.Line}, Col {token.Column})";

        return new FerretException($"Syntax Error {location}: {message}", FerretErrorCode.SyntaxError);
    }
}