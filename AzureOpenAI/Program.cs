using AzureOpenAI.UI;

using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.InMemory;

string apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? throw new InvalidOperationException("Please set the AZURE_OPENAI_API_KEY environment variable.");
string endpoint = "https://open-ai-sandbox-1234.openai.azure.com/";
string chatDeploymentName = "o4-mini";
string embeddingDeploymentName = "text-embedding-3-small";

Kernel kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(chatDeploymentName, apiKey, endpoint)
    .AddAzureOpenAIEmbeddingGenerator(embeddingDeploymentName, apiKey, endpoint)
    .Build();

IChatCompletionService chatService = kernel.GetRequiredService<IChatCompletionService>();
IEmbeddingGenerator<string, Embedding<float>> embeddingService = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
InMemoryVectorStore vectorStore = new();

ConsoleUI consoleApp = new(kernel, chatService, embeddingService, vectorStore);
await consoleApp.RunAsync();
