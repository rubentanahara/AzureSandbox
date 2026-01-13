using AzureOpenAI.Features.RAG;

using Microsoft.SemanticKernel;

using System.ComponentModel;

namespace AzureOpenAI.Features.Hybrid;

public class KnowledgeRagPlugin(KnowledgeRagService ragService)
{
    private readonly KnowledgeRagService _ragService = ragService;

    [KernelFunction("SemanticKnowledgeSearch")]
    [Description("Search the IT knowledge base for information about policies, procedures, troubleshooting guides, and technical documentation. Use this when users ask 'how to' questions, need help with issues, or want to know about IT policies.")]
    public async Task<string> SemanticKnowledgeSearch(
        [Description("The search query describing what information is needed from the knowledge base")]
        string query)
    {
        return await _ragService.SemanticKnowledgeSearch(query);
    }
}