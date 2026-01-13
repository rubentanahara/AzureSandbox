using AzureOpenAI.Services.MCP;
using AzureOpenAI.Services.RAG;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AzureOpenAI.Services.Hybrid;

public class HybridQueryService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private readonly RagService _ragService;
    private readonly McpClient _mcpClient;
    private bool _isInitialized = false;

    public HybridQueryService(
        Kernel kernel,
        IChatCompletionService chatService,
        RagService ragService,
        McpClient mcpClient)
    {
        _kernel = kernel;
        _chatService = chatService;
        _ragService = ragService;
        _mcpClient = mcpClient;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        Console.WriteLine("\n🚀 Initializing Hybrid Mode...");

        // Initialize RAG
        Console.WriteLine("📥 Indexing tickets for semantic search...");
        await _ragService.IndexTicketsAsync();

        // Initialize MCP
        await _mcpClient.InitializeAsync();

        // Register RAG plugin
        var ragPlugin = new RagPlugin(_ragService);
        _kernel.Plugins.AddFromObject(ragPlugin, "RAG");

        Console.WriteLine($"✅ Hybrid mode initialized with RAG + {_mcpClient.ToolCount} MCP tools");
        _isInitialized = true;
    }

    public async Task<string> QueryTicketsAsync(string query)
    {
        Console.WriteLine($"\n🔀 Hybrid Query: \"{query}\"");
        Console.WriteLine("─────────────────────────────────────");

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(GetHybridSystemPrompt());
        chatHistory.AddUserMessage(query);

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        Console.WriteLine("🤖 LLM analyzing query and selecting appropriate tools...");

        var response = await _chatService.GetChatMessageContentAsync(
            chatHistory,
            executionSettings,
            _kernel
        );

        Console.WriteLine($"\n💬 Final Response:\n{response.Content}");
        return response.Content ?? string.Empty;
    }

    private static string GetHybridSystemPrompt()
    {
        return """
            You are a support ticket assistant with access to two types of tools:

            1. **SEMANTIC SEARCH** - Use RAG.SemanticSearch for conceptual/broad queries:
               - When user asks about "problems", "issues", "topics", or themes
               - When looking for tickets with similar meanings or concepts
               - Examples: "authentication problems", "billing issues", "what are users experiencing"
               - Returns contextually similar tickets based on meaning

            2. **STRUCTURED FILTERS** - Use McpTools for exact criteria:
               - FilterByPriority: When user specifies priority (Low, Medium, High, Critical)
               - FilterByStatus: When user asks about status (Open, InProgress, Waiting, Resolved, Closed)
               - FilterByCategory: When user asks about category (Billing, Technical, Account)
               - GetTicketById: When user asks for specific ticket number
               - SearchByTag: When user asks for exact tags
               - GetTicketsAfterDate, GetAllTickets, GetTicketCountByStatus: For other structured queries
               - Returns exact matches

            3. **COMBINED APPROACH** - Use both when query has semantic AND structured aspects:
               - First use SemanticSearch to find conceptually relevant tickets
               - Then use MCP filters to refine results
               - Example: "critical authentication problems"
                 → SemanticSearch("authentication problems") AND FilterByPriority("Critical")
               - Example: "open billing issues"
                 → SemanticSearch("billing issues") AND FilterByStatus("Open")

            **Decision Guidelines:**
            - Semantic words: "problems", "issues", "about", "related to", "experiencing" → Use SemanticSearch
            - Exact criteria: "critical", "high priority", "open", "resolved", "ticket 1002" → Use MCP tools
            - Both present: Use BOTH tools and combine results

            After getting results, provide a clear, helpful summary. When using both tools, explain how you combined the results.
            """;
    }
}
