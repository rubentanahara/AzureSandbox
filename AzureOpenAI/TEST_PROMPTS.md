# IT Support System - Test Prompts & Flow Verification

## System Overview

This document verifies the correct flow of all three modes and provides comprehensive test prompts for each.

______________________________________________________________________

## Mode 1: Knowledge Base RAG (Option 1)

### Flow Verification ✅

**Entry Point:** `ConsoleUI.cs:38-43`

- User selects "RAG", "KB", or "1"
- Creates `KnowledgeRagService` instance
- Calls `DemoRunner.RunKnowledgeRAGDemo()`

**Initialization:** `DemoRunner.cs:10-29`

1. Loads markdown files from `Data/KnowledgeBase/` (8 files)
1. Parses YAML frontmatter for metadata
1. Chunks documents using TextChunker (400 tokens, 80 overlap)
1. Generates embeddings for each chunk
1. Stores in vector store

**Query Processing:** `KnowledgeRagService.cs:119-176`

1. User enters natural language query
1. Query is embedded
1. Vector similarity search finds top 5 relevant chunks
1. Context is built from relevant documents
1. LLM generates answer based on context
1. Shows matched documents and response

### 5 Test Prompts for Knowledge Base Mode

```plaintext
1. "How do I reset a user's password? Walk me through the complete procedure."
   Expected: Password reset policy with self-service and IT support options

2. "My VPN won't connect on Windows 11. What should I check?"
   Expected: VPN troubleshooting steps, known issues with v5.1.x

3. "What software can I install without IT approval?"
   Expected: List of approved software from software installation policy

4. "My laptop is running extremely slow and the fan is loud. Help!"
   Expected: Performance troubleshooting steps including disk space, startup programs

5. "I need to set up MFA but I lost my phone. What are my options?"
   Expected: MFA setup methods and troubleshooting for lost/changed phone
```

______________________________________________________________________

## Mode 2: MCP Ticket Management (Option 2)

### Flow Verification ✅

**Entry Point:** `ConsoleUI.cs:45-53`

- User selects "MCP" or "2"
- Creates `McpClient` instance
- Initializes MCP server subprocess
- Calls `DemoRunner.RunMCPDemo()`

**Initialization:** `McpClient.cs:31-112`

1. Starts MCP server as subprocess (`dotnet run --project McpServer`)
1. Initializes SQLite database with sample tickets
1. Establishes JSON-RPC communication (stdin/stdout)
1. Retrieves available MCP tools from server
1. Registers tools with Semantic Kernel

**Available MCP Tools:** `SupportTicketTools.cs:23-247`

- `GetTicketById` - Retrieve specific ticket
- `FilterByStatus` - Filter by Open/InProgress/Waiting/Resolved/Closed
- `FilterByPriority` - Filter by Low/Medium/High/Critical
- `FilterByCategory` - Filter by Billing/Technical/Account
- `GetTicketsAfterDate` - Get tickets created after date
- `SearchByTag` - Search by exact tag
- `GetAllTickets` - List all tickets
- `GetTicketCountByStatus` - Count tickets by status
- `CreateTicket` - Create new support ticket
- `UpdateTicket` - Update ticket status/priority/assignment

**Query Processing:** `McpClient.cs:114-145`

1. User enters natural language query
1. LLM analyzes query and selects appropriate MCP tool(s)
1. Tool is invoked via JSON-RPC to MCP server
1. Server queries SQLite database
1. Results returned to LLM
1. LLM formats and presents results to user

### 5 Test Prompts for MCP Mode

```plaintext
1. "Show me all high priority tickets that are currently open"
   Expected: Filtered list of tickets with Priority=High and Status=Open

2. "What's the status of ticket number 1005?"
   Expected: Detailed ticket information for ticket ID 1005

3. "Create a new ticket: Sarah from accounting can't access the shared drive. Her email is sarah.jones@company.com, customer ID ACC-1042. This is high priority."
   Expected: New ticket created with all details, confirmation with ticket ID

4. "How many tickets do we have in each status category?"
   Expected: Count breakdown (e.g., Open: 5, InProgress: 3, Resolved: 12)

5. "Update ticket 1001 to InProgress status and assign it to network-team"
   Expected: Ticket 1001 updated with new status and assignment, confirmation message

6.Hey, I'm trying to log in but I keep getting a 'Database Connection Error' after I enter my password. This is urgent because I have a deadline today! My ID is CUST-882 and my email is dev_pro@company.com.
    Expected: Ticket created for 'Database Connection Error' with Urgent priority, confirmation with ticket ID
```

______________________________________________________________________

## Mode 3: Hybrid (Knowledge Base + Tickets) (Option 3)

### Flow Verification ✅

**Entry Point:** `ConsoleUI.cs:55-64`

