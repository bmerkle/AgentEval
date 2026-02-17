# MAF Upgrade Plan

## Executive Summary

**From:** `Microsoft.Agents.AI` `1.0.0-preview.251110.2`  
**To:** `Microsoft.Agents.AI` `1.0.0-preview.260212.1`  
**Date Range:** ~3 months of MAF development (Nov 2025 → Feb 2026)  
**Risk Level:** **HIGH** — Pervasive breaking renames and architectural pattern changes  
**Estimated Effort:** 2–3 focused development sessions  
**Files Requiring Changes:** 5 source files + 3 test files  
**Files Unaffected:** 2 source files (MAFEvaluationHarness, WorkflowEvaluationHarness — zero MAF coupling)

### Impact At a Glance

| Category | Count | Severity |
|----------|:-----:|----------|
| Type renames | 5 | 🔴 Breaking — every reference must update |
| Method renames | 4 | 🔴 Breaking — behavioral changes |
| Architectural pattern changes | 2 | 🔴 Breaking — abstract→protected+Core pattern |
| Removed APIs | 4 | 🔴 Breaking — replaced by new concepts |
| New APIs (additive) | 12+ | 🟢 Non-breaking — new capabilities |
| Dependency version bumps | 1 | 🟡 Medium — M.E.AI 10.0.0 → 10.3.0 |

### What Changed and Why

MAF `260212.1` introduces a major conceptual shift:

1. **Thread → Session**: `AgentThread` is replaced by `AgentSession` with a `StateBag` for typed state management. Serialization responsibility moves from the thread to the agent.
2. **AgentRunResponse → AgentResponse**: All response types drop the "Run" infix (simpler naming).
3. **Public abstract → Protected Core**: `RunAsync`/`RunStreamingAsync` are no longer directly overridable. The new pattern uses `protected abstract RunCoreAsync`/`RunCoreStreamingAsync` with the public methods handling cross-cutting concerns (e.g., `CurrentRunContext`).
4. **Structured Output**: New `RunAsync<T>()` overloads for type-safe deserialization.
5. **Workflow Observability**: ActivitySource moved from static always-on to opt-in `WithOpenTelemetry()`.

---

## 1. Breaking Changes — Detailed Analysis

### 1.1 Type Renames (Search & Replace)

These are mechanical renames. Every occurrence in AgentEval code must be updated.

| Old Type | New Type | Impact on AgentEval |
|----------|----------|---------------------|
| `AgentRunResponse` | `AgentResponse` | `MAFAgentAdapter`, `MAFIdentifiableAgentAdapter` — return type of `RunAsync()` |
| `AgentRunResponseUpdate` | `AgentResponseUpdate` | `MAFAgentAdapter`, `MAFIdentifiableAgentAdapter` — streaming return type |
| `AgentThread` | `AgentSession` | `MAFAgentAdapter`, `MAFIdentifiableAgentAdapter` — field type, constructor param |
| `AgentRunUpdateEvent` | `AgentResponseUpdateEvent` | `MAFWorkflowEventBridge` — pattern match case |
| `AgentRunResponseEvent` *(if used)* | `AgentResponseEvent` | Not currently used in AgentEval (only `AgentRunUpdateEvent` is matched) |

**Properties preserved under same name on `AgentResponse`:**
- `.Text` ✅
- `.Messages` ✅ (`.ToList()` still works)
- `.Usage` ✅
- `.Usage.InputTokenCount` ✅
- `.Usage.OutputTokenCount` ✅
- `.AgentId` ✅
- `.CreatedAt` ✅
- `.AdditionalProperties` ✅

### 1.2 Method Signature Changes

