// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 AgentEval Contributors

using System.Text;

namespace AgentEval.Samples;

/// <summary>
/// Main program - interactive menu to run samples.
/// </summary>
public static class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        PrintBanner();

        if (!AIConfig.IsConfigured)
        {
            AIConfig.PrintMissingCredentialsWarning();
        }

        if (args.Length > 0 && int.TryParse(args[0], out var sampleNumber))
        {
            await RunSample(sampleNumber);
            return;
        }

        while (true)
        {
            PrintMenu();
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input) || input.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("\n👋 Goodbye!\n");
                break;
            }

            if (int.TryParse(input, out var choice))
            {
                await RunSample(choice);
            }
            else
            {
                Console.WriteLine("❌ Invalid choice. Enter a number or 'q' to quit.\n");
            }
        }
    }

    private static async Task RunSample(int sampleNumber)
    {
        Console.WriteLine();
        
        try
        {
            switch (sampleNumber)
            {
                case 1:
                    await Sample01_HelloWorld.RunAsync();
                    break;
                case 2:
                    await Sample02_AgentWithOneTool.RunAsync();
                    break;
                case 3:
                    await Sample03_AgentWithMultipleTools.RunAsync();
                    break;
                case 4:
                    await Sample04_PerformanceMetrics.RunAsync();
                    break;
                case 5:
                    await Sample05_RAGEvaluation.RunAsync();
                    break;
                default:
                    Console.WriteLine($"❌ Sample {sampleNumber} not found.\n");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ Error: {ex.Message}");
            Console.ResetColor();
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }

    private static void PrintBanner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════════════════════════╗
║                                                                               ║
║     █████╗  ██████╗ ███████╗███╗   ██╗████████╗███████╗██╗   ██╗ █████╗ ██╗   ║
║    ██╔══██╗██╔════╝ ██╔════╝████╗  ██║╚══██╔══╝██╔════╝██║   ██║██╔══██╗██║   ║
║    ███████║██║  ███╗█████╗  ██╔██╗ ██║   ██║   █████╗  ██║   ██║███████║██║   ║
║    ██╔══██║██║   ██║██╔══╝  ██║╚██╗██║   ██║   ██╔══╝  ╚██╗ ██╔╝██╔══██║██║   ║
║    ██║  ██║╚██████╔╝███████╗██║ ╚████║   ██║   ███████╗ ╚████╔╝ ██║  ██║███████╗
║    ╚═╝  ╚═╝ ╚═════╝ ╚══════╝╚═╝  ╚═══╝   ╚═╝   ╚══════╝  ╚═══╝  ╚═╝  ╚═╝╚══════╝
║                                                                               ║
║              The .NET-native AI Agent Testing Framework                       ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
");
        Console.ResetColor();
    }

    private static void PrintMenu()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│                    📚 SAMPLES MENU                          │");
        Console.WriteLine("├─────────────────────────────────────────────────────────────┤");
        Console.ResetColor();
        
        Console.WriteLine("│  1. 🌍 Hello World        - Minimal AgentEval test          │");
        Console.WriteLine("│  2. 🔧 Agent + One Tool   - Tool tracking assertions        │");
        Console.WriteLine("│  3. 🔧 Agent + Multi Tool - Tool ordering & timeline        │");
        Console.WriteLine("│  4. ⚡ Performance        - Latency, cost, TTFT metrics     │");
        Console.WriteLine("│  5. 📊 RAG Evaluation     - Faithfulness metric demo        │");
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("├─────────────────────────────────────────────────────────────┤");
        Console.WriteLine("│  q. Quit                                                    │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘");
        Console.ResetColor();
        
        Console.Write("\nEnter your choice: ");
    }
}
