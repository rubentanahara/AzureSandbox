# IT Knowledge Base RAG Implementation

## Overview

This implementation transforms the RAG system from storing support tickets to storing **static IT knowledge documents** that help the LLM provide intelligent IT support assistance.

## Architecture

### Core Concept

The system now uses a **dual-purpose architecture**:

1. **Knowledge Base (RAG)**: Provides IT knowledge, policies, procedures, and troubleshooting guides
2. **Ticket Management (MCP Tools)**: Handles CRUD operations on support tickets

This separation creates a powerful IT support assistant that can:
- Answer "how-to" questions using the knowledge base
- Help troubleshoot issues with step-by-step guides
- Manage support tickets (create, search, filter)

## What's Stored in the Vector Store

The knowledge base contains:

### 1. **Policies**
- Password reset procedures
- Software installation policies
- Security policies
- Incident management policies

### 2. **Troubleshooting Guides**
- VPN connectivity issues
- Email problems
- Laptop performance optimization
- Network issues

### 3. **User Guides**
- MFA setup instructions
- Printer setup and troubleshooting
- Software installation guides

### 4. **Standard Operating Procedures (SOPs)**
- Escalation procedures
- Incident priority definitions
- SLA timeframes

## New Components

### Models

1. **`KnowledgeDocument`** - Represents a knowledge article
   - Id, Title, Content, DocumentType, Category, Tags
   - Metadata: LastUpdated, Source, Author
   - Chunking support: ParentDocumentId, ChunkIndex, TotalChunks

2. **`DocumentType`** - Enum for document classification
   - KnowledgeBaseArticle, Policy, Procedure, TechnicalDocumentation, FAQ, Troubleshooting, UserGuide, SOP

3. **`DocumentChunk`** - Represents a chunk of a large document

### Services

1. **`DocumentChunker`** (`Services/DocumentProcessing/DocumentChunker.cs`)
   - Splits large documents into smaller chunks (default: 1000 chars)
   - Implements overlap strategy (default: 200 chars) for context preservation
   - Breaks at sentence boundaries when possible

2. **`DocumentLoader`** (`Services/DocumentProcessing/DocumentLoader.cs`)
   - Loads documents from various sources (files, directories)
   - Supports multiple formats: Markdown (.md), Text (.txt), JSON (.json)
   - Auto-detects document type and category based on filename/path
   - Extensible for future formats (PDF, DOCX, etc.)

3. **`KnowledgeRagService`** (`Services/RAG/KnowledgeRagService.cs`)
   - Indexes knowledge documents into vector store
   - Chunks large documents automatically
   - Performs semantic search over knowledge base
   - Generates AI-powered answers using retrieved context

4. **`KnowledgeRagPlugin`** (`Services/Hybrid/KnowledgeRagPlugin.cs`)
   - Exposes knowledge search as a Semantic Kernel plugin
   - Allows LLM to autonomously search knowledge base

### Updated Components

1. **`IVectorStore<TMetadata>`** - Now generic to support different metadata types
2. **`InMemoryVectorStore<TMetadata>`** - Generic implementation
3. **`HybridQueryService`** - Updated to use knowledge base instead of ticket search
4. **`Program.cs`** - Instantiates both knowledge and ticket vector stores

## Sample Knowledge Base

Located in `Models/SampleKnowledgeBase.cs`, contains 8 comprehensive documents:

1. **Password Reset Policy** - Self-service and IT-assisted reset procedures
2. **VPN Troubleshooting** - Common VPN issues and solutions
3. **Email Troubleshooting** - Email configuration and common problems
4. **Software Installation Policy** - Approved software and request procedures
5. **Printer Setup Guide** - Network printer installation and troubleshooting
6. **Incident Escalation Policy** - Priority levels, SLAs, and escalation paths
7. **MFA Setup Guide** - Multi-factor authentication setup and troubleshooting
8. **Laptop Performance Guide** - Performance optimization and diagnostics

## How It Works

### Indexing Process

1. Knowledge documents are loaded (from `SampleKnowledgeBase` or external sources)
2. Large documents are chunked into smaller pieces
3. Each chunk is embedded with metadata (title, category, type, tags)
4. Embeddings and metadata are stored in vector store

### Query Process

1. User asks a question: "How do I reset my password?"
2. LLM analyzes the question
3. LLM calls `KnowledgeBase.SemanticKnowledgeSearch` with the query
4. Service performs semantic search to find relevant document chunks
5. Service builds context from top-K relevant chunks
6. LLM generates answer based on the knowledge base context
7. Response includes actionable guidance with references to source documents

### Hybrid Mode Benefits

**Knowledge Base provides:**
- Policies and procedures
- Troubleshooting steps
- Configuration instructions
- Best practices

**MCP Tools provide:**
- Ticket creation
- Ticket searching and filtering
- Status updates
- Historical ticket analysis

**Together, they enable queries like:**
- "My VPN isn't working, what should I do?" → Knowledge base provides troubleshooting
- "Create a ticket for printer issue on Floor 2" → MCP creates ticket
- "How do I reset a password and create a ticket for user John Doe?" → Both systems work together