| Method | Old Signature | New Signature | AgentEval Impact |
|--------|--------------|--------------|------------------|
| `AIAgent.GetNewThread()` | `public abstract AgentThread GetNewThread()` | **REMOVED** → `public ValueTask<AgentSession> CreateSessionAsync(ct)` | 🔴 `MAFAgentAdapter` calls `_agent.GetNewThread()` — must change to `await _agent.CreateSessionAsync(ct)` (sync→async) |
| `AIAgent.RunAsync()` | `Task<AgentRunResponse> RunAsync(IEnumerable<ChatMessage>, AgentThread?, ...)` | `Task<AgentResponse> RunAsync(IEnumerable<ChatMessage>, AgentSession?, ...)` | 🔴 `MAFAgentAdapter` — param type + return type change |
| `AIAgent.RunStreamingAsync()` | `IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(..., AgentThread?, ...)` | `IAsyncEnumerable<AgentResponseUpdate> RunStreamingAsync(..., AgentSession?, ...)` | 🔴 `MAFAgentAdapter` — param type + return type change |
| `AIAgent.RunAsync(string, ...)` | `(string prompt, AgentThread?, ...)` | `(string prompt, AgentSession?, ...)` | 🔴 Same as above — convenience overload |

### 1.3 Removed APIs

| Removed API | Replacement | AgentEval Impact |
|-------------|------------|------------------|
| `AIAgent.GetNewThread()` | `AIAgent.CreateSessionAsync(CancellationToken)` | 🔴 Direct — used in both adapters |
| `AIAgent.DisplayName` | Removed (use `Name` directly) | ⚪ Not used by AgentEval |
| `AIAgent.DeserializeThread()` | `AIAgent.DeserializeSessionAsync()` | ⚪ Not used by AgentEval |
| `AIAgent.NotifyThreadOfNewMessagesAsync()` | Handled by `ChatHistoryProvider` | ⚪ Not used by AgentEval |
| `AgentRunResponse.UserInputRequests` | Removed | ⚪ Not used by AgentEval |

### 1.4 Architectural Pattern Changes

#### Abstract Method Pattern

MAFvnext changes the override pattern for custom agents:

```
// OLD: Direct override
public abstract Task<AgentRunResponse> RunAsync(...)

// NEW: Template method pattern  
public Task<AgentResponse> RunAsync(...)        // concrete — sets CurrentRunContext
protected abstract Task<AgentResponse> RunCoreAsync(...)  // override point
```

**AgentEval impact:** None directly — AgentEval does NOT subclass `AIAgent`. It only calls the public `RunAsync()`/`RunStreamingAsync()` methods, which still exist. However, the return types have changed (see §1.2).

#### DelegatingAIAgent Now Abstract

`DelegatingAIAgent` changed from `public class` to `public abstract class`. This means it cannot be instantiated directly. **AgentEval does not use `DelegatingAIAgent`**, so no impact.

### 1.5 Workflow Event Renames

| Old Event Type | New Event Type | Property Changes |
|----------------|---------------|------------------|
| `AgentRunUpdateEvent` | `AgentResponseUpdateEvent` | `.Update` type: `AgentRunResponseUpdate` → `AgentResponseUpdate` |
| `AgentRunResponseEvent` | `AgentResponseEvent` | `.Response` type: `AgentRunResponse` → `AgentResponse` |

**AgentEval impact:** `MAFWorkflowEventBridge.cs` pattern-matches on `AgentRunUpdateEvent` — must update to `AgentResponseUpdateEvent`.

### 1.6 ExecutorBindingExtensions Changes

| Old | New | Impact |
|-----|-----|--------|
| `BindAsExecutor(AIAgent, bool emitEvents = false)` | `BindAsExecutor(AIAgent, bool emitEvents)` (required) + `BindAsExecutor(AIAgent, AIAgentHostOptions? options = null)` | ⚪ AgentEval doesn't call the `AIAgent` overload |
| `BindAsExecutor<TIn, TOut>(Func, id)` | `BindAsExecutor<TIn, TOut>(Func, id, ExecutorOptions? = null, bool threadsafe = false)` | ⚪ Additive — test callers unaffected |

**AgentEval tests use the generic `BindAsExecutor<string, string>(id)` overload**, which gains optional parameters — no breaking change for tests.

### 1.7 InProcessExecution Changes

