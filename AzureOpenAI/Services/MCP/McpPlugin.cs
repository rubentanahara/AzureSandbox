using System.ComponentModel;
using System.Text.Json;

using AzureOpenAI.Models;

using Microsoft.SemanticKernel;

namespace AzureOpenAI.Services.MCP;

public class McpPlugin
{
    private readonly List<SupportTicket> _tickets;

    public McpPlugin()
    {
        _tickets = SampleTickets.GetSampleData();
    }

    [KernelFunction, Description("Get a specific ticket by ID")]
    public string GetTicketById([Description("The ticket ID")] int ticketId)
    {
        Console.WriteLine($"  🔧 Tool: GetTicketById({ticketId})");
        var ticket = _tickets.FirstOrDefault(t => t.Id == ticketId);
        return ticket != null ? JsonSerializer.Serialize(ticket) : "Ticket not found";
    }

    [KernelFunction, Description("Filter tickets by status (Open, InProgress, Waiting, Resolved, Closed)")]
    public string FilterByStatus([Description("Status: Open, InProgress, Waiting, Resolved, Closed")] string status)
    {
        Console.WriteLine($"  🔧 Tool: FilterByStatus({status})");

        if (Enum.TryParse<TicketStatus>(status, true, out var ticketStatus))
        {
            var results = _tickets.Where(t => t.Status == ticketStatus).ToList();
            return JsonSerializer.Serialize(results);
        }
        return "[]";
    }

    [KernelFunction, Description("Filter tickets by priority (Low, Medium, High, Critical)")]
    public string FilterByPriority([Description("Priority: Low, Medium, High, Critical")] string priority)
    {
        Console.WriteLine($"  🔧 Tool: FilterByPriority({priority})");

        if (Enum.TryParse<TicketPriority>(priority, true, out var ticketPriority))
        {
            var results = _tickets.Where(t => t.Priority == ticketPriority).ToList();
            return JsonSerializer.Serialize(results);
        }
        return "[]";
    }

    [KernelFunction, Description("Filter tickets by category (Billing, Technical, Account)")]
    public string FilterByCategory([Description("Category: Billing, Technical, Account")] string category)
    {
        Console.WriteLine($"  🔧 Tool: FilterByCategory({category})");
        var results = _tickets.Where(t =>
            t.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        return JsonSerializer.Serialize(results);
    }

    [KernelFunction, Description("Get tickets created after a specific date")]
    public string GetTicketsAfterDate([Description("Date in yyyy-MM-dd format")] string date)
    {
        Console.WriteLine($"  🔧 Tool: GetTicketsAfterDate({date})");

        if (DateTime.TryParse(date, out var parsedDate))
        {
            var results = _tickets.Where(t => t.CreatedAt >= parsedDate).ToList();
            return JsonSerializer.Serialize(results);
        }
        return "[]";
    }

    [KernelFunction, Description("Search tickets by exact tag")]
    public string SearchByTag([Description("Tag to search for")] string tag)
    {
        Console.WriteLine($"  🔧 Tool: SearchByTag({tag})");
        var results = _tickets.Where(t =>
            t.Tags.Any(tg => tg.Equals(tag, StringComparison.OrdinalIgnoreCase))).ToList();
        return JsonSerializer.Serialize(results);
    }
}