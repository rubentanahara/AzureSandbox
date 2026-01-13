using AzureOpenAI.Services.VectorStore;

using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AzureOpenAI.UI;

public class ConsoleUI(
    Kernel kernel,
    IChatCompletionService chatService,
    IEmbeddingGenerator embeddingService,
    IVectorStore vectorStore)
{
    private readonly Kernel _kernel = kernel;
    private readonly IChatCompletionService _chatService = chatService;
    private readonly IEmbeddingGenerator _embeddingService = embeddingService;
    private readonly IVectorStore _vectorStore = vectorStore;

    public async Task RunAsync()
    {
        ShowHeader();
        ShowMenu();

        var userInput = Console.ReadLine()?.Trim().ToUpper();

        switch (userInput)
        {
            case "RAG":
            case "1":
                await DemoRunner.RunRAGDemo(_chatService, _embeddingService, _vectorStore);
                break;

            case "MCP":
            case "2":
                await DemoRunner.RunMCPDemo(_kernel, _chatService);
                break;

            case "HYBRID":
            case "3":
                await DemoRunner.RunHybridDemo(_kernel, _chatService, _embeddingService, _vectorStore);
                break;

            case "DEMO":
            case "4":
                await DemoRunner.RunFullDemo(_kernel, _chatService, _embeddingService, _vectorStore);
                break;

            default:
                Console.WriteLine("❌ Invalid selection. Please run the program again.");
                break;
        }
    }

    private static void ShowHeader()
    {
        Console.Clear();
        Console.WriteLine("╔════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Support Ticket System - RAG vs MCP vs HYBRID Demo  ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════╝");
        Console.WriteLine();
    }

    private static void ShowMenu()
    {
        Console.WriteLine("Select a mode to run:");
        Console.WriteLine();
        Console.WriteLine("  1. RAG     - Semantic search using vector embeddings");
        Console.WriteLine("  2. MCP     - Structured queries using function calling");
        Console.WriteLine("  3. HYBRID  - Combined RAG + MCP approach");
        Console.WriteLine("  4. DEMO    - Run all three with comparison");
        Console.WriteLine();
        Console.Write("Enter your choice (1-4 or RAG/MCP/HYBRID/DEMO): ");
    }
}