| Method | Change | AgentEval Impact |
|--------|--------|------------------|
| `StreamAsync<TInput>(...)` | **Unchanged** | ⚪ No impact |
| `StreamAsync(Workflow, ChatMessage, ...)` | **Unchanged** (generic `StreamAsync<ChatMessage>`) | ⚪ No impact |
| `ResumeStreamAsync(...)` | `string? runId` parameter **removed** | ⚪ Not used by AgentEval |
| `ResumeAsync(...)` | `string? runId` parameter **removed** | ⚪ Not used by AgentEval |

### 1.8 Dependency Version Changes

| Package | Current | Required by MAFvnext | Action |
|---------|---------|---------------------|--------|
| `Microsoft.Extensions.AI` | `10.0.0` | `10.3.0` | 🟡 Must bump in `Directory.Packages.props` |
| `Microsoft.Extensions.AI.Abstractions` | (transitive) | `10.3.0` | 🟡 Pulled transitively |
| `Microsoft.Extensions.VectorData.Abstractions` | (not referenced) | `9.7.0` | ⚪ Transitive, no action |

---

## 2. Additive Changes (Non-Breaking, New Capabilities)

These are new features in MAFvnext that AgentEval could optionally adopt:

| Feature | Description | Potential AgentEval Use |
|---------|-------------|----------------------|
| `AgentSession.StateBag` | Typed key-value state storage per session | Could expose in `TestResult` for session state inspection |
| `AIAgent.CurrentRunContext` | Ambient `AsyncLocal` context during runs | Could capture for tracing/diagnostics |
| `RunAsync<T>()` | Structured output with typed deserialization | Could add structured output assertions |
| `AgentRunOptions.ResponseFormat` | Request specific response formats | Could pass through for evaluation |
| `AgentRunOptions.AdditionalProperties` | Extensible options dictionary | Could use for custom evaluation metadata |
| `Workflow.ReflectExecutors()` | Reflection over bound executors | Could enhance `MAFGraphExtractor` |
| Edge `Label` property | Labels on workflow edges | Could include in `WorkflowGraphSnapshot` |
| `WorkflowTelemetryContext` | Opt-in workflow observability | Could enable during evaluation runs |
| `AIAgentHostOptions` | Rich executor hosting config | Could expose in workflow evaluation options |
| `LoggingAgent` | Built-in delegating agent for logging | Could use in evaluation harness |
| `ChatHistoryProvider` | Richer chat history lifecycle | Could integrate with trace recording |

---

## 3. No-Impact Changes (No AgentEval Code Affected)

| Change | Category | Reason No Impact |
|--------|----------|------------------|
| `AIAgent.Id` no longer `virtual` (uses `IdCore`) | Pattern change | AgentEval reads `.Id`, doesn't override it |
| `DelegatingAIAgent` now `abstract` | API change | Not used by AgentEval |
| `AIAgentMetadata` now `sealed` | API change | Not used by AgentEval |
| `AgentRunOptions` copy constructor now `protected` | Visibility | Not called by AgentEval |
| `ContinuationToken` type: `object?` → `ResponseContinuationToken?` | Type change | Not used by AgentEval |
| `DisplayName` removed | Property removal | Not used by AgentEval |
| `AIAgentBuilderExtensions` split into separate files | File reorganization | Not used by AgentEval |
| `Data/` directory removed from `Microsoft.Agents.AI` | File reorganization | Not used by AgentEval |
| `WorkflowMessageStore` removed | Type removal | Not used by AgentEval |
| `WorkflowThread` → `WorkflowSession` | Type rename | Not used by AgentEval |
| Checkpointing types | **Unchanged** | Stable API surface |
| Reflection types | **Unchanged** | Stable API surface |
| All Execution types | **Unchanged** | Stable API surface |
| `StreamingRun` | **Unchanged** | Used by EventBridge — no changes needed |
| `TurnToken` | **Unchanged** | Used by EventBridge — no changes needed |
| `WorkflowEvent` base class | **Unchanged** | Used by EventBridge — no changes needed |
| `EdgeKind` enum | **Unchanged** | Used by GraphExtractor — no changes needed |
| `EdgeInfo`, `DirectEdgeInfo` | **Unchanged** | Used by GraphExtractor — no changes needed |

