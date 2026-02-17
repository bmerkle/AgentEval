---
applyTo: "src/AgentEval/MAF/**,tests/AgentEval.Tests/MAF/**,Directory.Packages.props"
description: Instructions for detecting and adapting to Microsoft Agent Framework (MAF) breaking changes
---

# MAF Update & Breaking Change Instructions

These instructions apply when updating the Microsoft Agent Framework (MAF) NuGet dependency in AgentEval.

## Two-Phase Upgrade Workflow

MAF upgrades follow two phases:

```
Phase 1 (OPTIONAL):  maf-upgrade-preparation.instructions.md
                     Diff MAF source → produce plan document
                     Output: src/AgentEval/MAF/MAF-Upgrade-Plan.md

Phase 2 (THIS FILE): Update NuGet → fix breaks → run tests
                     Input: plan document (if available) OR compile errors
```

Phase 1 is recommended for major MAF version changes. For minor/patch updates or Dependabot PRs, start directly at Step 0 below.

## Step 0: Check for an Existing Upgrade Plan

Before starting, check if `src/AgentEval/MAF/MAF-Upgrade-Plan.md` exists.

- **If it exists:** Read it first. It contains pre-analyzed breaking changes, behavioral changes, and concrete fix code snippets produced by Phase 1. Use it as a guide — you already know what will break and how to fix it.
- **If it doesn't exist:** Proceed normally with build-error-driven detection (Step 1).

## Where MAF Versions Are Pinned

All MAF package versions are pinned in a single file:

```
Directory.Packages.props
```

Look for these three entries:
```xml
<PackageVersion Include="Microsoft.Agents.AI" Version="..." />
<PackageVersion Include="Microsoft.Agents.AI.OpenAI" Version="..." />
<PackageVersion Include="Microsoft.Agents.AI.Workflows" Version="..." />
```

All three MUST be updated to the same version simultaneously.

## Step 1: Update the NuGet Version and Detect Breaking Changes

When a Dependabot PR arrives (or when manually bumping MAF versions):

1. Update the three `<PackageVersion>` entries in `Directory.Packages.props` to the new MAF version
2. Run:
   ```powershell
   dotnet restore --force
   dotnet build
   ```
3. Interpret results:
   - **Build succeeds** → No API breaking change. Proceed to Step 3 (run tests).
   - **Build fails with compile errors** → Breaking change detected. Proceed to Step 2.
   - **If an upgrade plan exists** → Compare compile errors against the plan. The plan should have predicted every failure. If there are surprises, investigate.

## Step 2: Fix MAF Breaking Changes

MAF-dependent code is confined to exactly these files in `src/AgentEval/MAF/`:

| File | MAF APIs Used | Most Likely to Break |
|------|--------------|---------------------|
| `MAFAgentAdapter.cs` | `AIAgent`, `RunAsync()`, `RunStreamingAsync()`, `GetNewThread()`, `AgentRunResponse` | Method signatures, response shape |
| `MAFIdentifiableAgentAdapter.cs` | Same as above + `IModelIdentifiable` | Same as above |
| `MAFWorkflowEventBridge.cs` | `Workflow`, `InProcessExecution`, `StreamingRun`, `WorkflowEvent` subclasses, `TurnToken` | Event hierarchy restructuring |
| `MAFGraphExtractor.cs` | `Workflow.ReflectEdges()`, `EdgeKind`, `EdgeInfo`, `DirectEdgeInfo`, `Checkpointing` APIs | Reflection/edge API surface |
| `MAFWorkflowAdapter.cs` | `Workflow` (only in `FromMAFWorkflow()` static factory) | Factory wiring |

### Fix procedure:

1. Read the compile errors — they will point to the exact file and line
2. Check MAF release notes or changelog for the API change
3. Update ONLY the affected adapter file(s) to match the new MAF API
4. **Do NOT change** any of these (they have zero MAF dependencies):
   - Core interfaces (`IEvaluableAgent`, `IStreamableAgent`, `IWorkflowEvaluableAgent`)
   - `MAFEvaluationHarness.cs` (despite the name, no MAF imports)
   - `WorkflowEvaluationHarness.cs` (no MAF imports)
   - Anything in `Core/`, `Models/`, `Metrics/`, `Assertions/`, `Comparison/`, `Tracing/`

### Common breaking change patterns and how to fix:

**`AIAgent.RunAsync()` signature changed:**
- Fix in `MAFAgentAdapter.InvokeAsync()` and `MAFIdentifiableAgentAdapter.InvokeAsync()`
- Map new parameters/return type to existing `AgentResponse` shape
- The `IEvaluableAgent.InvokeAsync()` contract stays unchanged

**`AgentRunResponse` properties renamed or restructured:**
- Fix in `MAFAgentAdapter.InvokeAsync()` — map new property names
- Key properties: `.Text`, `.Messages`, `.Usage`

**Workflow event types changed:**
- Fix in `MAFWorkflowEventBridge.cs` — update the event pattern matching
- Map new MAF event types to existing AgentEval `WorkflowEvent` records
- AgentEval's own event records (`ExecutorOutputEvent`, `ExecutorToolCallEvent`, etc.) stay unchanged

