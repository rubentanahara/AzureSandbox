# Azure OpenAI - RAG, MCP, and Hybrid Demo

A .NET 10 console application showcasing **RAG (Retrieval-Augmented Generation)**, **MCP (Model Context Protocol)**, and a **Hybrid approach** combining both techniques for AI-powered support ticket querying.

## 📋 Overview

This project demonstrates three distinct AI architectures for querying support tickets:

### 1. **RAG (Retrieval-Augmented Generation)**
- **Two-Phase Pipeline:** Ingestion → Inference
- **Ingestion Phase:** Converts tickets to embeddings and stores in vector database
- **Inference Phase:** Semantic search via cosine similarity + LLM-generated response
- **Best For:** Natural language queries, conceptual searches ("authentication problems", "billing issues")

### 2. **MCP (Model Context Protocol)**
- **Client-Server Architecture:** JSON-RPC over stdin/stdout
- **Separate Process:** MCP server runs as subprocess, communicates via protocol
- **Structured Tools:** Exact filtering by status, priority, category, date, tags
- **Best For:** Precise queries ("all critical tickets", "open status", "billing category")

### 3. **Hybrid (RAG + MCP)**
- **Combined Approach:** LLM can choose between semantic search and structured tools
- **Best For:** Complex queries requiring both semantic understanding and exact filtering

---

## 🏗️ Project Structure

```
AzureOpenAI/
├── Program.cs                    # Entry point
├── UI/
│   ├── ConsoleUI.cs             # Menu system
│   └── DemoRunner.cs            # Demo orchestration
├── Services/
│   ├── RAG/
│   │   └── RagService.cs        # RAG implementation (2-phase pipeline)
│   ├── MCP/
│   │   ├── McpClient.cs         # MCP client (subprocess management)
│   │   └── McpPlugin.cs         # Function calling fallback (not true MCP)
│   ├── Hybrid/
│   │   ├── HybridService.cs     # Combined RAG + MCP
│   │   └── RagPlugin.cs         # RAG as plugin for hybrid mode
│   └── VectorStore/
│       ├── IVectorStore.cs      # Vector DB interface
│       └── InMemoryVectorStore.cs # In-memory vector storage
├── Models/
│   ├── SupportTicket.cs         # Ticket data model
│   ├── TicketStatus.cs          # Enum
│   ├── TicketPriority.cs        # Enum
│   └── SampleTickets.cs         # Sample data
└── McpServer/                    # Separate MCP Server Project
    ├── AzureOpenAI.McpServer.csproj
    ├── Program.cs                # MCP server host (stdio transport)
    └── SupportTicketTools.cs     # MCP tool implementations
```

---

## 📦 Architecture Details

### RAG Workflow

```
PHASE 1: INGESTION (One-time)
┌────────────┐
│  Tickets   │  (Sample data)
└─────┬──────┘
      │
      ▼
┌────────────┐
│ Text Chunk │  Format as searchable text
└─────┬──────┘
      │
      ▼
┌────────────┐
│ Embeddings │  text-embedding-3-small
└─────┬──────┘
      │
      ▼
┌────────────┐
│Vector Store│  In-memory cosine similarity search
└────────────┘

PHASE 2: INFERENCE (Runtime)
User Query → Embed Query → Search Vectors → Top K Results → LLM + Context → Response
```

**Implementation:** `Services/RAG/RagService.cs`
- `IndexTicketsAsync()`: Ingestion phase
- `SearchTicketsAsync()`: Inference phase

### MCP Workflow

```
┌─────────────────┐
│   MCP Client    │  (Main App - McpClient.cs)
└────────┬────────┘
         │ 1. Starts subprocess
         │ 2. JSON-RPC over stdio
         ▼
┌──────────────────────────────────┐
│     MCP Server (Subprocess)      │
│  - Uses ModelContextProtocol SDK │
│  - Stdio transport               │
│  - Tool discovery & execution    │
│                                  │
│  Tools:                          │
│  • GetTicketById                 │
│  • FilterByStatus                │
│  • FilterByPriority              │
│  • FilterByCategory              │
│  • GetTicketsAfterDate           │
│  • SearchByTag                   │
│  • GetAllTickets                 │
│  • GetTicketCountByStatus        │
└──────────────────────────────────┘
```

**Implementation:**
- **Client:** `Services/MCP/McpClient.cs` - Manages subprocess, JSON-RPC communication
- **Server:** `McpServer/Program.cs` + `McpServer/SupportTicketTools.cs` - Standalone MCP server using official SDK

**Key MCP Features:**
- ✅ True client-server architecture
- ✅ Separate process communication
- ✅ JSON-RPC 2.0 protocol
- ✅ Official `ModelContextProtocol` C# SDK (v0.5.0-preview.1)
- ✅ Stdio transport (stdin/stdout)
- ✅ Tool auto-discovery via attributes

### Hybrid Workflow

```
┌──────────────┐
│     User     │
└──────┬───────┘
       │
       ▼
┌──────────────┐
│     LLM      │  Decides which tool(s) to use
└──────┬───────┘
       │
       ├──────────────────────┐
       │                      │
       ▼                      ▼
┌─────────────┐     ┌──────────────────┐
│ RAG Plugin  │     │   MCP Plugin     │
│ (Semantic)  │     │  (Structured)    │
└─────────────┘     └──────────────────┘
       │                      │
       └──────────┬───────────┘
                  ▼
           Combined Result
```

