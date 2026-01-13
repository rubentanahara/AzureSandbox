using AzureOpenAI.Services.Hybrid;
using AzureOpenAI.Services.MCP;
using AzureOpenAI.Services.RAG;
using AzureOpenAI.Services.VectorStore;

using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AzureOpenAI.UI;

public static class DemoRunner
{
    public static async Task RunRAGDemo(
        IChatCompletionService chatService,
        IEmbeddingGenerator embeddingService,
        IVectorStore vectorStore)
    {
        Console.WriteLine("\n🔍 RAG (Retrieval-Augmented Generation) Mode");
        Console.WriteLine("════════════════════════════════════════════════");

        var ragService = new RagService(chatService, embeddingService, vectorStore);

        Console.WriteLine("\n📥 Indexing tickets...");
        await ragService.IndexTicketsAsync();

        await RunInteractiveLoop(ragService.SearchTicketsAsync);
    }

    public static async Task RunMCPDemo(Kernel kernel, IChatCompletionService chatService)
    {
        Console.WriteLine("\n🔧 MCP (Model Context Protocol) Mode");
        Console.WriteLine("════════════════════════════════════════════════");

        var mcpClient = new McpClient(kernel, chatService);

        await RunInteractiveLoop(mcpClient.QueryTicketsAsync);
    }

    public static async Task RunHybridDemo(
        Kernel kernel,
        IChatCompletionService chatService,
        IEmbeddingGenerator embeddingService,
        IVectorStore vectorStore)
    {
        Console.WriteLine("\n🚀 HYBRID (RAG + MCP) Mode");
        Console.WriteLine("════════════════════════════════════════════════");

        var hybridService = new HybridService(kernel, chatService, embeddingService, vectorStore);

        Console.WriteLine("\n📥 Indexing tickets for RAG...");
        await hybridService.IndexTicketsAsync();

        await RunInteractiveLoop(hybridService.QueryTicketsHybridAsync);
    }

    public static async Task RunFullDemo(
        Kernel kernel,
        IChatCompletionService chatService,
        IEmbeddingGenerator embeddingService,
        IVectorStore vectorStore)
    {
        Console.WriteLine("\n╔════════════════════════════════════════════════════════╗");
        Console.WriteLine("║             FULL COMPARISON DEMO                       ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════╝");

        // Initialize all services
        var ragService = new RagService(chatService, embeddingService, vectorStore);
        var mcpClient = new McpClient(kernel, chatService);
        var hybridService = new HybridService(kernel, chatService, embeddingService, vectorStore);

        // Index tickets
        Console.WriteLine("\n📥 Indexing tickets...");
        await ragService.IndexTicketsAsync();
        await hybridService.IndexTicketsAsync();

        // Demo queries
        var demoQueries = new[]
        {
            new
            {
                Title = "Semantic Understanding (RAG excels)",
                Query = "Find tickets about authentication problems",
                Service = "RAG"
            },
            new
            {
                Title = "Exact Filtering (MCP excels)",
                Query = "Show all critical priority tickets",
                Service = "MCP"
            },
            new
            {
                Title = "Complex Query (Hybrid excels)",
                Query = "Find high priority authentication issues from the last 24 hours",
                Service = "HYBRID"
            }
        };

        foreach (var demo in demoQueries)
        {
            Console.WriteLine("\n" + new string('═', 80));
            Console.WriteLine($"📌 DEMO: {demo.Title}");
            Console.WriteLine($"Query: \"{demo.Query}\"");
            Console.WriteLine($"Best Approach: {demo.Service}");
            Console.WriteLine(new string('═', 80));

            switch (demo.Service)
            {
                case "RAG":
                    await ragService.SearchTicketsAsync(demo.Query);
                    break;
                case "MCP":
                    await mcpClient.QueryTicketsAsync(demo.Query);
                    break;
                case "HYBRID":
                    await hybridService.QueryTicketsHybridAsync(demo.Query);
                    break;
            }

            await Task.Delay(2000);
        }

        PrintComparisonTable();
    }

    private static async Task RunInteractiveLoop(Func<string, Task<string>> queryHandler)
    {
        while (true)
        {
            Console.WriteLine("\n" + new string('─', 60));
            Console.Write("💬 Enter your query (or 'exit' to quit): ");
            var query = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(query) || query.Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;

            await queryHandler(query);
        }

        Console.WriteLine("\n👋 Goodbye!");
    }

    private static void PrintComparisonTable()
    {
        Console.WriteLine("\n\n╔════════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                         APPROACH COMPARISON                                ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════════════╝\n");

        var table = new[]
        {
            new { Query = "Authentication problems", RAG = "✅ Excellent", MCP = "❌ Needs exact tag", Hybrid = "✅ Excellent" },
            new { Query = "Critical tickets", RAG = "⚠️  Inefficient", MCP = "✅ Perfect", Hybrid = "✅ Perfect" },
            new { Query = "Ticket #1002", RAG = "⚠️  Works but slow", MCP = "✅ Instant", Hybrid = "✅ Instant" },
            new { Query = "Recent billing issues", RAG = "⚠️  No date filter", MCP = "⚠️  No semantic", Hybrid = "✅ Both!" },
        };

        Console.WriteLine("┌─────────────────────────────────┬──────────────────┬──────────────────┬──────────────────┐");
        Console.WriteLine("│ Query Type                      │ RAG Only         │ MCP Only         │ Hybrid (RAG+MCP) │");
        Console.WriteLine("├─────────────────────────────────┼──────────────────┼──────────────────┼──────────────────┤");

        foreach (var row in table)
        {
            Console.WriteLine($"│ {row.Query,-31} │ {row.RAG,-16} │ {row.MCP,-16} │ {row.Hybrid,-16} │");
        }

        Console.WriteLine("└─────────────────────────────────┴──────────────────┴──────────────────┴──────────────────┘");

        Console.WriteLine("\n💡 Key Takeaways:");
        Console.WriteLine("  • RAG: Best for semantic/conceptual queries");
        Console.WriteLine("  • MCP: Best for exact/structured filtering");
        Console.WriteLine("  • Hybrid: Combines both for optimal results");
    }
}