**`Workflow.ReflectEdges()` API changed:**
- Fix in `MAFGraphExtractor.ExtractGraph()`
- Map new edge model to existing `WorkflowGraphSnapshot`

**Streaming content model changed (`update.Contents`):**
- Fix in `MAFAgentAdapter.InvokeStreamingAsync()`
- Note: Content types (`TextContent`, `FunctionCallContent`, etc.) come from `Microsoft.Extensions.AI`, not MAF. If these change, the impact is broader.

## Step 3: Run Tests

After fixing compile errors (or if there were none):

```powershell
# Run MAF integration tests first (fastest feedback for MAF-specific issues):
dotnet test --filter "Category=MAFIntegration"

# Then run all tests to confirm nothing else broke:
dotnet test
```

### Test files that exercise real MAF objects (tagged with `[Trait("Category", "MAFIntegration")]`):
- `tests/AgentEval.Tests/MAF/MAFWorkflowEventBridgeTests.cs`
- `tests/AgentEval.Tests/MAF/MAFGraphExtractorTests.cs`
- `tests/AgentEval.Tests/MAF/MAFWorkflowAdapterFromMAFWorkflowTests.cs`

### Test files that do NOT use real MAF objects (if these fail, the issue is in AgentEval logic, not MAF):
- `MAFEvaluationHarnessTests.cs`
- `MAFWorkflowAdapterTests.cs`
- `MAFWorkflowAdapterEdgeTests.cs`
- `WorkflowEvaluationHarnessTests.cs`
- `WorkflowToolTrackingTests.cs`
- `MicrosoftEvaluatorAdapterTests.cs`

## Step 4: Update Compatibility Note

After successfully updating, add a one-line compatibility note to the release:

```
AgentEval X.Y.Z — Compatible with MAF <new-version>
```

## What NOT to Change During a MAF Update

- **Core interfaces** — `IEvaluableAgent`, `IStreamableAgent`, `IWorkflowEvaluableAgent` are the public API contract. MAF changes should NEVER require changes to these interfaces. If they do, something is architecturally wrong.
- **Models** — `AgentResponse`, `WorkflowExecutionResult`, `WorkflowEvent` records are AgentEval-owned types. Adapters translate MAF types into these; the models themselves don't change.
- **Metrics, Assertions, Tracing** — These work through the interfaces, never with MAF types directly.
- **Harnesses** — `MAFEvaluationHarness` and `WorkflowEvaluationHarness` have zero MAF imports despite their names.

## Versioning Rules

- AgentEval uses **independent SemVer** — version numbers reflect AgentEval's own changes, NOT MAF's version
- A MAF update that requires adapter changes → bump AgentEval **minor** version
- A MAF update that forces changes to core interfaces (shouldn't happen) → bump AgentEval **major** version
- A MAF update with zero code changes → bump AgentEval **patch** version (if releasing for compatibility note only)
- Do NOT try to match AgentEval version numbers to MAF version numbers

## Architecture Reference

```
src/AgentEval/
├── Core/           ← NO MAF deps — interfaces, extractors
├── Models/         ← NO MAF deps — AgentResponse, TestResult, etc.
├── Metrics/        ← NO MAF deps — all metrics
├── Assertions/     ← NO MAF deps — all assertions
├── Comparison/     ← NO MAF deps — stochastic runner, model comparison
├── Tracing/        ← NO MAF deps — trace record/replay
├── MAF/            ← ALL MAF deps confined here (4 files with imports)
│   ├── MAFAgentAdapter.cs              ← Microsoft.Agents.AI
│   ├── MAFIdentifiableAgentAdapter.cs  ← Microsoft.Agents.AI
│   ├── MAFWorkflowEventBridge.cs       ← Microsoft.Agents.AI.Workflows (heaviest)
│   ├── MAFGraphExtractor.cs            ← Microsoft.Agents.AI.Workflows + Checkpointing
│   ├── MAFWorkflowAdapter.cs           ← Microsoft.Agents.AI.Workflows (only in factory)
│   ├── MAFEvaluationHarness.cs         ← NO MAF deps (works through interfaces)
│   └── WorkflowEvaluationHarness.cs    ← NO MAF deps (works through interfaces)
└── AgentEval.csproj                    ← Package refs pinned in Directory.Packages.props
```

Only 4 out of 7 files in `src/AgentEval/MAF/` actually import MAF types. The other 3 are framework-agnostic and work entirely through `IEvaluableAgent`/`IStreamableAgent`/`IWorkflowEvaluableAgent`.

## Step 5: Clean Up After Upgrade

After a successful upgrade:

1. If `src/AgentEval/MAF/MAF-Upgrade-Plan.md` exists, update its status to "Completed" with the date, or delete it
2. If `/MAFvnext/` exists, tell the user to replace `/MAF/` contents with `/MAFvnext/` (so `/MAF/` reflects the new current version)
3. Commit all changes with message: `deps: Update MAF to <new-version>`

## Related Instructions

- **Pre-upgrade analysis:** `.github/instructions/maf-upgrade-preparation.instructions.md` — Diff MAF source before updating NuGet
- **Detailed architecture:** `src/AgentEval/MAF/Analysis-Microsoft-Agent-Framework-Integration.md` — Full MAF integration analysis, versioning decisions, extraction contingency plan
