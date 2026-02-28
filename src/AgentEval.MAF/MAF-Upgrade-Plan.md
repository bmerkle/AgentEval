# MAF Upgrade Plan: 1.0.0-rc1 → 1.0.0-rc2

**Date:** February 28, 2026  
**Status:** ✅ Completed — All steps executed successfully, 7,345/7,345 tests pass  
**Prepared by:** AI agent (source diff analysis)  
**Analysis tool:** Phase 1 of MAF upgrade workflow (`.github/instructions/maf-upgrade-preparation.instructions.md`)

## Summary

Comprehensive source diff of **197+ files across 4 MAF packages** reveals **zero public API breaking changes** between RC1 and RC2. All types and methods that AgentEval depends on are **byte-for-byte identical**. The RC2 release contains only:

- Internal telemetry restructuring (session-level OTel spans in Workflows)
- Two internal resource leak fixes (`using` on `CancellationTokenSource`)
- Three new **additive** public APIs (all marked `[Experimental]`): Agent Skills system, builder-level context providers, stored-output-disabled client
- Package version bump (`RCNumber: 1 → 2`, `GitTag: 1.0.0-rc2`)
- Transitive `Microsoft.Agents.ObjectModel` version bump (`2026.2.3.1 → 2026.2.4.1`)

**Overall risk assessment: Very Low.** This is a safe, zero-effort upgrade from AgentEval's perspective.

---

## 1. Breaking Changes (compile errors expected)

### None.

Every public type and method that AgentEval depends on is identical between RC1 and RC2. Specifically verified:

| AgentEval Dependency | Package | Status |
|---------------------|---------|--------|
| `AIAgent` (class, all methods) | `Microsoft.Agents.AI.Abstractions` | ✅ Identical |
| `AgentSession` | `Microsoft.Agents.AI.Abstractions` | ✅ Identical |
| `AgentResponse` (.Text, .Messages, .Usage) | `Microsoft.Agents.AI.Abstractions` | ✅ Identical |
| `AgentResponseUpdate` (.Contents) | `Microsoft.Agents.AI.Abstractions` | ✅ Identical |
| `AgentRunOptions` | `Microsoft.Agents.AI.Abstractions` | ✅ Identical |
| `DelegatingAIAgent` | `Microsoft.Agents.AI.Abstractions` | ✅ Identical |
| `ChatClientAgent` (constructor, all methods) | `Microsoft.Agents.AI` | ✅ Identical |
| `ChatClientAgentOptions` (all properties) | `Microsoft.Agents.AI` | ✅ Identical |
| `Workflow` (ReflectEdges, StartExecutorId, DescribeProtocolAsync) | `Microsoft.Agents.AI.Workflows` | ✅ Identical |
| `WorkflowBuilder` | `Microsoft.Agents.AI.Workflows` | ✅ Identical |
| `InProcessExecution` (RunStreamingAsync) | `Microsoft.Agents.AI.Workflows` | ✅ Identical |
| `StreamingRun` (WatchStreamAsync, TrySendMessageAsync) | `Microsoft.Agents.AI.Workflows` | ✅ Identical |
| `TurnToken` (constructor, emitEvents) | `Microsoft.Agents.AI.Workflows` | ✅ Identical |
| `ExecutorBindingExtensions` (CreateFuncBinding) | `Microsoft.Agents.AI.Workflows` | ✅ Identical |
| ALL event types (19 types) | `Microsoft.Agents.AI.Workflows` | ✅ Identical |
| `ChatProtocolExtensions.IsChatProtocol()` | `Microsoft.Agents.AI.Workflows` | ✅ Identical |
| ALL Checkpointing types (EdgeInfo, DirectEdgeInfo, EdgeKind) | `Microsoft.Agents.AI.Workflows` | ✅ Identical |

Full package-level breakdown:
- `Microsoft.Agents.AI.Abstractions`: **29/29 files identical** — zero changes
- `Microsoft.Agents.AI.Workflows`: **191/197 files identical** — 6 files changed (all `internal`)
- `Microsoft.Agents.AI`: **Core types identical** — new files added (additive only)
- `Microsoft.Agents.AI.OpenAI`: **All files identical** — 1 new method added (additive only)

---

## 2. Behavioral Changes (compiles but may fail tests)

### 2.1 Internal Telemetry: Session-Level OTel Spans

