namespace AzureOpenAI.Services.VectorStore;

public interface IVectorStore<TMetadata> where TMetadata : class
{
    Task UpsertAsync(string id, ReadOnlyMemory<float> embedding, TMetadata metadata);
    Task<List<TMetadata>> SearchAsync(ReadOnlyMemory<float> embedding, int topK);
}