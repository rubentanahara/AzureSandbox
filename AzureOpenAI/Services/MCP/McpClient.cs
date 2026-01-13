using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AzureOpenAI.Services.MCP;

public class McpClient
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private readonly McpPlugin _plugin;

    public McpClient(Kernel kernel, IChatCompletionService chatService)
    {
        _kernel = kernel;
        _chatService = chatService;
        _plugin = new McpPlugin();

        _kernel.Plugins.AddFromObject(_plugin, "TicketTools");
    }

    public async Task<string> QueryTicketsAsync(string query)
    {
        Console.WriteLine($"\n🔧 MCP Query: \"{query}\"");
        Console.WriteLine("─────────────────────────────────────");

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("""
            You are a support ticket assistant. Use the available tools to answer questions about tickets.
            Always call the appropriate tool functions to get accurate data.
            """);
        chatHistory.AddUserMessage(query);

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var response = await _chatService.GetChatMessageContentAsync(
            chatHistory,
            executionSettings,
            _kernel
        );

        Console.WriteLine($"\n💬 Response:\n{response.Content}");

        return response.Content ?? string.Empty;
    }
}