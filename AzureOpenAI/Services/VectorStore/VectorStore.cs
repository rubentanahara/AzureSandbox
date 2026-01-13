namespace AzureOpenAI.Services.VectorStore;

public class InMemoryVectorStore<TMetadata> : IVectorStore<TMetadata> where TMetadata : class
{
    private readonly List<VectorEntry> _entries = [];

    public Task UpsertAsync(string id, ReadOnlyMemory<float> embedding, TMetadata metadata)
    {
        VectorEntry? existing = _entries.FirstOrDefault(e => e.Id == id);
        if (existing != null)
        {
            _entries.Remove(existing);
        }

        _entries.Add(new VectorEntry
        {
            Id = id,
            Embedding = embedding,
            Metadata = metadata
        });

        return Task.CompletedTask;
    }

    public Task<List<TMetadata>> SearchAsync(ReadOnlyMemory<float> queryEmbedding, int topK)
    {
        var results = _entries
            .Select(entry => new
            {
                Metadata = entry.Metadata,
                Similarity = CosineSimilarity(queryEmbedding, entry.Embedding)
            })
            .OrderByDescending(x => x.Similarity)
            .Take(topK)
            .Select(x => x.Metadata)
            .ToList();

        return Task.FromResult(results);
    }

    private static float CosineSimilarity(ReadOnlyMemory<float> a, ReadOnlyMemory<float> b)
    {
        ReadOnlySpan<float> aSpan = a.Span;
        ReadOnlySpan<float> bSpan = b.Span;

        float dot = 0, magA = 0, magB = 0;
        for (int i = 0; i < aSpan.Length; i++)
        {
            dot += aSpan[i] * bSpan[i];
            magA += aSpan[i] * aSpan[i];
            magB += bSpan[i] * bSpan[i];
        }

        return dot / (MathF.Sqrt(magA) * MathF.Sqrt(magB));
    }

    private class VectorEntry
    {
        public string Id { get; set; } = string.Empty;
        public ReadOnlyMemory<float> Embedding { get; set; }
        public TMetadata Metadata { get; set; } = default!;
    }
}