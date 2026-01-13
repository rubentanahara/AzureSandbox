using AzureOpenAI.Models;
using AzureOpenAI.Services.VectorStore;

using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;


namespace AzureOpenAI.Services.RAG;

public class RagService
{
    private readonly IChatCompletionService _chatService;
    private readonly IEmbeddingGenerator _embeddingService;
    private readonly IVectorStore _vectorStore;
    private readonly List<SupportTicket> _tickets;

    public RagService(
        IChatCompletionService chatService,
        IEmbeddingGenerator embeddingService,
        IVectorStore vectorStore)
    {
        _chatService = chatService;
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
        _tickets = SampleTickets.GetSampleData();
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
                Customer: {ticket.CustomerEmail}
                Created: {ticket.CreatedAt:yyyy-MM-dd HH:mm}
                """;

            var embeddings = await _embeddingService.GenerateEmbeddingsAsync([text]);
            await _vectorStore.UpsertAsync(
                ticket.Id.ToString(),
                embeddings[0],
                ticket
            );
        }

        Console.WriteLine($"✅ Indexed {_tickets.Count} tickets into vector store");
    }

    public async Task<string> SearchTicketsAsync(string query)
    {
        Console.WriteLine($"\n🔍 RAG Search: \"{query}\"");
        Console.WriteLine("─────────────────────────────────────");

        // Generate embedding for query
        var queryEmbeddings = await _embeddingService.GenerateEmbeddingsAsync([query]);
        var queryEmbedding = queryEmbeddings[0];

        // Search vector store
        var similarTickets = await _vectorStore.SearchAsync(queryEmbedding, topK: 3);

        if (!similarTickets.Any())
        {
            Console.WriteLine("❌ No relevant tickets found.");
            return "No relevant tickets found.";
        }

        // Build context
        var context = BuildContext(similarTickets);

        // Generate answer
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("""
            You are a support ticket assistant. Use the following ticket information to answer the question.
            Provide a helpful answer with specific ticket numbers and details.
            """);
        chatHistory.AddUserMessage($"""
            TICKETS:
            {context}
            
            QUESTION: {query}
            """);

        var response = await _chatService.GetChatMessageContentAsync(chatHistory);

        Console.WriteLine($"\n📊 Found {similarTickets.Count} similar tickets:");
        foreach (var ticket in similarTickets)
        {
            Console.WriteLine($"  • Ticket #{ticket.Id}: {ticket.Title} [{ticket.Priority}]");
        }
        Console.WriteLine($"\n💬 Response:\n{response.Content}");

        return response.Content ?? string.Empty;
    }

    private string BuildContext(List<SupportTicket> tickets)
    {
        return string.Join("\n\n", tickets.Select(t => $"""
            Ticket #{t.Id}
            Title: {t.Title}
            Description: {t.Description}
            Status: {t.Status}
            Priority: {t.Priority}
            Category: {t.Category}
            Created: {t.CreatedAt:yyyy-MM-dd HH:mm}
            Tags: {string.Join(", ", t.Tags)}
            """));
    }
}