- **MAF change:** Workflow execution now wraps runs in a parent "session" activity span. Individual `workflow_invoke` activities are nested under a new `workflow.session` activity. Error events are recorded on both session and run activities.
- **Affected files:**
  - `Execution/LockstepRunEventStream.cs` (internal sealed)
  - `Execution/StreamingRunEventStream.cs` (internal sealed)
  - `Observability/ActivityNames.cs` (internal static — constant renamed: `WorkflowRun` → `WorkflowInvoke`, same value `"workflow_invoke"`)
  - `Observability/EventNames.cs` (internal static — 3 new constants: `session.started/completed/error`)
  - `Observability/Tags.cs` (internal static — new `ErrorMessage` constant)
  - `Observability/WorkflowTelemetryContext.cs` (internal sealed — new `StartWorkflowSessionActivity()` method)
- **Affected AgentEval file(s):** None. These are all `internal` types. AgentEval does not access workflow telemetry internals.
- **Existing test coverage:** N/A (telemetry internals not tested by AgentEval)
- **Suggested test addition:** None needed.

### 2.2 Internal Bug Fixes: Resource Leak Prevention

- **MAF change:** `CancellationTokenSource` now has `using` keyword in both `LockstepRunEventStream` and `StreamingRunEventStream`, preventing resource leaks.
- **Affected AgentEval file(s):** None.
- **Existing test coverage:** N/A
- **Suggested test addition:** None needed. This is a pure improvement.

### 2.3 Internal Tag Rename: Error Message OTel Tag

- **MAF change:** OTel error tag changed from `Tags.BuildErrorMessage` (method/property) to `Tags.ErrorMessage` (constant string `"error.message"`). This may produce slightly different OTel trace attribute names if consumers inspect raw traces.
- **Affected AgentEval file(s):** None. AgentEval does not inspect MAF OTel traces.
- **Existing test coverage:** N/A
- **Suggested test addition:** None needed.

---

## 3. Deprecations (compiles with warnings)

### None.

No APIs were deprecated between RC1 and RC2. No `[Obsolete]` attributes were added to any public types or members.

---

## 4. New APIs Worth Adopting

### 4.1 Agent Skills System (`[Experimental]`)

- **New API:**
  - `FileAgentSkillsProvider` : `AIContextProvider` — Discovers `SKILL.md` files with YAML frontmatter, implements progressive disclosure (advertise → `load_skill` tool → `read_skill_resource` tool)
  - `FileAgentSkillsProviderOptions` — Configuration with `SkillsInstructionPrompt` template
