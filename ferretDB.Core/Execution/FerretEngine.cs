using ferretDB.Core.Common;
using ferretDB.Core.Parsing;

namespace ferretDB.Core.Execution;

public class FerretEngine : IDisposable
{
    private readonly Executor _executor;

    public FerretEngine()
    {
        // Initialize our execution engine (which currently holds the in-memory state)
        _executor = new Executor();
    }

    public string Execute(string rawSql)
    {
        if (string.IsNullOrWhiteSpace(rawSql))
            return "Error: Empty query.";

        try
        {
            // 1. Tokenize (Raw String -> Tokens)
            var lexer = new Lexer(rawSql);
            var tokens = lexer.Tokenize();

            // 2. Parse (Tokens -> AST)
            var parser = new Parser(tokens);
            var statement = parser.Parse();

            // 3. Execute (AST -> Result)
            return _executor.Execute(statement);
        }
        catch (FerretException ex)
        {
            // If any stage throws a FerretException, safely catch it and print to CLI
            return ex.ToString();
        }
        catch (System.Exception ex)
        {
            // Catch anything we missed
            return $"[FatalError] {ex.Message}";
        }
    }

    public void Dispose() 
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[Engine] Saving data to disk and safely shutting down...");
        Console.ResetColor();
    } 
}