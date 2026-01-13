using AzureOpenAI.Features.Hybrid;
using AzureOpenAI.Features.MCP;
using AzureOpenAI.Features.RAG;

namespace AzureOpenAI.UI;

public static class DemoRunner
{
    public static async Task RunKnowledgeRAGDemo(KnowledgeRagService knowledgeRagService)
    {
        Console.WriteLine("\n📚 Knowledge Base RAG Mode");
        Console.WriteLine("════════════════════════════════════════════════");
        Console.WriteLine("Using vector embeddings to search IT knowledge base");
        Console.WriteLine();

        Console.WriteLine("📥 Indexing knowledge base from markdown files...");
        var markdownPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "KnowledgeBase");
        await knowledgeRagService.IndexKnowledgeBaseFromMarkdownAsync(markdownPath);

        Console.WriteLine("\n💡 Try queries like:");
        Console.WriteLine("  • 'How do I reset a password?'");
        Console.WriteLine("  • 'VPN troubleshooting steps'");
        Console.WriteLine("  • 'What is the software installation policy?'");
        Console.WriteLine("  • 'How to set up MFA?'");
        Console.WriteLine("  • 'Laptop is running slow, what should I do?'");

        await RunInteractiveLoop(knowledgeRagService.GetKnowledgeAnswerAsync);
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
        Console.WriteLine("\n🔀 HYBRID Mode (Knowledge Base + Ticket Management)");
        Console.WriteLine("════════════════════════════════════════════════");
        Console.WriteLine("LLM intelligently uses Knowledge Base for help and MCP tools for tickets");
        Console.WriteLine();

        await hybridService.InitializeAsync();

        Console.WriteLine("\n💡 Try these types of queries:");
        Console.WriteLine("\n  Knowledge Base queries (how-to, troubleshooting):");
        Console.WriteLine("    • 'How do I reset a user password?'");
        Console.WriteLine("    • 'VPN won't connect, what should I do?'");
        Console.WriteLine("    • 'What's the software installation policy?'");
        Console.WriteLine("    • 'How to troubleshoot email issues?'");
        Console.WriteLine("\n  Ticket Management queries:");
        Console.WriteLine("    • 'Show all critical priority tickets'");
        Console.WriteLine("    • 'Create a ticket for printer issue on Floor 2'");
        Console.WriteLine("    • 'What tickets are open in the Technical category?'");
        Console.WriteLine("\n  Combined queries (both knowledge + tickets):");
        Console.WriteLine("    • 'Help me fix VPN issues and create a ticket'");
        Console.WriteLine("    • 'Show me password reset procedure and related open tickets'");

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

            var response = await queryHandler(query);

            Console.WriteLine("\n📝 Response:");
            Console.WriteLine(response);
        }

        Console.WriteLine("\n👋 Goodbye!");
    }
}