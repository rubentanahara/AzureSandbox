using System.ComponentModel;

using AzureOpenAI.Models;
using AzureOpenAI.Services.RAG;

using Microsoft.SemanticKernel;

namespace AzureOpenAI.Services.Hybrid;

/// <summary>
/// Semantic Kernel plugin that exposes RAG search as a tool
/// </summary>
public class RagPlugin(RagService ragService)
{
    private readonly RagService _ragService = ragService;

    [KernelFunction]
    [Description("Search for support tickets using semantic/conceptual similarity. Use this when the user asks about themes, topics, or problems (e.g., 'authentication issues', 'billing problems', 'what are users complaining about'). Returns tickets with similar meanings, not exact matches.")]
    public async Task<string> SemanticSearch(
        [Description("The search query describing what to look for conceptually")] string query)
    {
        // Get similar tickets from RAG
        var tickets = await _ragService.SearchTicketsInternalAsync(query);

        if (tickets.Count == 0)
        {
            return "No semantically similar tickets found.";
        }

        // Format results for LLM consumption
        return FormatTicketsForTool(tickets);
    }

    private static string FormatTicketsForTool(List<SupportTicket> tickets)
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