## Chunking Strategy

### Why Chunking?

Large documents exceed embedding model token limits and reduce retrieval precision. Chunking:
- Keeps chunks within token limits
- Improves retrieval relevance
- Allows precise context extraction

### Implementation

- **Chunk Size**: 1000 characters (configurable)
- **Overlap**: 200 characters (preserves context across chunk boundaries)
- **Smart Boundaries**: Breaks at sentences or newlines when possible
- **Metadata Preservation**: Each chunk retains parent document metadata

### Example

```
Original Document (3000 chars)
↓
Chunk 1: chars 0-1000 (ends at sentence)
Chunk 2: chars 800-1800 (200 char overlap)
Chunk 3: chars 1600-2600
Chunk 4: chars 2400-3000
```

## Extensibility

### Adding New Documents

**Option 1: Code (Quick)**
```csharp
// Add to SampleKnowledgeBase.GetSampleDocuments()
new KnowledgeDocument
{
    Id = "kb-009",
    Title = "New Policy Document",
    Content = "...",
    DocumentType = DocumentType.Policy,
    Category = "Security",
    Tags = ["policy", "security"],
    // ...
}
```

**Option 2: Files (Scalable)**
```csharp
var loader = new DocumentLoader();
var docs = await loader.LoadFromDirectoryAsync("/path/to/knowledge/base");
await knowledgeRagService.IndexKnowledgeBaseAsync(docs);
```

### Supported File Formats

- **Markdown (.md)**: Best for formatted documentation
- **Text (.txt)**: Simple text documents
- **JSON (.json)**: Structured document metadata

### Adding New Formats

Extend `DocumentLoader`:

```csharp
case ".pdf":
    return await LoadPdfFileAsync(filePath);
case ".docx":
    return await LoadDocxFileAsync(filePath);
```

## System Prompt

The hybrid system uses an intelligent prompt that guides the LLM to:

1. **Identify query type**:
   - Knowledge question → Use KnowledgeBase
   - Ticket operation → Use MCP Tools
   - Both → Use both systems

2. **Provide context-aware responses**:
   - Reference specific documents
   - Include step-by-step instructions
   - Cite policies when applicable

3. **Handle edge cases**:
   - Unknown topics → Acknowledge and suggest escalation
   - Incomplete information → Ask for clarification

## Testing the System

### Run the Application

```bash
dotnet run
```

### Select Mode 3 (HYBRID)

This initializes both the knowledge base and ticket management tools.

### Example Queries

**Knowledge Base Queries:**
- "How do I reset a user's password?"
- "What's the VPN troubleshooting procedure?"
- "Tell me about the software installation policy"
- "How do I set up MFA?"
- "My laptop is slow, what should I do?"

**Ticket Management Queries:**
- "Show me all high priority tickets"
- "Create a ticket for email issue"
- "What's the status of ticket 1001?"
- "List open tickets in the Technical category"

**Combined Queries:**
- "Help me with VPN issues and create a ticket"
- "What's the password reset procedure and show me related tickets"

## Benefits of This Architecture

### For IT Support Staff
- Quick access to policies and procedures
- Consistent troubleshooting guidance
- Automated knowledge retrieval
- Efficient ticket management

### For End Users
- Self-service troubleshooting
- Clear, step-by-step instructions
- Fast answers to common questions
- Automated ticket creation

### For the Organization
- Centralized knowledge management
- Consistent policy enforcement
- Reduced resolution time
- Better knowledge retention

## Future Enhancements

1. **External Document Sources**
   - SharePoint integration
   - Confluence integration
   - Wiki scraping

2. **Document Management**
   - Version control
   - Document expiration/refresh
   - Approval workflows

3. **Advanced Retrieval**
   - Hybrid search (semantic + keyword)
   - Re-ranking
   - Query expansion

4. **Analytics**
   - Most searched topics
   - Knowledge gap identification
   - Document effectiveness metrics

5. **Real Vector Database**
   - Azure AI Search
   - Pinecone
   - Qdrant
   - Weaviate

## Technical Details

### Embedding Strategy

Each chunk is embedded with rich metadata:

```
Title: {document.Title}
Category: {document.Category}
Type: {document.DocumentType}
Tags: {comma-separated tags}

Content:
{chunk.Content}
```

This ensures the embedding captures both the content and context.

### Vector Store Architecture

- **Knowledge Vector Store**: `InMemoryVectorStore<KnowledgeDocument>`
- **Ticket Vector Store**: `InMemoryVectorStore<SupportTicket>`
- Both use the same generic implementation
- Separate stores prevent cross-contamination
- Production: Use persistent vector database

## Conclusion

This implementation transforms your RAG system into a powerful IT support assistant by:
- Storing static, curated knowledge instead of dynamic ticket data
- Separating knowledge retrieval from ticket management
- Providing comprehensive IT documentation and procedures
- Enabling intelligent, context-aware responses
- Supporting both human and AI-assisted troubleshooting

The system is production-ready and can be easily extended with additional documents, formats, and features.
