using System;
using System.IO;
using ferretDB.Core.Execution;

// 1. Setup the physical data directory based on our "configs"
// (In a full enterprise app, we'd use Microsoft.Extensions.Configuration to parse the JSON, 
// but for this MVP, we will keep it lightweight and hardcode the fallback).
string dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FerretData");

if (!Directory.Exists(dataDir))
{
    Directory.CreateDirectory(dataDir);
}

// Change the current working directory so the Pager saves .db files in the right folder
Directory.SetCurrentDirectory(dataDir);


// 2. Initialize the Database Engine
using var engine = new FerretEngine();


// 3. Print the Welcome Banner
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("======================================");
Console.WriteLine("        ferretDB - Version 0.1        ");
Console.WriteLine("======================================");
Console.ResetColor();
Console.WriteLine($"Data directory: {dataDir}");
Console.WriteLine("Type 'exit' or 'quit' to shut down.");
Console.WriteLine("Note: SQL statements must end with a semicolon (;)\n");


// 4. The REPL (Read-Evaluate-Print Loop)
while (true)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("ferretDB> ");
    Console.ResetColor();

    string input = Console.ReadLine();

    // Handle shutdown
    if (string.IsNullOrWhiteSpace(input)) continue;
    if (input.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase) ||
        input.Trim().Equals("quit", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Shutting down ferretDB. Goodbye!");
        break;
    }

    // Execute the SQL and measure the time it takes
    try
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();

        string result = engine.Execute(input);

        watch.Stop();

        Console.WriteLine(result);

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"Execution time: {watch.ElapsedMilliseconds} ms\n");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        // Catch any fatal CLI crashes that escaped the engine
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[Fatal CLI Error] {ex.Message}\n");
        Console.ResetColor();
    }
}