using AzureOpenAI.Features.Hybrid;
using AzureOpenAI.Features.MCP;
using AzureOpenAI.Features.RAG;
using AzureOpenAI.Services.VectorStore;
using AzureOpenAI.Models;

using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AzureOpenAI.UI;

public class ConsoleUI(
    Kernel kernel,
    IChatCompletionService chatService,
    IEmbeddingGenerator<string, Embedding<float>> embeddingService,
    IVectorStore<KnowledgeDocument> knowledgeVectorStore,
    IVectorStore<SupportTicket> ticketVectorStore)
{
    private readonly Kernel _kernel = kernel;
    private readonly IChatCompletionService _chatService = chatService;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingService = embeddingService;
    private readonly IVectorStore<KnowledgeDocument> _knowledgeVectorStore = knowledgeVectorStore;
    private readonly IVectorStore<SupportTicket> _ticketVectorStore = ticketVectorStore;

    private KnowledgeRagService? _knowledgeRagService;
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
                _knowledgeRagService ??= new KnowledgeRagService(_chatService, _embeddingService, _knowledgeVectorStore);

                await DemoRunner.RunKnowledgeRAGDemo(_knowledgeRagService);
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

            case "HYBRID":
            case "3":
                _knowledgeRagService ??= new KnowledgeRagService(_chatService, _embeddingService, _knowledgeVectorStore);

                _mcpClient ??= new McpClient(_kernel, _chatService);

                var hybridService = new HybridQueryService(_kernel, _chatService, _knowledgeRagService, _mcpClient);
                await DemoRunner.RunHybridDemo(hybridService);
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
        Console.WriteLine("║      IT Support System - Knowledge Base + Tickets    ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════╝");
        Console.WriteLine();
    }

    private static void ShowMenu()
    {
        Console.WriteLine("Select a mode to run:");
        Console.WriteLine();
        Console.WriteLine("  1. KNOWLEDGE BASE - AI-powered IT help and troubleshooting");
        Console.WriteLine("              Uses: IT policies, procedures, troubleshooting guides");
        Console.WriteLine("              Best for: Getting answers to IT questions");
        Console.WriteLine();
        Console.WriteLine("  2. TICKET MANAGEMENT - Create and manage support tickets");
        Console.WriteLine("              Uses: SQLite database with structured queries");
        Console.WriteLine("              Best for: Creating, filtering, and managing tickets");
        Console.WriteLine();
        Console.WriteLine("  3. HYBRID - Complete IT Support Assistant");
        Console.WriteLine("              Combines: Knowledge Base + Ticket Management");
        Console.WriteLine("              Best for: Getting help AND managing tickets");
        Console.WriteLine();
        Console.Write("Enter your choice (1-3): ");
    }
}