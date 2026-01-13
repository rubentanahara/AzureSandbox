using AzureOpenAI.Services.MCP;
using AzureOpenAI.Services.RAG;
using AzureOpenAI.Services.VectorStore;

using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AzureOpenAI.UI;

public class ConsoleUI(
    Kernel kernel,
    IChatCompletionService chatService,
    IEmbeddingGenerator<string, Embedding<float>> embeddingService,
    IVectorStore vectorStore)
{
    private readonly Kernel _kernel = kernel;
    private readonly IChatCompletionService _chatService = chatService;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingService = embeddingService;
    private readonly IVectorStore _vectorStore = vectorStore;

    private RagService? _ragService;
    private McpClient? _mcpClient;

    public async Task RunAsync()
    {
        ShowHeader();
        ShowMenu();

        var userInput = Console.ReadLine()?.Trim().ToUpper();

        switch (userInput)
        {
            case "RAG":
            case "1":
                _ragService ??= new RagService(_chatService, _embeddingService, _vectorStore);
                await DemoRunner.RunRAGDemo(_ragService);
                break;

            case "MCP":
            case "2":
                if (_mcpClient == null)
                {
                    _mcpClient = new McpClient(_kernel, _chatService);
                    await _mcpClient.InitializeAsync();
                }
                await DemoRunner.RunMCPDemo(_mcpClient);
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
        Console.WriteLine("║      Support Ticket System - RAG vs MCP Demo         ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════╝");
        Console.WriteLine();
    }

    private static void ShowMenu()
    {
        Console.WriteLine("Select a mode to run:");
        Console.WriteLine();
        Console.WriteLine("  1. RAG - Semantic search using vector embeddings");
        Console.WriteLine("           Best for: Conceptual queries, natural language");
        Console.WriteLine();
        Console.WriteLine("  2. MCP - LLM-powered structured queries via MCP protocol");
        Console.WriteLine("           Best for: Exact filtering, specific criteria");
        Console.WriteLine();
        Console.Write("Enter your choice (1-2 or RAG/MCP): ");
    }
}