using AzureOpenAI.Models;
using AzureOpenAI.Services.VectorStore;
using AzureOpenAI.Services.DocumentProcessing;

using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Text;

namespace AzureOpenAI.Features.RAG;

public class KnowledgeRagService(
    IChatCompletionService chatService,
    IEmbeddingGenerator<string, Embedding<float>> embeddingService,
    IVectorStore<KnowledgeDocument> vectorStore)
{
    private readonly IChatCompletionService _chatService = chatService;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingService = embeddingService;
    private readonly IVectorStore<KnowledgeDocument> _vectorStore = vectorStore;

    public async Task IndexKnowledgeBaseFromMarkdownAsync(string markdownFolderPath)
    {
        Console.WriteLine($"📂 Loading knowledge documents from: {markdownFolderPath}");

        var loader = new DocumentLoader();
        var documents = await loader.LoadFromDirectoryAsync(markdownFolderPath);

        if (documents.Count == 0)
        {
            Console.WriteLine("⚠️  No documents found in the specified folder.");
            return;
        }

        Console.WriteLine($"✅ Loaded {documents.Count} documents from markdown files");
        await IndexKnowledgeBaseAsync(documents);
    }

    public async Task IndexKnowledgeBaseAsync(List<KnowledgeDocument> documents)
    {
        Console.WriteLine($"📚 Indexing {documents.Count} knowledge documents...");

        int totalChunks = 0;

#pragma warning disable SKEXP0050 // TextChunker is experimental
        for (int docIndex = 0; docIndex < documents.Count; docIndex++)
        {
            var document = documents[docIndex];
            Console.Write($"   [{docIndex + 1}/{documents.Count}] {document.Title}...");

            // Step 1: Split into lines (preserves markdown structure)
            List<string> lines = TextChunker.SplitMarkDownLines(document.Content, 800);

            // Step 2: Split into paragraphs with smaller chunks and overlap
            List<string> chunks = TextChunker.SplitMarkdownParagraphs(lines, 400, 80);
            totalChunks += chunks.Count;

            Console.Write($" ({chunks.Count} chunks) ");

            for (int chunkIndex = 0; chunkIndex < chunks.Count; chunkIndex++)
            {
                var chunkContent = chunks[chunkIndex];

                // Create text for embedding that includes metadata for better search
                var textForEmbedding = $"""
                Title: {document.Title}
                Category: {document.Category}
                Type: {document.DocumentType}
                Tags: {string.Join(", ", document.Tags)}

                Content:
                {chunkContent}
                """;

                // Generate embedding
                var embeddings = await _embeddingService.GenerateAsync([textForEmbedding]);

                // Create a document chunk with metadata for storage
                var chunkId = $"{document.Id}-chunk-{chunkIndex}";
                var chunkDocument = new KnowledgeDocument
                {
                    Id = chunkId,
                    Title = document.Title,
                    Content = chunkContent,
                    DocumentType = document.DocumentType,
                    Category = document.Category,
                    Tags = document.Tags,
                    LastUpdated = document.LastUpdated,
                    Source = document.Source,
                    Author = document.Author,
                    ParentDocumentId = document.Id,
                    ChunkIndex = chunkIndex,
                    TotalChunks = chunks.Count
                };

                // Store in vector store
                await _vectorStore.UpsertAsync(
                    chunkId,
                    embeddings[0].Vector,
                    chunkDocument
                );

                Console.Write(".");
            }

            Console.WriteLine(" ✓");
        }
#pragma warning restore SKEXP0050

        Console.WriteLine($"✅ Indexed {documents.Count} documents ({totalChunks} chunks) into vector store");
    }

    public async Task<List<KnowledgeDocument>> SearchKnowledgeAsync(string query, int topK = 3)
    {
        var queryEmbeddings = await _embeddingService.GenerateAsync([query]);
        var queryEmbedding = queryEmbeddings[0].Vector;
        return await _vectorStore.SearchAsync(queryEmbedding, topK);
    }

    public async Task<string> GetKnowledgeAnswerAsync(string query)
    {
        Console.WriteLine($"\n🔍 Knowledge Search: \"{query}\"");
        Console.WriteLine("─────────────────────────────────────");

        var relevantDocs = await SearchKnowledgeAsync(query, topK: 5);

        if (relevantDocs.Count == 0)
        {
            Console.WriteLine("❌ No relevant knowledge articles found.");
            return "I couldn't find any relevant information in the knowledge base for this query.";
        }

        // AUGMENT: Build context from relevant documents
        var context = BuildKnowledgeContext(relevantDocs);

        // GENERATE: Create chat prompt with context and user query
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("""
        You are an IT support assistant with access to a knowledge base.
        Use the provided knowledge articles to answer the user's question accurately and helpfully.

        Guidelines:
        - Provide specific, actionable guidance based on the knowledge articles
        - Reference the relevant document titles when applicable
        - If the knowledge base doesn't fully answer the question, say so
        - Provide step-by-step instructions when available
        - Include relevant warnings or security considerations
        """);

        chatHistory.AddUserMessage($"""
        KNOWLEDGE BASE ARTICLES:
        {context}

        USER QUESTION: {query}

        Please provide a helpful answer based on the knowledge base articles above.
        """);

        var response = await _chatService.GetChatMessageContentAsync(chatHistory);

        Console.WriteLine($"\n📊 Found {relevantDocs.Count} relevant knowledge articles:");
        foreach (var doc in relevantDocs)
        {
            var chunkInfo = doc.ChunkIndex.HasValue ? $" (chunk {doc.ChunkIndex + 1})" : "";
            Console.WriteLine($"  • {doc.Title} [{doc.Category}]{chunkInfo}");
        }
        Console.WriteLine($"\n💬 Response:\n{response.Content}");

        return response.Content ?? string.Empty;
    }

    private string BuildKnowledgeContext(List<KnowledgeDocument> documents)
    {
        return string.Join("\n\n---\n\n", documents.Select(doc => $"""
        Document: {doc.Title}
        Category: {doc.Category}
        Type: {doc.DocumentType}
        Tags: {string.Join(", ", doc.Tags)}

        Content:
        {doc.Content}
        """));
    }

    public async Task<string> SemanticKnowledgeSearch(string query)
    {
        return await GetKnowledgeAnswerAsync(query);
    }
}