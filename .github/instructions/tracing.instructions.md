---
applyTo: "src/AgentEval/Tracing/**/*.cs"
description: Guidelines for implementing trace recording and replay
---

# Tracing Implementation Guidelines

## Core Tracing Components

### Recording Agents
- `TraceRecordingAgent` - Wraps agent to capture executions
- `ChatTraceRecorder` - Records multi-turn conversations
- `WorkflowTraceRecorder` - Records multi-agent workflow steps

### Replay Agents
- `TraceReplayingAgent` - Replays recorded traces deterministically
- `WorkflowTraceReplayingAgent` - Replays workflow traces

### Serialization
- `TraceSerializer` - Save/load `AgentTrace` to/from JSON
- `WorkflowTraceSerializer` - Save/load `WorkflowTrace` to/from JSON

## AgentTrace Structure

```csharp
public class AgentTrace
{
    public string Version { get; set; }
    public string TraceName { get; set; }
    public DateTimeOffset CapturedAt { get; set; }
    public string? AgentName { get; set; }
    public string? ModelId { get; set; }
    public List<TraceEntry> Entries { get; set; }
    public TracePerformance? Performance { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class TraceEntry
{
    public TraceEntryType Type { get; set; }
    public int Index { get; set; }
    public string? Prompt { get; set; }
    public string? Text { get; set; }
    public long? DurationMs { get; set; }
    public TraceTokenUsage? TokenUsage { get; set; }
    public List<TraceToolCall>? ToolCalls { get; set; }
    public TraceError? Error { get; set; }
    public bool IsStreaming { get; set; }
    public List<TraceStreamChunk>? StreamingChunks { get; set; }
}
```

## Recording Pattern

```csharp
// Wrap real agent
await using var recorder = new TraceRecordingAgent(realAgent, "weather_test");

// Execute (calls real agent, captures result)
var response = await recorder.InvokeAsync("query");

// Get trace for storage
var trace = recorder.Trace;

// Save to file
await TraceSerializer.SaveToFileAsync(trace, "trace.json");
```

## Replay Pattern

```csharp
// Load saved trace
var trace = await TraceSerializer.LoadFromFileAsync("trace.json");

// Create replayer
var replayer = new TraceReplayingAgent(trace);

// Replay entries in order
while (!replayer.IsComplete)
{
    var response = await replayer.InvokeAsync("prompt");
    // Response is identical to original
}
```

## Multi-Turn Chat Recording

```csharp
var chatRecorder = new ChatTraceRecorder(chatAgent, "support_conv");

// Record conversation turns
await chatRecorder.AddUserTurnAsync("Hello");
await chatRecorder.AddUserTurnAsync("Book a flight");

// Get execution result and trace
var result = chatRecorder.GetResult();
var trace = chatRecorder.ToAgentTrace();
```

## Workflow Recording

```csharp
await using var recorder = new WorkflowTraceRecorder(workflowAgent, "workflow-name");
var result = await recorder.ExecuteWorkflowAsync("Plan trip");
var trace = recorder.Trace;
```

## Best Practices

### 1. Unique Trace IDs
Always generate unique IDs for traces:
```csharp
TraceId = Guid.NewGuid().ToString("N")
```

### 2. Capture Token Usage
Include token usage for cost analysis:
```csharp
TokenUsage = new TraceTokenUsage
{
    PromptTokens = 100,
    CompletionTokens = 50
}
```

### 3. Capture Errors
Record errors for debugging:
```csharp
Error = exception != null ? new TraceError
{
    Message = exception.Message,
    Type = exception.GetType().Name
} : null
```

### 4. JSON Serialization
Use System.Text.Json with proper options:
```csharp
var options = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};
```

## Test Patterns

Store traces alongside tests:
```
tests/
├── traces/
│   ├── weather-agent.json
│   ├── booking-workflow.json
│   └── chat-session.json
└── Tracing/
    └── TraceReplayTests.cs
```

Use replay in CI without API credentials:
```csharp
[Fact]
public async Task Agent_ShouldReplayCorrectly()
{
    var trace = await TraceSerializer.LoadFromFileAsync("traces/weather-agent.json");
    var replayer = new TraceReplayingAgent(trace);
    var response = await replayer.InvokeAsync("What's the weather?");
    Assert.Equal("expected response", response.Text);
}
```
