using AzureOpenAI.Services.Hybrid;
using AzureOpenAI.Services.MCP;
using AzureOpenAI.Services.RAG;

namespace AzureOpenAI.UI;

public static class DemoRunner
{
    public static async Task RunRAGDemo(RagService ragService)
    {
        Console.WriteLine("\n🔍 RAG (Retrieval-Augmented Generation) Mode");
        Console.WriteLine("════════════════════════════════════════════════");
        Console.WriteLine("Using vector embeddings to find semantically similar tickets");
        Console.WriteLine();

        Console.WriteLine("📥 Indexing tickets...");
        await ragService.IndexTicketsAsync();

        Console.WriteLine("\n💡 Try queries like:");
        Console.WriteLine("  • 'Find tickets about authentication problems'");
        Console.WriteLine("  • 'Show me billing issues'");
        Console.WriteLine("  • 'What problems are users having with login?'");

        await RunInteractiveLoop(ragService.SearchTicketsAsync);
    }

    public static async Task RunMCPDemo(McpClient mcpClient)
    {
        Console.WriteLine("\n🔧 MCP (Model Context Protocol) Mode");
        Console.WriteLine("════════════════════════════════════════════════");
        Console.WriteLine("Using LLM to intelligently select MCP tools for structured queries");
        Console.WriteLine();

        Console.WriteLine("💡 Try queries like:");
        Console.WriteLine("\n  Read operations:");
        Console.WriteLine("    • 'Show all critical priority tickets'");
        Console.WriteLine("    • 'What's ticket 1002?'");
        Console.WriteLine("    • 'Show me open tickets'");
        Console.WriteLine("    • 'How many tickets per status?'");
        Console.WriteLine("\n  Write operations:");
        Console.WriteLine("    • 'Create a ticket for login issue from john@example.com'");
        Console.WriteLine("    • 'Update ticket 1001 status to InProgress'");
        Console.WriteLine("    • 'Assign ticket 1002 to billing-team'");
        Console.WriteLine("    • 'Mark ticket 1003 as resolved'");

        await RunInteractiveLoop(mcpClient.QueryTicketsAsync);
    }

    public static async Task RunHybridDemo(HybridQueryService hybridService)
    {
        Console.WriteLine("\n🔀 HYBRID Mode (RAG + MCP)");
        Console.WriteLine("════════════════════════════════════════════════");
        Console.WriteLine("LLM intelligently selects RAG and/or MCP tools based on query");
        Console.WriteLine();

        await hybridService.InitializeAsync();

        Console.WriteLine("\n💡 Try queries that combine semantic and structured aspects:");
        Console.WriteLine("\n  Semantic queries (uses RAG):");
        Console.WriteLine("    • 'What authentication problems are users having?'");
        Console.WriteLine("    • 'Show me billing complaints'");
        Console.WriteLine("\n  Structured queries (uses MCP):");
        Console.WriteLine("    • 'Show all critical priority tickets'");
        Console.WriteLine("    • 'What tickets are open?'");
        Console.WriteLine("\n  Hybrid queries (uses BOTH):");
        Console.WriteLine("    • 'Critical authentication problems'");
        Console.WriteLine("    • 'Open billing issues'");
        Console.WriteLine("    • 'High priority login problems created recently'");

        await RunInteractiveLoop(hybridService.QueryTicketsAsync);
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
}