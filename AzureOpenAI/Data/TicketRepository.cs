using AzureOpenAI.Models;
using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace AzureOpenAI.Data;

public class TicketRepository : IDisposable
{
    private readonly SqliteConnection _connection;
    private bool _isInitialized = false;

    public TicketRepository(string databasePath = "tickets.db")
    {
        _connection = new SqliteConnection($"Data Source={databasePath}");
        _connection.Open();
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        var createTableCommand = _connection.CreateCommand();
        createTableCommand.CommandText = """
        CREATE TABLE IF NOT EXISTS Tickets (
            Id INTEGER PRIMARY KEY,
            Title TEXT NOT NULL,
            Description TEXT NOT NULL,
            Category TEXT NOT NULL,
            Status INTEGER NOT NULL,
            Priority INTEGER NOT NULL,
            CreatedAt TEXT NOT NULL,
            ResolvedAt TEXT,
            CustomerId TEXT NOT NULL,
            CustomerEmail TEXT NOT NULL,
            Tags TEXT NOT NULL,
            Resolution TEXT NOT NULL,
            AssignedTo TEXT NOT NULL
        )
        """;
        await createTableCommand.ExecuteNonQueryAsync();

        _isInitialized = true;
    }

    public async Task<int> CreateTicketAsync(SupportTicket ticket)
    {
        var command = _connection.CreateCommand();
        command.CommandText = """
        INSERT INTO Tickets (Title, Description, Category, Status, Priority, CreatedAt, ResolvedAt,
                             CustomerId, CustomerEmail, Tags, Resolution, AssignedTo)
        VALUES (@title, @description, @category, @status, @priority, @createdAt, @resolvedAt,
                @customerId, @customerEmail, @tags, @resolution, @assignedTo);
        SELECT last_insert_rowid();
        """;

        command.Parameters.AddWithValue("@title", ticket.Title);
        command.Parameters.AddWithValue("@description", ticket.Description);
        command.Parameters.AddWithValue("@category", ticket.Category);
        command.Parameters.AddWithValue("@status", (int)ticket.Status);
        command.Parameters.AddWithValue("@priority", (int)ticket.Priority);
        command.Parameters.AddWithValue("@createdAt", ticket.CreatedAt.ToString("o"));
        command.Parameters.AddWithValue("@resolvedAt", ticket.ResolvedAt?.ToString("o") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@customerId", ticket.CustomerId);
        command.Parameters.AddWithValue("@customerEmail", ticket.CustomerEmail);
        command.Parameters.AddWithValue("@tags", JsonSerializer.Serialize(ticket.Tags));
        command.Parameters.AddWithValue("@resolution", ticket.Resolution);
        command.Parameters.AddWithValue("@assignedTo", ticket.AssignedTo);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<SupportTicket?> GetTicketByIdAsync(int id)
    {
        var command = _connection.CreateCommand();
        command.CommandText = "SELECT * FROM Tickets WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return ReadTicket(reader);
        }
        return null;
    }

    public async Task<List<SupportTicket>> GetAllTicketsAsync()
    {
        var command = _connection.CreateCommand();
        command.CommandText = "SELECT * FROM Tickets ORDER BY CreatedAt DESC";

        var tickets = new List<SupportTicket>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tickets.Add(ReadTicket(reader));
        }
        return tickets;
    }

