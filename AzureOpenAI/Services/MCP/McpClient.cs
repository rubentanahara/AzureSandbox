using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AzureOpenAI.Services.MCP;

public class McpClient : IDisposable
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private Process? _mcpServerProcess;
    private StreamWriter? _serverStdin;
    private StreamReader? _serverStdout;
    private StreamReader? _serverStderr;
    private Task? _stderrReaderTask;
    private int _requestId = 0;
    private readonly List<ToolDefinition> _availableTools = [];

    public int ToolCount => _availableTools.Count;

    public McpClient(Kernel kernel, IChatCompletionService chatService)
    {
        _kernel = kernel;
        _chatService = chatService;
    }

    public async Task InitializeAsync()
    {
        Console.WriteLine("\n🚀 Starting MCP Server subprocess...");

        // Start MCP server as subprocess
        var mcpServerPath = FindMcpServerExecutable();

        _mcpServerProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = mcpServerPath,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        _mcpServerProcess.Start();
        _serverStdin = _mcpServerProcess.StandardInput;
        _serverStdout = _mcpServerProcess.StandardOutput;
        _serverStderr = _mcpServerProcess.StandardError;

        // Read stderr in background for logging
        _stderrReaderTask = Task.Run(async () =>
        {
            while (!_mcpServerProcess.HasExited)
            {
                var line = await _serverStderr.ReadLineAsync();
                if (!string.IsNullOrEmpty(line))
                {
                    Console.WriteLine($"[MCP] {line}");
                }
            }
        });

        // Initialize MCP connection
        await SendRequest("initialize", new
        {
            protocolVersion = "2024-11-05",
            capabilities = new { },
            clientInfo = new
            {
                name = "AzureOpenAI-McpClient",
                version = "1.0.0"
            }
        });

        await SendRequest("ping", new { });

        // Get available tools from MCP Server
        var toolsResponse = await SendRequest("tools/list", new { });
        if (toolsResponse != null)
        {
            var result = toolsResponse["result"];
            var tools = result?["tools"]?.AsArray();
            if (tools != null)
            {
                foreach (var tool in tools)
                {
                    var toolName = tool?["name"]?.ToString() ?? "";
                    _availableTools.Add(new ToolDefinition
                    {
                        Name = toolName,
                        Description = tool?["description"]?.ToString() ?? "",
                        InputSchema = tool?["inputSchema"]
                    });
                    Console.WriteLine($"   → Tool: {toolName}");
                }
            }
        }

        Console.WriteLine($"✅ MCP Server initialized with {_availableTools.Count} tools");

        // Register MCP tools with Semantic Kernel using plugin class
        var mcpPlugin = new McpToolsPlugin(this);
        _kernel.Plugins.AddFromObject(mcpPlugin, "McpTools");
        Console.WriteLine($"✅ Registered {_availableTools.Count} MCP tools with LLM");
    }

    public async Task<string> QueryTicketsAsync(string query)
    {
        Console.WriteLine($"\n🔧 MCP Query: \"{query}\"");
        Console.WriteLine("─────────────────────────────────────");

        // Build chat with user query
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("""
            You are a support ticket assistant with access to MCP tools for querying tickets.

            Use the available tools to answer user questions accurately.
            Always call the appropriate tool based on what the user is asking for.
            After getting results, provide a clear, helpful summary to the user.
            """);
        chatHistory.AddUserMessage(query);

        // Let LLM automatically invoke MCP tools via Semantic Kernel
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        Console.WriteLine("🤖 LLM analyzing query and selecting appropriate MCP tools...");

        var response = await _chatService.GetChatMessageContentAsync(
            chatHistory,
            executionSettings,
            _kernel
        );

        return response.Content ?? string.Empty;
    }

    // Public method to allow McpToolsPlugin to call MCP tools
    public async Task<string?> CallMcpToolPublic(string toolName, object arguments)
    {
        return await CallMcpTool(toolName, arguments);
    }

    private async Task<string?> CallMcpTool(string toolName, object arguments)
    {
        Console.WriteLine($"  🔧 MCP Tool Call: {toolName}({JsonSerializer.Serialize(arguments)})");

        var response = await SendRequest("tools/call", new
        {
            name = toolName,
            arguments
        });

        if (response != null)
        {
            var result = response["result"];
            var content = result?["content"]?.AsArray();
            if (content != null && content.Count > 0)
            {
                return content[0]?["text"]?.ToString();
            }
        }

        return null;
    }

    private async Task<JsonNode?> SendRequest(string method, object parameters)
    {
        if (_serverStdin == null || _serverStdout == null)
        {
            throw new InvalidOperationException("MCP Server not initialized");
        }

        var request = new
        {
            jsonrpc = "2.0",
            id = ++_requestId,
            method,
            @params = parameters
        };

        var requestJson = JsonSerializer.Serialize(request);
        await _serverStdin.WriteLineAsync(requestJson);
        await _serverStdin.FlushAsync();

        var responseJson = await _serverStdout.ReadLineAsync();
        // Console.WriteLine($"  📥 MCP Response: {responseJson}");

        return string.IsNullOrEmpty(responseJson) ? null : JsonNode.Parse(responseJson);
    }

    private static string FindMcpServerExecutable()
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var serverDll = Path.Combine(basePath, "mcp-server.dll");

        if (File.Exists(serverDll))
        {
            return serverDll;
        }

        // Development mode - use the project directly
        var projectPath = Path.Combine(Directory.GetCurrentDirectory(), "Services", "MCP", "McpServer", "AzureOpenAI.McpServer.csproj");
        if (File.Exists(projectPath))
        {
            return $"run --project \"{projectPath}\"";
        }

        throw new FileNotFoundException("MCP Server executable not found. Please build the McpServer project.");
    }

    public void Dispose()
    {
        _serverStdin?.Close();
        _mcpServerProcess?.Kill();
        _mcpServerProcess?.Dispose();
        _stderrReaderTask?.Wait(TimeSpan.FromSeconds(2));
        GC.SuppressFinalize(this);
    }
}

public class ToolDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public JsonNode? InputSchema { get; set; }
}