using AzureOpenAI.UI;
using AzureOpenAI.Models;

using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using AzureOpenAI.Services.VectorStore;

string apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? throw new InvalidOperationException("Please set the AZURE_OPENAI_API_KEY environment variable.");
string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("Please set the AZURE_OPENAI_ENDPOINT environment variable.");
string chatDeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME") ?? throw new InvalidOperationException("Please set the AZURE_OPENAI_CHAT_DEPLOYMENT_NAME environment variable.");
string embeddingDeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_EMBEDDING_DEPLOYMENT_NAME") ?? throw new InvalidOperationException("Please set the AZURE_OPENAI_EMBEDDING_DEPLOYMENT_NAME environment variable.");

Console.WriteLine("🔧 Initializing Azure OpenAI...");
Console.WriteLine($"   Endpoint: {endpoint}");
Console.WriteLine($"   Chat Model: {chatDeploymentName}");
Console.WriteLine($"   Embedding Model: {embeddingDeploymentName}");

Kernel kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(chatDeploymentName, endpoint, apiKey)
    .AddAzureOpenAIEmbeddingGenerator(embeddingDeploymentName, endpoint, apiKey)
    .Build();

IChatCompletionService chatService = kernel.GetRequiredService<IChatCompletionService>();
IEmbeddingGenerator<string, Embedding<float>> embeddingService = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

var knowledgeVectorStore = new InMemoryVectorStore<KnowledgeDocument>();
var ticketVectorStore = new InMemoryVectorStore<SupportTicket>();

ConsoleUI consoleApp = new(kernel, chatService, embeddingService, knowledgeVectorStore, ticketVectorStore);
await consoleApp.RunAsync();