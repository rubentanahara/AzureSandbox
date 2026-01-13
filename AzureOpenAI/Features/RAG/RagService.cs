using AzureOpenAI.Models;
using AzureOpenAI.Services.VectorStore;

using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AzureOpenAI.Features.RAG;

public class RagService(
    IChatCompletionService chatService,
    IEmbeddingGenerator<string, Embedding<float>> embeddingService,
    IVectorStore<SupportTicket> vectorStore)
{
    private readonly IChatCompletionService _chatService = chatService;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingService = embeddingService;
    private readonly IVectorStore<SupportTicket> _vectorStore = vectorStore;
    private readonly List<SupportTicket> _tickets = SampleTickets.GetSampleData();

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

            var embeddings = await _embeddingService.GenerateAsync([text]);
            await _vectorStore.UpsertAsync(
                ticket.Id.ToString(),
                embeddings[0].Vector,
                ticket
            );
        }

        Console.WriteLine($"✅ Indexed {_tickets.Count} tickets into vector store");
    }

    public async Task<List<SupportTicket>> SearchTicketsInternalAsync(string query, int topK = 3)
    {
        var queryEmbeddings = await _embeddingService.GenerateAsync([query]);
        var queryEmbedding = queryEmbeddings[0].Vector;
        return await _vectorStore.SearchAsync(queryEmbedding, topK);
    }

    public async Task<string> SearchTicketsAsync(string query)
    {
        Console.WriteLine($"\n🔍 RAG Search: \"{query}\"");
        Console.WriteLine("─────────────────────────────────────");

        var similarTickets = await SearchTicketsInternalAsync(query);

        if (similarTickets.Count == 0)
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