using AzureOpenAI.Features.MCP;
using AzureOpenAI.Features.RAG;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AzureOpenAI.Features.Hybrid;

public class HybridQueryService(
    Kernel kernel,
    IChatCompletionService chatService,
    KnowledgeRagService knowledgeRagService,
    McpClient mcpClient)
{
    private readonly Kernel _kernel = kernel;
    private readonly IChatCompletionService _chatService = chatService;
    private readonly KnowledgeRagService _knowledgeRagService = knowledgeRagService;
    private readonly McpClient _mcpClient = mcpClient;
    private bool _isInitialized = false;

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        Console.WriteLine("\n🚀 Initializing Hybrid Mode...");

        // Initialize Knowledge Base RAG
        Console.WriteLine("📚 Indexing IT knowledge base from markdown files...");
        var markdownPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "KnowledgeBase");
        await _knowledgeRagService.IndexKnowledgeBaseFromMarkdownAsync(markdownPath);

        // Initialize MCP
        await _mcpClient.InitializeAsync();

        // Register Knowledge RAG plugin
        var knowledgeRagPlugin = new KnowledgeRagPlugin(_knowledgeRagService);
        _kernel.Plugins.AddFromObject(knowledgeRagPlugin, "KnowledgeBase");

        Console.WriteLine($"✅ Hybrid mode initialized with Knowledge Base + {_mcpClient.ToolCount} MCP tools");
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
        You are an IT support assistant with access to two complementary systems:

        1. **KNOWLEDGE BASE** - Use KnowledgeBase.SemanticKnowledgeSearch for:
           - How-to questions and troubleshooting guidance
           - IT policies, procedures, and best practices
           - Technical documentation and setup guides
           - Security procedures (password resets, MFA, access management)
           - Common issues and their solutions
           - Examples:
             * "How do I reset a password?"
             * "VPN is not connecting, what should I do?"
             * "What's the software installation policy?"
             * "How to set up MFA?"
             * "Laptop is running slow, how to fix it?"
           - Returns: Relevant documentation, policies, and step-by-step guidance

        2. **TICKET MANAGEMENT** - Use McpTools for ticket operations:
           - Creating, updating, searching, and filtering support tickets
           - FilterByPriority: Filter tickets by priority level
           - FilterByStatus: Filter tickets by status
           - FilterByCategory: Filter by category (Billing, Technical, Account)
           - GetTicketById: Retrieve specific ticket
           - SearchByTag: Find tickets with specific tags
           - GetAllTickets, GetTicketsAfterDate: List tickets
           - Examples:
             * "Show me all open high-priority tickets"
             * "What's the status of ticket #1234?"
             * "List all billing-related tickets"
             * "Create a ticket for printer issue"

        **Decision Guidelines:**
        - User needs help/information/how-to → Use KnowledgeBase
        - User wants to manage/query/filter tickets → Use McpTools
        - User asks both (e.g., "Create a ticket for VPN issue and tell me how to fix it"):
          → Use KnowledgeBase for guidance + McpTools for ticket creation

        **Important:**
        - Always provide helpful, actionable guidance from the knowledge base
        - When creating tickets, include relevant context from knowledge base if available
        - If knowledge base doesn't have the answer, acknowledge it and suggest escalation
        - Reference specific policies or procedures when applicable

        Provide clear, professional responses that help users resolve issues efficiently.
        """;
    }
}