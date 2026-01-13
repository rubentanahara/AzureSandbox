namespace AzureOpenAI.Models;

public sealed class KnowledgeDocument
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string Category { get; set; } = string.Empty; // e.g., "Network", "Security", "Email", "Hardware"
    public List<string> Tags { get; set; } = [];
    public DateTime LastUpdated { get; set; }
    public string Source { get; set; } = string.Empty; // File path, URL, or source identifier
    public string Author { get; set; } = string.Empty;

    // For chunked documents
    public string? ParentDocumentId { get; set; }
    public int? ChunkIndex { get; set; }
    public int? TotalChunks { get; set; }
}