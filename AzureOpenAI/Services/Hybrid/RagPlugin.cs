using System.ComponentModel;
using System.Text.Json;

using AzureOpenAI.Services.VectorStore;

using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;

namespace AzureOpenAI.Services.Hybrid;

public class RagPlugin(IEmbeddingGenerator embeddingService, IVectorStore vectorStore)
{
    private readonly IEmbeddingGenerator _embeddingService = embeddingService;
    private readonly IVectorStore _vectorStore = vectorStore;

    [KernelFunction, Description("Search tickets by semantic meaning (e.g., 'login issues', 'payment problems')")]
    public async Task<string> SemanticSearch(
        [Description("The conceptual query to search for")] string query,
        [Description("Number of results to return (default 5)")] int topK = 5)
    {
        Console.WriteLine($"  🔍 Tool: SemanticSearch(\"{query}\", topK={topK})");

        var embeddings = await _embeddingService.GenerateEmbeddingsAsync([query]);
        var results = await _vectorStore.SearchAsync(embeddings[0], topK);

        Console.WriteLine($"    → Found {results.Count} semantically similar tickets");

        return JsonSerializer.Serialize(results);
    }
}