using AzureOpenAI.Models;
using AzureOpenAI.Services.MCP;
using AzureOpenAI.Services.VectorStore;

using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;


namespace AzureOpenAI.Services.Hybrid;

public class HybridService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private readonly IEmbeddingGenerator _embeddingService;
    private readonly IVectorStore _vectorStore;
    private readonly List<SupportTicket> _tickets;

    public HybridService(
        Kernel kernel,
        IChatCompletionService chatService,
        IEmbeddingGenerator embeddingService,
        IVectorStore vectorStore)
    {
        _kernel = kernel;
        _chatService = chatService;
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
        _tickets = SampleTickets.GetSampleData();

        _kernel.Plugins.AddFromObject(new McpPlugin(), "TicketTools");
        _kernel.Plugins.AddFromObject(new RagPlugin(_embeddingService, _vectorStore), "SemanticSearch");
    }

    public async Task IndexTicketsAsync()
    {
        foreach (var ticket in _tickets)
        {
            var text = $"""
                Ticket #{ticket.Id}: {ticket.Title}
                Description: {ticket.Description}
                Category: {ticket.Category}
                Status: {ticket.Status}
                Priority: {ticket.Priority}
                Tags: {string.Join(", ", ticket.Tags)}
                """;

            var embeddings = await _embeddingService.([text]);
            await _vectorStore.UpsertAsync(ticket.Id.ToString(), embeddings[0], ticket);
        }

        Console.WriteLine($"✅ Indexed {_tickets.Count} tickets");
    }

    public async Task<string> QueryTicketsHybridAsync(string query)
    {
        Console.WriteLine($"\n🚀 HYBRID Query: \"{query}\"");
        Console.WriteLine("─────────────────────────────────────");

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("""
            You are an intelligent support ticket assistant with access to both structured and semantic search.
            
            Use MCP tools (FilterByStatus, FilterByPriority, FilterByCategory, GetTicketById) for:
            - Exact status, priority, category filtering
            - Specific ticket IDs
            - Date ranges
            
            Use SemanticSearch for:
            - Conceptual queries ("authentication problems", "billing issues")
            - When user describes a problem in natural language
            
            You can combine multiple tools! For example:
            - "Critical authentication issues" = SemanticSearch("authentication") + FilterByPriority("Critical")
            
            Always use the most appropriate combination of tools.
            """);
        chatHistory.AddUserMessage(query);

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var response = await _chatService.GetChatMessageContentAsync(
            chatHistory,
            executionSettings,
            _kernel
        );

        Console.WriteLine($"\n💬 Final Response:\n{response.Content}");

        return response.Content ?? string.Empty;
    }
}