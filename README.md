# 🧩 Components.MCP.Blazor

> **A Model Context Protocol (MCP) server that exposes Blazor component metadata for AI-assisted development.**  
> It enables ChatGPT, Claude, and other MCP-capable tools to inspect your actual Blazor components — no guessing, no static docs.

---

## 🚀 Overview

**Components.MCP.Blazor** provides a **Model Context Protocol (MCP)** interface that lets AI agents and development tools dynamically discover your real Blazor components.  

It reflects all types inheriting from `ComponentBase` in your assemblies and exposes them as structured MCP tools.

### Why this matters

| Without MCP | With Components.MCP.Blazor |
|--------------|----------------------------|
| AI assistants hallucinate Razor parameters | AI reads your actual component metadata |
| Static component docs go out of sync | Metadata is live and updated automatically |
| You manually describe your UI components | AI tools introspect them directly via MCP |

This project bridges **AI and Blazor** — giving large language models the same understanding of your components that your IDE has.

---

## 🧰 Available MCP Tools

### 🔹 `components.list`

**Description:** Lists all discoverable Blazor components and their namespaces.

**Request Example**
```json
{
  "method": "tools/call",
  "params": {
    "name": "components.list"
  }
}
```

**Response Example**
```json
{
  "content": [
    {
      "type": "text",
      "text": [
        {
          "name": "NavMenu",
          "namespace": "MyApp.Components.Shared"
        },
        {
          "name": "UserCard",
          "namespace": "MyApp.Components.UI"
        }
      ]
    }
  ]
}
```

**Use case:**  
AI agents can query this tool to discover available components, then suggest or autogenerate Razor markup that references them correctly.

---

### 🔹 `component.details`

**Description:** Returns full metadata for a specific Blazor component by namespace and name.

**Request Example**
```json
{
  "method": "tools/call",
  "params": {
    "name": "component.details",
    "arguments": {
      "componentNamespace": "MyApp.Components.UI",
      "componentName": "UserCard"
    }
  }
}
```

**Response Example**
```json
{
  "content": [
    {
      "type": "text",
      "text": {
        "name": "UserCard",
        "namespace": "MyApp.Components.UI",
        "assembly": "MyApp.Components",
        "parameters": [
          {
            "name": "User",
            "type": "UserModel",
            "isRequired": true,
            "captureUnmatchedValues": false
          }
        ],
        "cascadingParameters": [],
        "injectedDependencies": [
          {
            "name": "Logger",
            "type": "ILogger<UserCard>",
            "isRequired": false,
            "captureUnmatchedValues": false
          }
        ]
      }
    }
  ]
}
```

**Use case:**  
This allows AI tools to understand the actual parameters and injected services for any component, enabling:
- Accurate Razor code generation  
- Intelligent documentation  
- Semantic search and completion  

---

## ⚙️ How It Works

1. Scans assemblies for all types inheriting from `ComponentBase`
2. Collects `[Parameter]`, `[CascadingParameter]`, and `[Inject]` property info
3. Exposes metadata through standardized MCP tools
4. AI agents or IDEs can query these tools live via HTTP (JSON-RPC)

---

## ⚙️ Setup

### 1. Configure inspected assemblies in `Program.cs`
```csharp
using Components.MCP.Blazor.Introspection;
using Components.MCP.Blazor.Tools;
using ModelContextProtocol.Server;
using Microsoft.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

// Configure which assemblies to scan
var discoveryOptions = new ComponentDiscoveryOptions();
discoveryOptions.Assemblies.Add(typeof(DynamicHeadContent).Assembly);

builder.Services.AddSingleton(discoveryOptions);
builder.Services.AddSingleton<ComponentMetadataProvider>();

builder.Services
    .AddMcpServer()
    .AddBuiltInHandlers()
    .WithHttpTransport()
    .WithTools<ComponentTool>();

var app = builder.Build();
await app.RunAsync();
```

### 2. Run the MCP server
```bash
dotnet run
```

---

## 🧱 Roadmap

- [ ] Add `components.search` tool for name-based filtering  
- [ ] Add `resources/read` for raw source inspection  
- [ ] Provide parameter documentation from XML comments

---

## 📄 License

MIT © 2025 Simon Liebers
