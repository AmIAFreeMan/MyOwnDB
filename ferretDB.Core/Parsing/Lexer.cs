
using System.Text;
using ferretDB.Core.Common;

namespace ferretDB.Core.Parsing;

/// <summary>
/// Reads a raw SQL string character by character and groups them into meaningful Tokens.
/// </summary>
public class Lexer
{
    private readonly string _source;
    private int _position;
    private int _line = 1;
    private int _column = 1;

    public Lexer(string source)
    {
        _source = source ?? string.Empty;
    }

    /// <summary>
    /// Scans the entire input string and returns a list of Tokens.
    /// </summary>
    public IReadOnlyList<Token> Tokenize()
    {
        var tokens = new List<Token>();

        while (!IsAtEnd())
        {
            SkipWhitespace();
            if (IsAtEnd()) break;

            int startColumn = _column;
            char c = Advance();

            // Match single-character symbols
            switch (c)
            {
                case '*': tokens.Add(new Token(TokenType.Asterisk, "*", _line, startColumn)); break;
                case ',': tokens.Add(new Token(TokenType.Comma, ",", _line, startColumn)); break;
                case '(': tokens.Add(new Token(TokenType.OpenParen, "(", _line, startColumn)); break;
                case ')': tokens.Add(new Token(TokenType.CloseParen, ")", _line, startColumn)); break;
                case ';': tokens.Add(new Token(TokenType.Semicolon, ";", _line, startColumn)); break;
                case '=': tokens.Add(new Token(TokenType.Equals, "=", _line, startColumn)); break;

                // Match strings enclosed in single quotes
                case '\'':
                    tokens.Add(ReadString(startColumn));
                    break;

                default:
                    // Match numbers
                    if (char.IsDigit(c))
                    {
                        tokens.Add(ReadNumber(c, startColumn));
                    }
                    // Match keywords and identifiers (e.g., SELECT, users, id)
                    else if (char.IsLetter(c) || c == '_')
                    {
                        tokens.Add(ReadIdentifierOrKeyword(c, startColumn));
                    }
                    // Throw our custom exception for anything else
                    else
                    {
                        throw new FerretException(
                            $"Unexpected character '{c}' at Line {_line}, Column {startColumn}",
                            FerretErrorCode.SyntaxError);
                    }
                    break;
            }
        }

        tokens.Add(new Token(TokenType.EndOfFile, "", _line, _column));
        return tokens;
    }

    // ==========================================
    // HELPER METHODS
    // ==========================================

    private bool IsAtEnd() => _position >= _source.Length;

    private char Peek() => IsAtEnd() ? '\0' : _source[_position];

    private char Advance()
    {
        char c = _source[_position++];
        _column++;
        return c;
    }

    private void SkipWhitespace()
    {
        while (!IsAtEnd() && char.IsWhiteSpace(Peek()))
        {
            char c = Advance();
            if (c == '\n')
            {
                _line++;
                _column = 1; // Reset column on new line
            }
        }
    }

    private Token ReadString(int startColumn)
    {
        var sb = new StringBuilder();
        while (!IsAtEnd() && Peek() != '\'')
        {
            sb.Append(Advance());
        }

        if (IsAtEnd())
        {
            throw new FerretException(
                $"Unterminated string literal starting at Line {_line}, Column {startColumn}",
                FerretErrorCode.SyntaxError);
        }

        Advance(); // Consume the closing quote
        return new Token(TokenType.StringLiteral, sb.ToString(), _line, startColumn);
    }

    private Token ReadNumber(char firstChar, int startColumn)
    {
        var sb = new StringBuilder();
        sb.Append(firstChar);

        while (!IsAtEnd() && char.IsDigit(Peek()))
        {
            sb.Append(Advance());
        }

        return new Token(TokenType.NumericLiteral, sb.ToString(), _line, startColumn);
    }

    private Token ReadIdentifierOrKeyword(char firstChar, int startColumn)
    {
        var sb = new StringBuilder();
        sb.Append(firstChar);

        // Identifiers can contain letters, numbers, or underscores (e.g., user_1)
        while (!IsAtEnd() && (char.IsLetterOrDigit(Peek()) || Peek() == '_'))
        {
            sb.Append(Advance());
        }

        string lexeme = sb.ToString();

        // Map the string to a Keyword, or default to an Identifier
        TokenType type = lexeme.ToUpperInvariant() switch
        {
            "SELECT" => TokenType.Select,
            "FROM" => TokenType.From,
            "WHERE" => TokenType.Where,
            "INSERT" => TokenType.Insert,
            "INTO" => TokenType.Into,
            "VALUES" => TokenType.Values,
            "CREATE" => TokenType.Create,
            "TABLE" => TokenType.Table,
            "INT" or "INTEGER" => TokenType.IntKeyword,
            "TEXT" or "VARCHAR" => TokenType.TextKeyword,
            "BOOL" or "BOOLEAN" => TokenType.BoolKeyword,
            _ => TokenType.Identifier
        };

        return new Token(type, lexeme, _line, startColumn);
    }
}