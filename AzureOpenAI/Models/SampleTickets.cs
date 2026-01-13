namespace AzureOpenAI.Models;

public static class SampleTickets
{
    public static List<SupportTicket> GetSampleData() =>
    [
        new SupportTicket
        {
            Id = 1001,
            Title = "Cannot log into account",
            Description = "I've been trying to log in for the past hour but keep getting 'invalid credentials' error. I'm sure my password is correct. I tried resetting it but didn't receive the email.",
            Category = "Technical",
            Status = TicketStatus.Open,
            Priority = TicketPriority.High,
            CreatedAt = DateTime.Now.AddHours(-2),
            CustomerId = "CUST-001",
            CustomerEmail = "john@example.com",
            Tags = ["login", "authentication", "password-reset"]
        },
        new SupportTicket
        {
            Id = 1002,
            Title = "Charged twice for subscription",
            Description = "I was billed $49.99 twice on January 10th. My bank statement shows two separate charges. I only have one active subscription.",
            Category = "Billing",
            Status = TicketStatus.InProgress,
            Priority = TicketPriority.Critical,
            CreatedAt = DateTime.Now.AddDays(-1),
            CustomerId = "CUST-002",
            CustomerEmail = "sarah@example.com",
            Tags = ["double-charge", "refund", "payment"],
            AssignedTo = "billing-team"
        },
        new SupportTicket
        {
            Id = 1003,
            Title = "2FA not working on mobile app",
            Description = "The two-factor authentication code doesn't work on the mobile app. I enter the 6-digit code from Google Authenticator but it says invalid. Works fine on desktop.",
            Category = "Technical",
            Status = TicketStatus.Open,
            Priority = TicketPriority.High,
            CreatedAt = DateTime.Now.AddHours(-5),
            CustomerId = "CUST-003",
            CustomerEmail = "mike@example.com",
            Tags = ["2fa", "mobile", "authentication"]
        },
        new SupportTicket
        {
            Id = 1004,
            Title = "Want to upgrade to premium plan",
            Description = "I'm currently on the basic plan and want to upgrade to premium. How do I do this without losing my current data?",
            Category = "Account",
            Status = TicketStatus.Resolved,
            Priority = TicketPriority.Low,
            CreatedAt = DateTime.Now.AddDays(-3),
            ResolvedAt = DateTime.Now.AddDays(-2),
            CustomerId = "CUST-004",
            CustomerEmail = "lisa@example.com",
            Tags = ["upgrade", "plan-change"],
            Resolution = "Provided upgrade instructions. Customer successfully upgraded to premium plan.",
            AssignedTo = "sales-team"
        },
        new SupportTicket
        {
            Id = 1005,
            Title = "API returning 401 errors",
            Description = "Our integration suddenly started failing. All API calls return 401 Unauthorized. Our API key hasn't changed. This started happening around 3pm EST.",
            Category = "Technical",
            Status = TicketStatus.InProgress,
            Priority = TicketPriority.Critical,
            CreatedAt = DateTime.Now.AddHours(-4),
            CustomerId = "CUST-005",
            CustomerEmail = "dev@company.com",
            Tags = ["api", "authentication", "integration"],
            AssignedTo = "engineering-team"
        },
        new SupportTicket
        {
            Id = 1006,
            Title = "Refund request for unused service",
            Description = "I signed up last week but haven't used the service at all. Can I get a refund for this month?",
            Category = "Billing",
            Status = TicketStatus.Waiting,
            Priority = TicketPriority.Medium,
            CreatedAt = DateTime.Now.AddDays(-2),
            CustomerId = "CUST-006",
            CustomerEmail = "alex@example.com",
            Tags = ["refund", "cancellation"],
            AssignedTo = "billing-team"
        }
    ];
}