**Implementation:** `Services/Hybrid/HybridService.cs`
- Registers both RAG and MCP tools as Semantic Kernel plugins
- LLM automatically chooses appropriate tool(s) based on query

---

## 🚀 Getting Started

### Prerequisites

- **.NET 10 SDK**
- **Azure OpenAI** account with:
  - Chat completion deployment (e.g., `o4-mini`)
  - Embedding deployment (e.g., `text-embedding-3-small`)

### Configuration

Set environment variables:

```bash
export AZURE_OPENAI_API_KEY="your-api-key"
```

Update `Program.cs` with your endpoint and deployment names:

```csharp
string endpoint = "https://your-resource.openai.azure.com/";
string chatDeploymentName = "o4-mini";
string embeddingDeploymentName = "text-embedding-3-small";
```

### Build

```bash
# Build main application
dotnet build AzureOpenAI.csproj

# Build MCP server
dotnet build McpServer/AzureOpenAI.McpServer.csproj
```

### Run

```bash
dotnet run
```

### Menu Options

```
Select a mode to run:

  1. RAG     - Semantic search using vector embeddings
  2. MCP     - Structured queries using function calling
  3. HYBRID  - Combined RAG + MCP approach
  4. DEMO    - Run all three with comparison
```

---

## 🧪 Example Queries

### RAG Mode (Semantic Understanding)

✅ **Good:**
- "Find tickets about authentication problems"
- "Show me billing issues"
- "What tickets mention password resets?"

❌ **Limitations:**
- "Show all critical tickets" (inefficient, should use MCP)
- "Get ticket #1002" (slow, should use MCP)

### MCP Mode (Structured Filtering)

✅ **Good:**
- "Show all critical priority tickets"
- "List open status tickets"
- "Get tickets in billing category"
- "Show tickets after 2024-12-20"

❌ **Limitations:**
- "Find authentication problems" (needs semantic search)
- "Show recent login issues" (needs RAG for "login issues")

### Hybrid Mode (Best of Both)

✅ **Excellent:**
- "Show critical authentication problems" (RAG for concept + MCP for priority)
- "Find high priority billing issues from last week" (combines all)
- "What are the urgent technical tickets?" (semantic + priority filter)

---

## 📊 Comparison Table

| Query Type | RAG Only | MCP Only | Hybrid |
|------------|----------|----------|--------|
| "Authentication problems" | ✅ Excellent | ❌ Needs exact tag | ✅ Excellent |
| "Critical tickets" | ⚠️ Inefficient | ✅ Perfect | ✅ Perfect |
| "Ticket #1002" | ⚠️ Works but slow | ✅ Instant | ✅ Instant |
| "Recent billing issues" | ⚠️ No date filter | ⚠️ No semantic | ✅ Both! |

---

## 🔑 Key Takeaways

### When to Use RAG
- Natural language queries
- Conceptual/semantic search
- "Find tickets about X"
- Similarity-based retrieval

### When to Use MCP
- Exact filtering (status, priority, category)
- Structured data queries
- Deterministic results
- Tool-based operations

### When to Use Hybrid
- Complex queries requiring both
- Production systems needing flexibility
- When query intent varies widely

---

## 🛠️ Technical Stack

- **.NET 10**
- **Microsoft.SemanticKernel** (v1.68.0) - Orchestration
- **Microsoft.SemanticKernel.Connectors.AzureOpenAI** - Azure OpenAI integration
- **Microsoft.Extensions.AI** - Embedding generation
- **ModelContextProtocol** (v0.5.0-preview.1) - Official MCP SDK

---

## 📝 Notes

### MCP Implementation

This project uses the **official Model Context Protocol C# SDK**:
- GitHub: https://github.com/modelcontextprotocol/csharp-sdk
- NuGet: `ModelContextProtocol` (preview)
- True client-server architecture with subprocess communication
- JSON-RPC 2.0 over stdio transport

### RAG Implementation

- **Vector Store:** In-memory implementation with cosine similarity
- **Embeddings:** Azure OpenAI text-embedding-3-small (1536 dimensions)
- **Chunking:** Simple text formatting (production would use advanced chunking)

### Hybrid Implementation

- Uses Semantic Kernel's plugin system
- LLM automatically routes to appropriate tools
- Can combine multiple tools in single query

---

## 🚧 Future Enhancements

- [ ] Persistent vector store (Qdrant, Pinecone, Azure AI Search)
- [ ] Advanced chunking strategies
- [ ] Streaming responses
- [ ] Authentication & authorization
- [ ] Production-grade error handling
- [ ] Metrics and telemetry
- [ ] Multi-tenant support
- [ ] Real-world data integration

---

## 📄 License

This is a demonstration project for educational purposes.

---

## 🤝 Contributing

This is a sandbox project. Feel free to experiment and extend!

---

**Built with ❤️ using Azure OpenAI, Semantic Kernel, and Model Context Protocol**