---

## 4. File-by-File Update Plan

### 4.1 `MAFAgentAdapter.cs` — 🔴 MUST UPDATE

| Line Area | Change Required |
|-----------|----------------|
| `using` statements | No change needed (same namespace) |
| Field `_thread` type | `AgentThread` → `AgentSession` |
| Constructor param | `AgentThread? thread` → `AgentSession? session` |
| `GetNewThread()` call | `_agent.GetNewThread()` → `await _agent.CreateSessionAsync(ct)` (now async) |
| `RunAsync()` return | `AgentRunResponse` → `AgentResponse` |
| `RunStreamingAsync()` return | implicit via `_agent.RunStreamingAsync()` return type |
| Property access on response | `.Text`, `.Messages`, `.Usage` — **unchanged** ✅ |

**Detailed changes:**
```csharp
// BEFORE:
private AgentThread? _thread;
public MAFAgentAdapter(AIAgent agent, AgentThread? thread = null) { ... }
_thread ??= _agent.GetNewThread();
var response = await _agent.RunAsync(prompt, _thread, cancellationToken: ct);

// AFTER:
private AgentSession? _session;
public MAFAgentAdapter(AIAgent agent, AgentSession? session = null) { ... }
_session ??= await _agent.CreateSessionAsync(ct);
var response = await _agent.RunAsync(prompt, _session, cancellationToken: ct);
```

### 4.2 `MAFIdentifiableAgentAdapter.cs` — 🔴 MUST UPDATE

Identical changes to `MAFAgentAdapter.cs` (same pattern):
- `AgentThread` → `AgentSession`
- `GetNewThread()` → `await CreateSessionAsync(ct)`
- Response types renamed

### 4.3 `MAFWorkflowEventBridge.cs` — 🔴 MUST UPDATE

| Line Area | Change Required |
|-----------|----------------|
| Pattern match | `AgentRunUpdateEvent e` → `AgentResponseUpdateEvent e` |
| `.Update` property type | Implicit — `AgentRunResponseUpdate` → `AgentResponseUpdate` (same properties) |
| `.Update.Contents` access | **Unchanged** ✅ |
| `.Update.Text` access | **Unchanged** ✅ |

**Single change required:**
```csharp
// BEFORE:
case AgentRunUpdateEvent e:

// AFTER:
case AgentResponseUpdateEvent e:
```

All other MAF API usage in this file (`InProcessExecution.StreamAsync`, `StreamingRun`, `TurnToken`, `ChatProtocolExtensions`, other `WorkflowEvent` subclasses) is **unchanged**.

### 4.4 `MAFGraphExtractor.cs` — ⚪ NO CHANGES NEEDED

All APIs used are unchanged:
- `Workflow.ReflectEdges()` ✅
- `Workflow.StartExecutorId` ✅
- `EdgeInfo.Kind`, `EdgeInfo.Connection` ✅
- `DirectEdgeInfo.HasCondition` ✅
- `EdgeKind.Direct/FanOut/FanIn` ✅

### 4.5 `MAFWorkflowAdapter.cs` — ⚪ NO CHANGES NEEDED

Delegates to `MAFWorkflowEventBridge` and `MAFGraphExtractor`. No direct MAF type references beyond `Workflow` parameter pass-through, which is unchanged.

### 4.6 `MAFEvaluationHarness.cs` — ⚪ NO CHANGES NEEDED

Zero MAF dependencies. Uses only AgentEval abstractions.

### 4.7 `WorkflowEvaluationHarness.cs` — ⚪ NO CHANGES NEEDED

Zero MAF dependencies. Uses only AgentEval abstractions.

---

## 5. Test File Update Plan

### 5.1 `MAFWorkflowEventBridgeTests.cs` — ⚪ NO CHANGES NEEDED

