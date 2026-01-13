namespace AzureOpenAI.Models;

public sealed class SupportTicket
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // "Billing", "Technical", "Account"
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public string Resolution { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
}