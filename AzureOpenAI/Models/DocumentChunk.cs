namespace AzureOpenAI.Models;

public sealed class DocumentChunk
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public KnowledgeDocument SourceDocument { get; set; } = null!;
    public int ChunkIndex { get; set; }
}