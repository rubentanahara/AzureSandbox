using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using AzureOpenAI.Data;

namespace AzureOpenAI.Features.MCP.McpServer;

public class Program
{
    public static async Task Main(string[] args)
    {
        var repository = new TicketRepository("tickets.db");
        await repository.InitializeAsync();
        await repository.SeedSampleDataAsync();
        SupportTicketTools.Initialize(repository);

        var builder = Host.CreateApplicationBuilder(args);

        builder.Logging.AddConsole(consoleLogOptions =>
        {
            consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
        });

        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly();

        await builder.Build().RunAsync();

        repository.Dispose();
    }
}