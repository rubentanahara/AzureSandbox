using System.ComponentModel;
using System.Text.Json;
using AzureOpenAI.Models;
using ModelContextProtocol.Server;

namespace AzureOpenAI.McpServer;

/// <summary>
/// Support Ticket MCP Tools
/// Provides structured query capabilities for the ticket system
/// </summary>
[McpServerToolType]
public static class SupportTicketTools
{
    private static readonly List<SupportTicket> _tickets = SampleTickets.GetSampleData();
    private static int _nextTicketId = 2000;

    [McpServerTool]
    [Description("Get a specific ticket by ID")]
    public static string GetTicketById(
        [Description("The ticket ID")] int ticketId)
    {
        var ticket = _tickets.FirstOrDefault(t => t.Id == ticketId);
        return ticket != null
            ? JsonSerializer.Serialize(ticket)
            : JsonSerializer.Serialize(new { error = "Ticket not found" });
    }

    [McpServerTool]
    [Description("Filter tickets by status (Open, InProgress, Waiting, Resolved, Closed)")]
    public static string FilterByStatus(
        [Description("Status: Open, InProgress, Waiting, Resolved, Closed")] string status)
    {
        if (Enum.TryParse<TicketStatus>(status, true, out var ticketStatus))
        {
            var results = _tickets.Where(t => t.Status == ticketStatus).ToList();
            return JsonSerializer.Serialize(results);
        }
        return "[]";
    }

    [McpServerTool]
    [Description("Filter tickets by priority (Low, Medium, High, Critical)")]
    public static string FilterByPriority(
        [Description("Priority: Low, Medium, High, Critical")] string priority)
    {
        if (Enum.TryParse<TicketPriority>(priority, true, out var ticketPriority))
        {
            var results = _tickets.Where(t => t.Priority == ticketPriority).ToList();
            return JsonSerializer.Serialize(results);
        }
        return "[]";
    }

    [McpServerTool]
    [Description("Filter tickets by category (Billing, Technical, Account)")]
    public static string FilterByCategory(
        [Description("Category: Billing, Technical, Account")] string category)
    {
        var results = _tickets.Where(t =>
            t.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        return JsonSerializer.Serialize(results);
    }

    [McpServerTool]
    [Description("Get tickets created after a specific date")]
    public static string GetTicketsAfterDate(
        [Description("Date in yyyy-MM-dd format")] string date)
    {
        if (DateTime.TryParse(date, out var parsedDate))
        {
            var results = _tickets.Where(t => t.CreatedAt >= parsedDate).ToList();
            return JsonSerializer.Serialize(results);
        }
        return "[]";
    }

    [McpServerTool]
    [Description("Search tickets by exact tag")]
    public static string SearchByTag(
        [Description("Tag to search for")] string tag)
    {
        var results = _tickets.Where(t =>
            t.Tags.Any(tg => tg.Equals(tag, StringComparison.OrdinalIgnoreCase))).ToList();
        return JsonSerializer.Serialize(results);
    }

    [McpServerTool]
    [Description("Get all tickets")]
    public static string GetAllTickets()
    {
        return JsonSerializer.Serialize(_tickets);
    }

    [McpServerTool]
    [Description("Get ticket count by status")]
    public static string GetTicketCountByStatus()
    {
        var counts = _tickets.GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToList();
        return JsonSerializer.Serialize(counts);
    }

    [McpServerTool]
    [Description("Create a new support ticket")]
    public static string CreateTicket(
        [Description("Ticket title")] string title,
        [Description("Ticket description")] string description,
        [Description("Category: Billing, Technical, or Account")] string category,
        [Description("Priority: Low, Medium, High, or Critical")] string priority,
        [Description("Customer email address")] string customerEmail,
        [Description("Customer ID")] string customerId,
        [Description("Comma-separated tags (optional)")] string tags = "")
    {
        try
        {
            if (!Enum.TryParse<TicketPriority>(priority, true, out var ticketPriority))
            {
                return JsonSerializer.Serialize(new { error = $"Invalid priority: {priority}. Use Low, Medium, High, or Critical" });
            }

            var newTicket = new SupportTicket
            {
                Id = _nextTicketId++,
                Title = title,
                Description = description,
                Category = category,
                Priority = ticketPriority,
                Status = TicketStatus.Open,
                CustomerEmail = customerEmail,
                CustomerId = customerId,
                CreatedAt = DateTime.Now,
                Tags = string.IsNullOrWhiteSpace(tags)
                    ? new List<string>()
                    : tags.Split(',').Select(t => t.Trim()).ToList()
            };

            _tickets.Add(newTicket);

            return JsonSerializer.Serialize(new
            {
                success = true,
                message = $"Ticket created successfully with ID {newTicket.Id}",
                ticket = newTicket
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = $"Failed to create ticket: {ex.Message}" });
        }
    }

    [McpServerTool]
    [Description("Update an existing support ticket")]
    public static string UpdateTicket(
        [Description("Ticket ID to update")] int ticketId,
        [Description("New status: Open, InProgress, Waiting, Resolved, or Closed (optional)")] string? status = null,
        [Description("New priority: Low, Medium, High, or Critical (optional)")] string? priority = null,
        [Description("New assigned person/team (optional)")] string? assignedTo = null,
        [Description("Resolution notes (optional)")] string? resolution = null)
    {
        try
        {
            var ticket = _tickets.FirstOrDefault(t => t.Id == ticketId);
            if (ticket == null)
            {
                return JsonSerializer.Serialize(new { error = $"Ticket {ticketId} not found" });
            }

            var updated = false;

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<TicketStatus>(status, true, out var ticketStatus))
                {
                    ticket.Status = ticketStatus;
                    if (ticketStatus == TicketStatus.Resolved || ticketStatus == TicketStatus.Closed)
                    {
                        ticket.ResolvedAt = DateTime.Now;
                    }
                    updated = true;
                }
                else
                {
                    return JsonSerializer.Serialize(new { error = $"Invalid status: {status}" });
                }
            }

            if (!string.IsNullOrWhiteSpace(priority))
            {
                if (Enum.TryParse<TicketPriority>(priority, true, out var ticketPriority))
                {
                    ticket.Priority = ticketPriority;
                    updated = true;
                }
                else
                {
                    return JsonSerializer.Serialize(new { error = $"Invalid priority: {priority}" });
                }
            }

            if (!string.IsNullOrWhiteSpace(assignedTo))
            {
                ticket.AssignedTo = assignedTo;
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(resolution))
            {
                ticket.Resolution = resolution;
                updated = true;
            }

            if (!updated)
            {
                return JsonSerializer.Serialize(new { error = "No fields to update were provided" });
            }

            return JsonSerializer.Serialize(new
            {
                success = true,
                message = $"Ticket {ticketId} updated successfully",
                ticket
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = $"Failed to update ticket: {ex.Message}" });
        }
    }
}