Uses `WorkflowBuilder`, `ExecutorBindingExtensions.BindAsExecutor<string, string>()`, `.AddEdge()`, `.Build()`. All these APIs are unchanged. The generic `BindAsExecutor<TInput, TOutput>` overloads gain optional `ExecutorOptions?` and `bool threadsafe` parameters — existing callers unaffected.

### 5.2 `MAFGraphExtractorTests.cs` — ⚪ NO CHANGES NEEDED

Same pattern: `WorkflowBuilder`, `BindAsExecutor<string, string>()`, `.AddEdge()`, `.AddEdge<string>(condition:)`, `.AddFanOutEdge()`, `.AddFanInEdge()`, `.Build()`. All APIs unchanged or only gained optional parameters.

### 5.3 `MAFWorkflowAdapterFromMAFWorkflowTests.cs` — ⚪ NO CHANGES NEEDED

Same pattern: `WorkflowBuilder`, `BindAsExecutor<string, string>()`, `.AddEdge()`, `.Build()`. All APIs unchanged.

### 5.4 `MAFWorkflowAdapterTests.cs` — ⚪ NO CHANGES NEEDED

Zero MAF SDK references (pure AgentEval types).

### 5.5 `MAFWorkflowAdapterEdgeTests.cs` — ⚪ NO CHANGES NEEDED

Zero MAF SDK references (pure AgentEval types).

---

## 6. NuGet Package Updates

### `Directory.Packages.props` changes required:

```xml
<!-- BEFORE -->
<PackageVersion Include="Microsoft.Agents.AI" Version="1.0.0-preview.251110.2" />
<PackageVersion Include="Microsoft.Agents.AI.OpenAI" Version="1.0.0-preview.251110.2" />
<PackageVersion Include="Microsoft.Agents.AI.Workflows" Version="1.0.0-preview.251110.2" />
<PackageVersion Include="Microsoft.Extensions.AI" Version="10.0.0" />

<!-- AFTER -->
<PackageVersion Include="Microsoft.Agents.AI" Version="1.0.0-preview.260212.1" />
<PackageVersion Include="Microsoft.Agents.AI.OpenAI" Version="1.0.0-preview.260212.1" />
<PackageVersion Include="Microsoft.Agents.AI.Workflows" Version="1.0.0-preview.260212.1" />
<PackageVersion Include="Microsoft.Extensions.AI" Version="10.3.0" />
```

**Note:** `Microsoft.Agents.AI.Abstractions` is now a separate NuGet package but is transitively referenced by `Microsoft.Agents.AI`. No explicit reference needed.

**Note:** `Microsoft.Extensions.AI.OpenAI` may also need a version bump from `10.0.0-preview.1.25559.3` to match M.E.AI 10.3.0 compatibility. Verify at upgrade time.

---

## 7. Recommended Update Sequence

### Step 1: NuGet Version Bump
1. Update `Directory.Packages.props` with new MAF and M.E.AI versions
2. `dotnet restore` — verify package resolution
3. `dotnet build` — collect initial error list

### Step 2: Type Renames (Mechanical)
1. Global find-replace across `src/AgentEval/MAF/`:
   - `AgentRunResponse` → `AgentResponse` (but NOT `AgentRunResponseUpdate`)
   - `AgentRunResponseUpdate` → `AgentResponseUpdate`
   - `AgentThread` → `AgentSession`
   - `AgentRunUpdateEvent` → `AgentResponseUpdateEvent`
2. Verify no over-replacement (e.g., comments, strings)

### Step 3: API Migration
1. In `MAFAgentAdapter.cs` and `MAFIdentifiableAgentAdapter.cs`:
   - Change `GetNewThread()` → `await CreateSessionAsync(ct)`
   - Update field names `_thread` → `_session`
   - Update constructor parameter names

### Step 4: Build & Fix
1. `dotnet build` — resolve any remaining compile errors
2. Verify no new warnings from M.E.AI 10.3.0

