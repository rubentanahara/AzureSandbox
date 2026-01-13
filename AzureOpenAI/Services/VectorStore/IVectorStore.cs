using AzureOpenAI.Models;

namespace AzureOpenAI.Services.VectorStore;

public interface IVectorStore
{
    Task UpsertAsync(string id, ReadOnlyMemory<float> embedding, SupportTicket metadata);
    Task<List<SupportTicket>> SearchAsync(ReadOnlyMemory<float> embedding, int topK);
}