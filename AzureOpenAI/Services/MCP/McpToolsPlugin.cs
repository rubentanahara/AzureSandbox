using System.ComponentModel;

using Microsoft.SemanticKernel;

namespace AzureOpenAI.Services.MCP;

/// <summary>
/// Semantic Kernel plugin that bridges to MCP Server tools
/// </summary>
public class McpToolsPlugin(McpClient mcpClient)
{
    private readonly McpClient _mcpClient = mcpClient;

    [KernelFunction, Description("Get a specific support ticket by its ID number")]
    public async Task<string> GetTicketById(
        [Description("The ticket ID number")] int ticketId)
    {
        return await _mcpClient.CallMcpToolPublic("get_ticket_by_id", new { ticketId }) ?? "No result";
    }

    [KernelFunction, Description("Filter support tickets by their status")]
    public async Task<string> FilterByStatus(
        [Description("Status to filter by: Open, InProgress, Waiting, Resolved, or Closed")] string status)
    {
        return await _mcpClient.CallMcpToolPublic("filter_by_status", new { status }) ?? "No result";
    }

    [KernelFunction, Description("Filter support tickets by their priority level")]
    public async Task<string> FilterByPriority(
        [Description("Priority level: Low, Medium, High, or Critical")] string priority)
    {
        return await _mcpClient.CallMcpToolPublic("filter_by_priority", new { priority }) ?? "No result";
    }

    [KernelFunction, Description("Filter support tickets by their category")]
    public async Task<string> FilterByCategory(
        [Description("Category: Billing, Technical, or Account")] string category)
    {
        return await _mcpClient.CallMcpToolPublic("filter_by_category", new { category }) ?? "No result";
    }

    [KernelFunction, Description("Get support tickets created after a specific date")]
    public async Task<string> GetTicketsAfterDate(
        [Description("Date in yyyy-MM-dd format, for example: 2026-01-10")] string date)
    {
        return await _mcpClient.CallMcpToolPublic("get_tickets_after_date", new { date }) ?? "No result";
    }

    [KernelFunction, Description("Search for support tickets by an exact tag")]
    public async Task<string> SearchByTag(
        [Description("Tag to search for, like: authentication, refund, api, etc.")] string tag)
    {
        return await _mcpClient.CallMcpToolPublic("search_by_tag", new { tag }) ?? "No result";
    }

    [KernelFunction, Description("Get all support tickets in the system")]
    public async Task<string> GetAllTickets()
    {
        return await _mcpClient.CallMcpToolPublic("get_all_tickets", new { }) ?? "No result";
    }

    [KernelFunction, Description("Get a count of tickets grouped by their status")]
    public async Task<string> GetTicketCountByStatus()
    {
        return await _mcpClient.CallMcpToolPublic("get_ticket_count_by_status", new { }) ?? "No result";
    }

    [KernelFunction, Description("Create a new support ticket in the system")]
    public async Task<string> CreateTicket(
        [Description("Title/subject of the ticket")] string title,
        [Description("Detailed description of the issue or request")] string description,
        [Description("Category: Billing, Technical, or Account")] string category,
        [Description("Priority level: Low, Medium, High, or Critical")] string priority,
        [Description("Customer's email address")] string customerEmail,
        [Description("Customer ID")] string customerId,
        [Description("Comma-separated tags (optional)")] string tags = "")
    {
        return await _mcpClient.CallMcpToolPublic("create_ticket", new
        {
            title,
            description,
            category,
            priority,
            customerEmail,
            customerId,
            tags
        }) ?? "No result";
    }

    [KernelFunction, Description("Update an existing support ticket's status, priority, assignment, or resolution")]
    public async Task<string> UpdateTicket(
        [Description("The ID of the ticket to update")] int ticketId,
        [Description("New status: Open, InProgress, Waiting, Resolved, or Closed (leave empty to keep current)")] string status = "",
        [Description("New priority: Low, Medium, High, or Critical (leave empty to keep current)")] string priority = "",
        [Description("Person or team to assign the ticket to (leave empty to keep current)")] string assignedTo = "",
        [Description("Resolution notes or outcome description (leave empty to keep current)")] string resolution = "")
    {
        var args = new Dictionary<string, object> { { "ticketId", ticketId } };

        if (!string.IsNullOrWhiteSpace(status)) args["status"] = status;
        if (!string.IsNullOrWhiteSpace(priority)) args["priority"] = priority;
        if (!string.IsNullOrWhiteSpace(assignedTo)) args["assignedTo"] = assignedTo;
        if (!string.IsNullOrWhiteSpace(resolution)) args["resolution"] = resolution;

        return await _mcpClient.CallMcpToolPublic("update_ticket", args) ?? "No result";
    }
}