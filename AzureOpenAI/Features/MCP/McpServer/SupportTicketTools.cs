using System.ComponentModel;
using System.Text.Json;

using AzureOpenAI.Models;
using AzureOpenAI.Data;

using ModelContextProtocol.Server;

namespace AzureOpenAI.Features.MCP.McpServer;

/// <summary>
/// Support Ticket MCP Tools
/// Provides structured query capabilities for the ticket system
/// </summary>
[McpServerToolType]
public static class SupportTicketTools
{
    private static TicketRepository? _repository;

    public static void Initialize(TicketRepository repository)
    {
        _repository = repository;
    }

    [McpServerTool]
    [Description("Get a specific ticket by ID")]
    public static string GetTicketById(
        [Description("The ticket ID")] int ticketId)
    {
        if (_repository == null)
            return JsonSerializer.Serialize(new { error = "Repository not initialized" });

        var ticket = _repository.GetTicketByIdAsync(ticketId).GetAwaiter().GetResult();
        return ticket != null
            ? JsonSerializer.Serialize(ticket)
            : JsonSerializer.Serialize(new { error = "Ticket not found" });
    }

    [McpServerTool]
    [Description("Filter tickets by status (Open, InProgress, Waiting, Resolved, Closed)")]
    public static string FilterByStatus(
        [Description("Status: Open, InProgress, Waiting, Resolved, Closed")] string status)
    {
        if (_repository == null)
            return JsonSerializer.Serialize(new { error = "Repository not initialized" });

        if (Enum.TryParse<TicketStatus>(status, true, out var ticketStatus))
        {
            var results = _repository.GetTicketsByStatusAsync(ticketStatus).GetAwaiter().GetResult();
            return JsonSerializer.Serialize(results);
        }
        return "[]";
    }

    [McpServerTool]
    [Description("Filter tickets by priority (Low, Medium, High, Critical)")]
    public static string FilterByPriority(
        [Description("Priority: Low, Medium, High, Critical")] string priority)
    {
        if (_repository == null)
            return JsonSerializer.Serialize(new { error = "Repository not initialized" });

        if (Enum.TryParse<TicketPriority>(priority, true, out var ticketPriority))
        {
            var results = _repository.GetTicketsByPriorityAsync(ticketPriority).GetAwaiter().GetResult();
            return JsonSerializer.Serialize(results);
        }
        return "[]";
    }

    [McpServerTool]
    [Description("Filter tickets by category (Billing, Technical, Account)")]
    public static string FilterByCategory(
        [Description("Category: Billing, Technical, Account")] string category)
    {
        if (_repository == null)
            return JsonSerializer.Serialize(new { error = "Repository not initialized" });

        var results = _repository.GetTicketsByCategoryAsync(category).GetAwaiter().GetResult();
        return JsonSerializer.Serialize(results);
    }

    [McpServerTool]
    [Description("Get tickets created after a specific date")]
    public static string GetTicketsAfterDate(
        [Description("Date in yyyy-MM-dd format")] string date)
    {
        if (_repository == null)
            return JsonSerializer.Serialize(new { error = "Repository not initialized" });

        if (DateTime.TryParse(date, out var parsedDate))
        {
            var results = _repository.GetTicketsAfterDateAsync(parsedDate).GetAwaiter().GetResult();
            return JsonSerializer.Serialize(results);
        }
        return "[]";
    }

    [McpServerTool]
    [Description("Search tickets by exact tag")]
    public static string SearchByTag(
        [Description("Tag to search for")] string tag)
    {
        if (_repository == null)
            return JsonSerializer.Serialize(new { error = "Repository not initialized" });

        var results = _repository.SearchByTagAsync(tag).GetAwaiter().GetResult();
        return JsonSerializer.Serialize(results);
    }

    [McpServerTool]
    [Description("Get all tickets")]
    public static string GetAllTickets()
    {
        if (_repository == null)
            return JsonSerializer.Serialize(new { error = "Repository not initialized" });

        var results = _repository.GetAllTicketsAsync().GetAwaiter().GetResult();
        return JsonSerializer.Serialize(results);
    }

    [McpServerTool]
    [Description("Get ticket count by status")]
    public static string GetTicketCountByStatus()
    {
        if (_repository == null)
            return JsonSerializer.Serialize(new { error = "Repository not initialized" });

        var counts = _repository.GetTicketCountByStatusAsync().GetAwaiter().GetResult();
        var result = counts.Select(kvp => new { Status = kvp.Key.ToString(), Count = kvp.Value }).ToList();
        return JsonSerializer.Serialize(result);
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
            if (_repository == null)
                return JsonSerializer.Serialize(new { error = "Repository not initialized" });

            if (!Enum.TryParse<TicketPriority>(priority, true, out var ticketPriority))
            {
                return JsonSerializer.Serialize(new { error = $"Invalid priority: {priority}. Use Low, Medium, High, or Critical" });
            }

            var newTicket = new SupportTicket
            {
                Title = title,
                Description = description,
                Category = category,
                Priority = ticketPriority,
                Status = TicketStatus.Open,
                CustomerEmail = customerEmail,
                CustomerId = customerId,
                CreatedAt = DateTime.UtcNow,
                Tags = string.IsNullOrWhiteSpace(tags)
                    ? new List<string>()
                    : tags.Split(',').Select(t => t.Trim()).ToList(),
                Resolution = string.Empty,
                AssignedTo = string.Empty
            };

            var ticketId = _repository.CreateTicketAsync(newTicket).GetAwaiter().GetResult();
            newTicket.Id = ticketId;

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
            if (_repository == null)
                return JsonSerializer.Serialize(new { error = "Repository not initialized" });

            var ticket = _repository.GetTicketByIdAsync(ticketId).GetAwaiter().GetResult();
            if (ticket == null)
            {
                return JsonSerializer.Serialize(new { error = $"Ticket {ticketId} not found" });
            }

            var updated = false;

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<TicketStatus>(status, true, out var ticketStatus))
                {
                    _repository.UpdateTicketStatusAsync(ticketId, ticketStatus, resolution).GetAwaiter().GetResult();
                    updated = true;
                }
                else
                {
                    return JsonSerializer.Serialize(new { error = $"Invalid status: {status}" });
                }
            }

            if (!string.IsNullOrWhiteSpace(assignedTo))
            {
                _repository.AssignTicketAsync(ticketId, assignedTo).GetAwaiter().GetResult();
                updated = true;
            }

            if (!updated)
            {
                return JsonSerializer.Serialize(new { error = "No fields to update were provided" });
            }

            // Get updated ticket
            var updatedTicket = _repository.GetTicketByIdAsync(ticketId).GetAwaiter().GetResult();

            return JsonSerializer.Serialize(new
            {
                success = true,
                message = $"Ticket {ticketId} updated successfully",
                ticket = updatedTicket
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = $"Failed to update ticket: {ex.Message}" });
        }
    }
}