    public async Task<List<SupportTicket>> GetTicketsByStatusAsync(TicketStatus status)
    {
        var command = _connection.CreateCommand();
        command.CommandText = "SELECT * FROM Tickets WHERE Status = @status ORDER BY CreatedAt DESC";
        command.Parameters.AddWithValue("@status", (int)status);

        var tickets = new List<SupportTicket>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tickets.Add(ReadTicket(reader));
        }
        return tickets;
    }

    public async Task<List<SupportTicket>> GetTicketsByPriorityAsync(TicketPriority priority)
    {
        var command = _connection.CreateCommand();
        command.CommandText = "SELECT * FROM Tickets WHERE Priority = @priority ORDER BY CreatedAt DESC";
        command.Parameters.AddWithValue("@priority", (int)priority);

        var tickets = new List<SupportTicket>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tickets.Add(ReadTicket(reader));
        }
        return tickets;
    }

    public async Task<List<SupportTicket>> GetTicketsByCategoryAsync(string category)
    {
        var command = _connection.CreateCommand();
        command.CommandText = "SELECT * FROM Tickets WHERE Category = @category ORDER BY CreatedAt DESC";
        command.Parameters.AddWithValue("@category", category);

        var tickets = new List<SupportTicket>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tickets.Add(ReadTicket(reader));
        }
        return tickets;
    }

    public async Task<List<SupportTicket>> SearchByTagAsync(string tag)
    {
        var command = _connection.CreateCommand();
        command.CommandText = "SELECT * FROM Tickets WHERE Tags LIKE @tag ORDER BY CreatedAt DESC";
        command.Parameters.AddWithValue("@tag", $"%{tag}%");

        var tickets = new List<SupportTicket>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tickets.Add(ReadTicket(reader));
        }
        return tickets;
    }

    public async Task<List<SupportTicket>> GetTicketsAfterDateAsync(DateTime date)
    {
        var command = _connection.CreateCommand();
        command.CommandText = "SELECT * FROM Tickets WHERE CreatedAt >= @date ORDER BY CreatedAt DESC";
        command.Parameters.AddWithValue("@date", date.ToString("o"));

        var tickets = new List<SupportTicket>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tickets.Add(ReadTicket(reader));
        }
        return tickets;
    }

    public async Task<bool> UpdateTicketStatusAsync(int id, TicketStatus status, string? resolution = null)
    {
        var command = _connection.CreateCommand();

        if (status == TicketStatus.Resolved || status == TicketStatus.Closed)
        {
            command.CommandText = "UPDATE Tickets SET Status = @status, ResolvedAt = @resolvedAt, Resolution = @resolution WHERE Id = @id";
            command.Parameters.AddWithValue("@resolvedAt", DateTime.UtcNow.ToString("o"));
            command.Parameters.AddWithValue("@resolution", resolution ?? string.Empty);
        }
        else
        {
            command.CommandText = "UPDATE Tickets SET Status = @status WHERE Id = @id";
        }

        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@status", (int)status);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> AssignTicketAsync(int id, string assignedTo)
    {
        var command = _connection.CreateCommand();
        command.CommandText = "UPDATE Tickets SET AssignedTo = @assignedTo WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@assignedTo", assignedTo);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<Dictionary<TicketStatus, int>> GetTicketCountByStatusAsync()
    {
        var command = _connection.CreateCommand();
        command.CommandText = "SELECT Status, COUNT(*) as Count FROM Tickets GROUP BY Status";

        var counts = new Dictionary<TicketStatus, int>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var status = (TicketStatus)reader.GetInt32(0);
            var count = reader.GetInt32(1);
            counts[status] = count;
        }
        return counts;
    }

    public async Task SeedSampleDataAsync()
    {
        // Check if data already exists
        var command = _connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Tickets";
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());

        if (count > 0)
        {
            Console.Error.WriteLine($"📊 Database already contains {count} tickets");
            return;
        }

        Console.Error.WriteLine("📥 Seeding sample ticket data...");
        var sampleTickets = SampleTickets.GetSampleData();

        foreach (var ticket in sampleTickets)
        {
            await CreateTicketAsync(ticket);
        }

        Console.Error.WriteLine($"✅ Seeded {sampleTickets.Count} sample tickets");
    }

    private static SupportTicket ReadTicket(SqliteDataReader reader)
    {
        return new SupportTicket
        {
            Id = reader.GetInt32(0),
            Title = reader.GetString(1),
            Description = reader.GetString(2),
            Category = reader.GetString(3),
            Status = (TicketStatus)reader.GetInt32(4),
            Priority = (TicketPriority)reader.GetInt32(5),
            CreatedAt = DateTime.Parse(reader.GetString(6)),
            ResolvedAt = reader.IsDBNull(7) ? null : DateTime.Parse(reader.GetString(7)),
            CustomerId = reader.GetString(8),
            CustomerEmail = reader.GetString(9),
            Tags = JsonSerializer.Deserialize<List<string>>(reader.GetString(10)) ?? [],
            Resolution = reader.GetString(11),
            AssignedTo = reader.GetString(12)
        };
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}