# LLM + MCP Integration Demo

## Architecture Overview

This demonstrates how to integrate **Model Context Protocol (MCP)** with **LLM function calling** for intelligent tool selection.

```
┌─────────────┐
│ User Query  │
└──────┬──────┘
       │
       ▼
┌────────────────────────────┐
│  LLM (Azure OpenAI)        │
│  "Show critical tickets"   │
└──────┬─────────────────────┘
       │ Function Calling
       ▼
┌────────────────────────────┐
│  Semantic Kernel           │
│  Selects: FilterByPriority │
└──────┬─────────────────────┘
       │
       ▼
┌────────────────────────────┐
│  MCP Client (Bridge)       │
│  Wrapper Functions         │
└──────┬─────────────────────┘
       │ JSON-RPC
       ▼
┌────────────────────────────┐
│  MCP Server (Subprocess)   │
│  Executes Tool             │
└──────┬─────────────────────┘
       │
       ▼
┌────────────────────────────┐
│  Result → LLM → User       │
└────────────────────────────┘
```

## Key Components

### 1. **MCP Server** (`McpServer/`)
- Standalone process that exposes 8 tools
- Communicates via JSON-RPC over stdin/stdout
- Language-agnostic (could be Python, Node.js, etc.)

### 2. **MCP Client** (`McpClient.cs`)
- Bridges MCP protocol to Semantic Kernel
- Creates wrapper functions for each MCP tool
- Manages subprocess lifecycle

### 3. **LLM Integration**
- Uses OpenAI function calling
- AI selects appropriate tool based on query
- Natural language → Structured tool calls

## Benefits

✅ **Intelligent Routing** - LLM understands intent, not just keywords
✅ **Protocol Compliance** - Uses real MCP protocol (JSON-RPC)
✅ **Language Agnostic** - MCP Server can be any language
✅ **Production Ready** - Bridges MCP to modern LLM frameworks

## Example Queries

All these work naturally:

- "What's ticket 1002 about?"
- "Show me all critical priority tickets"
- "How many tickets are in each status?"
- "Find tickets created after January 10th"
- "Show me everything tagged with authentication"

The LLM automatically:
1. Understands the intent
2. Selects the right MCP tool
3. Extracts parameters
4. Calls via MCP protocol
5. Formats the response

## Why This Approach?

This is the **practical way** to integrate MCP with LLMs:

- Most LLMs don't natively speak MCP protocol
- They DO support function calling (OpenAI, Anthropic, etc.)
- This bridges the gap while maintaining MCP benefits

Perfect for demos showing real-world LLM + MCP integration!