- **What it does:** Implements the [Agent Skills spec](https://agentskills.io/) for file-based skill injection into agents
- **AgentEval benefit:** Could be tested as a new sample scenario — evaluating agents with skill injection. Could also be used to test tool-call assertions on the `load_skill`/`read_skill_resource` tools.
- **Adopt now?** No — marked `[Experimental]`, not stable yet. Revisit at MAF GA.

### 4.2 Builder-Level Context Providers

- **New API:** `ChatClientBuilder.UseAIContextProviders(params AIContextProvider[] providers)` — Extension method on `ChatClientBuilder`
- **What it does:** Allows adding `AIContextProvider` instances directly into an `IChatClient` middleware pipeline (rather than at the agent level)
- **AgentEval benefit:** Could benefit `ChatClientAgentAdapter` scenarios where context providers are needed without a full MAF agent.
- **Adopt now?** No — niche use case, evaluate when needed.

### 4.3 Stored-Output-Disabled Client

- **New API:** `ResponsesClient.AsIChatClientWithStoredOutputDisabled()` — Returns an `IChatClient` with `StoredOutputEnabled = false`
- **What it does:** Creates an OpenAI Responses-backed `IChatClient` that doesn't store outputs
- **AgentEval benefit:** Minimal — specific to OpenAI Responses API usage pattern.
- **Adopt now?** No.

---

## 5. No Impact

Changes in MAF areas AgentEval doesn't use (listed for completeness):

| Area | Change | Package |
|------|--------|---------|
| `MessageAIContextProviderAgent.cs` moved to `AIContextProviderDecorators/` subfolder | Internal file reorganization | `Microsoft.Agents.AI` |
| `InjectSharedDiagnosticIds` build prop added | Build infrastructure | `Microsoft.Agents.AI` |
| `InjectExperimentalAttributeOnLegacy` build prop added | Build infrastructure (netstandard2.0 compat) | `Microsoft.Agents.AI` |
| `MAAI001` added to NoWarn | Build config | `Microsoft.Agents.AI` |
| `Microsoft.Agents.ObjectModel` `2026.2.3.1 → 2026.2.4.1` | Transitive dep version bump | `Microsoft.Agents.AI.Workflows` |
| New packages in MAFVnext repo: `Microsoft.Agents.AI.FoundryMemory`, `Microsoft.Agents.AI.Workflows.Declarative.Mcp` | New packages (not referenced by AgentEval) | N/A |

---

## 6. Recommended Update Sequence

1. **Update `Directory.Packages.props`** — Change all 3 MAF package versions:
   ```xml
   <PackageVersion Include="Microsoft.Agents.AI" Version="1.0.0-rc2" />
   <PackageVersion Include="Microsoft.Agents.AI.OpenAI" Version="1.0.0-rc2" />
   <PackageVersion Include="Microsoft.Agents.AI.Workflows" Version="1.0.0-rc2" />
   ```

2. **Restore and build:**
   ```powershell
   dotnet restore --force
   dotnet build
   ```
   Expected result: **Build succeeds with zero errors** (no code changes needed).

3. **Run MAF integration tests first:**
   ```powershell
   dotnet test --filter "Category=MAFIntegration"
   ```
   Expected result: **All pass** (event types, builder APIs, graph extraction — all identical).

4. **Run full test suite:**
   ```powershell
   dotnet test
   ```
   Expected result: **All 7,345+ tests pass across 3 TFMs** (no behavioral changes in public API).

5. **Update compatibility note** in release:
   ```
   AgentEval 0.6.0-beta — Compatible with MAF 1.0.0-rc2
   ```

6. **Clean up MAF reference source:**
   ```powershell
   # Move MAFVnext to MAF (so /MAF/ reflects the new current version)
   Remove-Item -Recurse -Force "MAF"
   Rename-Item "MAFVnext" "MAF"
   ```

7. **Update CHANGELOG.md** with the upgrade entry.

---

## 7. Estimated Effort

| Metric | Value |
|--------|-------|
| Files to modify | **1** (`Directory.Packages.props`) |
| Lines to change | **3** (version strings only) |
| New tests needed | **0** |
| AgentEval source code changes | **0** |
| Risk of regressions | **Very Low** |
| Estimated time | **15 minutes** (including test run) |

---

## Appendix: Diff Methodology

### Packages Analyzed

| Package | RC1 Files | RC2 Files | Files Changed | Files Identical |
|---------|-----------|-----------|---------------|-----------------|
| `Microsoft.Agents.AI.Abstractions` | 29 | 29 | 0 | 29 |
| `Microsoft.Agents.AI.Workflows` | 197 | 197 | 6 (all internal) | 191 |
| `Microsoft.Agents.AI` | ~15 | ~20 | 1 (.csproj) | ~14 |
| `Microsoft.Agents.AI.OpenAI` | ~8 | ~8 | 1 (new method) | ~7 |

### Packages Skipped (AgentEval doesn't reference)

Per the upgrade preparation instructions, the following MAF packages were not diffed:
- `Microsoft.Agents.AI.A2A`
- `Microsoft.Agents.AI.AGUI`
- `Microsoft.Agents.AI.Anthropic`
- `Microsoft.Agents.AI.AzureAI` / `.Persistent`
- `Microsoft.Agents.AI.CopilotStudio`
- `Microsoft.Agents.AI.CosmosNoSql`
- `Microsoft.Agents.AI.Declarative`
- `Microsoft.Agents.AI.DevUI`
- `Microsoft.Agents.AI.DurableTask`
- `Microsoft.Agents.AI.FoundryMemory` (new in RC2)
- `Microsoft.Agents.AI.GitHub.Copilot`
- `Microsoft.Agents.AI.Hosting` variants
- `Microsoft.Agents.AI.Mem0`
- `Microsoft.Agents.AI.Purview`
- `Microsoft.Agents.AI.Workflows.Declarative` variants
- `Microsoft.Agents.AI.Workflows.Generators`

### References

- [MAF Integration Analysis](../../strategy/MAF-Integration-Analysis.md) — Section 6 (MAF Surface Area & Coupling Points)
- [ADR-013](../../docs/adr/013-maf-rc1-upgrade.md) — Previous Preview → RC1 upgrade
- [maf-upgrade-preparation.instructions.md](../../.github/instructions/maf-upgrade-preparation.instructions.md) — Phase 1 procedure
- [maf-updates.instructions.md](../../.github/instructions/maf-updates.instructions.md) — Phase 2 procedure