- User selects "HYBRID" or "3"
- Creates both `KnowledgeRagService` and `McpClient`
- Creates `HybridQueryService` combining both
- Calls `DemoRunner.RunHybridDemo()`

**Initialization:** `HybridQueryService.cs:30-50`

1. Indexes knowledge base from markdown files
1. Initializes MCP client and server subprocess
1. Registers `KnowledgeRagPlugin` with Semantic Kernel
1. Registers all MCP ticket tools with Semantic Kernel

**Available Functions:**

- **KnowledgeBase.SemanticKnowledgeSearch** - Search IT documentation
- **McpTools.**\* - All 10 MCP ticket management tools

**Query Processing:** `HybridQueryService.cs:52-76`

1. User enters natural language query
1. LLM analyzes query intent
1. **Decision Logic:**
   - Knowledge question → Uses KnowledgeBase plugin
   - Ticket operation → Uses MCP tools
   - Both → Uses both plugins together
1. Tools auto-invoked by Semantic Kernel
1. LLM synthesizes final response

**System Prompt:** `HybridQueryService.cs:78-125`

- Provides clear guidelines on when to use each system
- Examples for each type of query
- Decision-making criteria

### 5 Test Prompts for Hybrid Mode

```plaintext
1. "How do I troubleshoot VPN issues? Also show me any open tickets related to VPN."
   Expected: VPN troubleshooting steps from KB + filtered ticket list from MCP

2. "Create a ticket: John can't reset his password and he doesn't have access to his email. His ID is EMP-2093, email john.smith@company.com. Priority: Medium."
   Expected: Password reset procedure from KB + new ticket created via MCP

3. "What's our MFA policy and how many MFA-related tickets are currently open?"
   Expected: MFA setup guide from KB + ticket count/list filtered by MFA tag

4. "My laptop is slow. Tell me how to fix it and create a ticket to track this issue. Customer ID: EMP-5521, email: user@company.com"
   Expected: Performance optimization steps from KB + new ticket created

5. "Show me the printer setup procedure for Floor 2 and check if there are any existing printer tickets for that floor"
   Expected: Printer setup guide from KB + filtered tickets by tag/search
```

______________________________________________________________________

## Data Verification

### Knowledge Base Files (8 total)

✅ `password-reset-policy.md` - Security category
✅ `vpn-troubleshooting.md` - Network category
✅ `email-troubleshooting.md` - Email category
✅ `software-installation-policy.md` - Software category
✅ `printer-setup-troubleshooting.md` - Hardware category
✅ `incident-management-escalation.md` - Incident Management category
✅ `mfa-setup-guide.md` - Security category
✅ `laptop-performance-optimization.md` - Hardware category

### Sample Ticket Data (seeded automatically)

- Multiple tickets with varying statuses
- Different priority levels
- Categories: Billing, Technical, Account
- Tags for filtering and searching

______________________________________________________________________

## Flow Diagrams

### Knowledge Base RAG Flow

```
User Query → Embedding → Vector Search → Top 5 Chunks →
Context Building → LLM (with context) → Answer
```

### MCP Ticket Management Flow

```
User Query → LLM Analysis → Tool Selection → JSON-RPC Call →
SQLite Query → Results → LLM Formatting → Response
```

### Hybrid Flow

```
User Query → LLM Analysis →
├─ Knowledge Question? → KnowledgeBase.SemanticKnowledgeSearch
├─ Ticket Operation? → McpTools.*
└─ Both? → Use Both Plugins → Synthesize Response
```

______________________________________________________________________

## Testing Tips

1. **Knowledge Base Mode**: Test with "how to" questions, troubleshooting scenarios
1. **MCP Mode**: Test CRUD operations, filtering, searching tickets
1. **Hybrid Mode**: Test complex queries combining both knowledge and ticket management
1. **Error Handling**: Try invalid ticket IDs, nonsensical queries to test robustness
1. **Multi-step**: In Hybrid mode, test queries requiring multiple tool calls

______________________________________________________________________

## Success Criteria

- ✅ All 8 markdown files load correctly
- ✅ Chunking completes without freezing
- ✅ Embeddings generate successfully
- ✅ Vector search returns relevant results
- ✅ MCP server starts without JSON errors
- ✅ All 10 MCP tools are registered
- ✅ Hybrid mode correctly routes to appropriate system(s)
- ✅ LLM provides coherent, accurate responses

______________________________________________________________________

## Build & Run

```bash
# Build main project
cd /Users/rubentanahara/Desktop/dev/dotnet/AzureSandbox/AzureOpenAI
dotnet build

# Build MCP server
cd Services/MCP/McpServer
dotnet build

# Run application
cd /Users/rubentanahara/Desktop/dev/dotnet/AzureSandbox/AzureOpenAI
dotnet run
```