### Step 5: Test
1. `dotnet test` — run all tests (×3 TFMs)
2. Verify MAF integration tests pass:
   - `MAFWorkflowEventBridgeTests`
   - `MAFGraphExtractorTests`
   - `MAFWorkflowAdapterFromMAFWorkflowTests`
3. Run samples that exercise MAF integration

### Step 6: Documentation
1. Update `src/AgentEval/MAF/Analysis-Microsoft-Agent-Framework-Integration.md` — bump version reference
2. Update CHANGELOG.md with breaking change notice
3. Consider documenting new MAF capabilities available to users

### Step 7: Cleanup
1. `Remove-Item -Recurse -Force MAFvnext/`
2. Copy MAFvnext source into `MAF/` for next diff cycle
3. Commit with `feat: upgrade MAF to 1.0.0-preview.260212.1`

---

## 8. Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| `CreateSessionAsync` behavioral difference from `GetNewThread` | Medium | Test both adapters with real MAF agents |
| M.E.AI 10.3.0 introduces breaking changes to `IChatClient` | Low | Check M.E.AI changelog before upgrading |
| `RunCoreAsync` pattern affects AgentEval behavior | None | AgentEval only calls public `RunAsync()` — the internal dispatch is transparent |
| `AgentResponse.Usage` property changes | None | Confirmed: same type, same name, same access pattern |
| Missing event types in pattern match | Low | Add `AgentResponseEvent` to pattern match if emitted (currently only `AgentResponseUpdateEvent` is matched) |
| Test `BindAsExecutor<string,string>` resolution changes | None | Generic overload only gains optional params |
| New MAF NuGet package not published yet | Medium | Verify `1.0.0-preview.260212.1` is on NuGet.org before upgrading |

---

## 9. Summary of Changes Per AgentEval Source File

| File | Changes | Effort |
|------|---------|--------|
| `MAFAgentAdapter.cs` | 6 changes (type renames + `GetNewThread` → `CreateSessionAsync`) | 🟡 Medium |
| `MAFIdentifiableAgentAdapter.cs` | 6 changes (identical to above) | 🟡 Medium |
| `MAFWorkflowEventBridge.cs` | 1 change (`AgentRunUpdateEvent` → `AgentResponseUpdateEvent`) | 🟢 Trivial |
| `MAFGraphExtractor.cs` | 0 changes | ⚪ None |
| `MAFWorkflowAdapter.cs` | 0 changes | ⚪ None |
| `MAFEvaluationHarness.cs` | 0 changes | ⚪ None |
| `WorkflowEvaluationHarness.cs` | 0 changes | ⚪ None |
| `Directory.Packages.props` | 4 version bumps | 🟢 Trivial |
| All 5 test files | 0 changes | ⚪ None |
| **Total** | **13 code changes + 4 version bumps** | |

---

## 10. New Package Structure

MAFvnext introduces `Microsoft.Agents.AI.Abstractions` as a separate NuGet package containing the core abstract types (`AIAgent`, `AgentSession`, `AgentResponse`, etc.). However, `Microsoft.Agents.AI` has a NuGet dependency on it, so it's transitively available. No new `<PackageReference>` needed in AgentEval's `.csproj` files.

New packages in the MAF ecosystem (not required by AgentEval):
- `Microsoft.Agents.AI.A2A` — Agent-to-Agent protocol
- `Microsoft.Agents.AI.AGUI` — Agent GUI protocol
- `Microsoft.Agents.AI.Anthropic` — Anthropic provider
- `Microsoft.Agents.AI.AzureAI` — Azure AI integration
- `Microsoft.Agents.AI.Declarative` — Declarative agent definitions
- `Microsoft.Agents.AI.DurableTask` — Durable Task orchestration
- `Microsoft.Agents.AI.Hosting` — Agent hosting infrastructure
- `Microsoft.Agents.AI.Workflows.Declarative` — Declarative workflows
- `Microsoft.Agents.AI.Workflows.Generators` — Source generators

---

*Generated by MAF upgrade preparation workflow. See `.github/instructions/maf-upgrade-preparation.instructions.md` for methodology.*
