# CLI Reference

AgentEval ships a standalone CLI tool for running evaluations from the terminal, CI/CD pipelines, and shell scripts — without writing C# code.

## Installation

```bash
dotnet tool install --global AgentEval.Cli --prerelease
```

After installation the `agenteval` command is available system-wide.

## Quick Start

```bash
# Evaluate with Azure OpenAI
agenteval eval \
  --azure --model gpt-4o \
  --dataset tests.yaml \
  --format json -o results.json

# Evaluate with any OpenAI-compatible endpoint (Ollama, vLLM, Groq, etc.)
agenteval eval \
  --endpoint http://localhost:11434/v1 \
  --model llama3 \
  --dataset tests.yaml

# Pipe JSON to jq for quick inspection
agenteval eval --azure --model gpt-4o --dataset tests.yaml | jq '.passRate'
```

## Commands

### `agenteval eval`

Evaluate an AI agent against a dataset of test cases.

```
agenteval eval [options]
```

#### Required Options

| Option | Description |
|--------|-------------|
| `--dataset <file>` | Dataset file (YAML, JSON, JSONL, or CSV) |
| `--model <name>` | Model or deployment name |

#### Endpoint Options (choose one)

| Option | Description |
|--------|-------------|
| `--endpoint <url>` | OpenAI-compatible API endpoint URL |
| `--azure` | Use Azure OpenAI (reads `AZURE_OPENAI_ENDPOINT` and `AZURE_OPENAI_API_KEY` env vars) |

#### Authentication

| Option | Description |
|--------|-------------|
| `--api-key <key>` | API key (or set `OPENAI_API_KEY` / `AZURE_OPENAI_API_KEY` env var) |

> **Tip:** For local providers like Ollama that don't require authentication, omit `--api-key`.

#### Agent Configuration

| Option | Default | Description |
|--------|---------|-------------|
| `--system-prompt <text>` | *(none)* | System prompt text |
| `--system-prompt-file <file>` | *(none)* | Read system prompt from a file |
| `--temperature <float>` | `0` | Sampling temperature (0 = deterministic) |
| `--max-tokens <int>` | *(none)* | Maximum output tokens |

#### LLM-as-Judge

| Option | Description |
|--------|-------------|
| `--judge <url>` | Separate endpoint for LLM-as-judge scoring |
| `--judge-model <name>` | Model for the judge (defaults to `--model`) |

When `--judge` is provided, the evaluation harness uses a separate LLM to score agent responses using the `TaskCompletionMetric` and other LLM-backed metrics.

#### Output

| Option | Default | Description |
|--------|---------|-------------|
| `--format <fmt>` | `json` | Export format (see table below) |
| `-o, --output <file>` | stdout | Output file path |

**Supported formats:**

| Format | Aliases | Description |
|--------|---------|-------------|
| `json` | | JSON report with full details |
| `junit` | `xml` | JUnit XML for CI/CD integration |
| `markdown` | `md` | Human-readable Markdown table |
| `trx` | | Visual Studio TRX format |
| `csv` | | Comma-separated values |

#### Verbosity

| Option | Description |
|--------|-------------|
| `--verbose` | Show detailed progress during evaluation |
| `--quiet` | Suppress all output except the export data |

### Exit Codes

| Code | Meaning |
|------|---------|
| `0` | All tests passed |
| `1` | One or more tests failed |
| `2` | Usage error (invalid arguments) |
| `3` | Runtime error (network, auth, file not found) |

Exit codes are designed for **CI/CD integration** — a non-zero exit code fails the pipeline step.

---

## Dataset Format

The CLI uses `DatasetLoaderFactory` which auto-detects format by file extension.

### YAML (recommended)

```yaml
data:
  - id: capital_city
    input: What is the capital of France?
    expected: Paris
    expected_tools:
      - lookup_city

  - id: simple_math
    input: What is 7 * 8?
    expected: "56"
```

### JSON

```json
[
  { "input": "What is the capital of France?", "expected": "Paris" },
  { "input": "What is 7 * 8?", "expected": "56" }
]
```

### JSONL

```jsonl
{"input": "What is the capital of France?", "expected": "Paris"}
{"input": "What is 7 * 8?", "expected": "56"}
```

### CSV

```csv
input,expected
"What is the capital of France?","Paris"
"What is 7 * 8?","56"
```

---

## CI/CD Examples

### GitHub Actions

```yaml
- name: Evaluate AI Agent
  run: |
    agenteval eval \
      --azure --model gpt-4o \
      --dataset tests/eval-dataset.yaml \
      --format junit -o results.xml
  env:
    AZURE_OPENAI_ENDPOINT: ${{ secrets.AZURE_OPENAI_ENDPOINT }}
    AZURE_OPENAI_API_KEY: ${{ secrets.AZURE_OPENAI_API_KEY }}

- name: Publish Test Results
  uses: dorny/test-reporter@v1
  with:
    name: Agent Evaluation
    path: results.xml
    reporter: java-junit
```

### Azure DevOps

```yaml
- task: DotNetCoreCLI@2
  displayName: Install AgentEval CLI
  inputs:
    command: custom
    custom: tool
    arguments: install --global AgentEval.Cli --prerelease

- script: |
    agenteval eval \
      --azure --model gpt-4o \
      --dataset $(Build.SourcesDirectory)/tests/eval-dataset.yaml \
      --format trx -o $(Build.ArtifactStagingDirectory)/eval-results.trx
  displayName: Run Agent Evaluation
  env:
    AZURE_OPENAI_ENDPOINT: $(AZURE_OPENAI_ENDPOINT)
    AZURE_OPENAI_API_KEY: $(AZURE_OPENAI_API_KEY)

- task: PublishTestResults@2
  inputs:
    testResultsFormat: VSTest
    testResultsFiles: '**/*.trx'
```

---

## Provider Examples

### Azure OpenAI

```bash
agenteval eval --azure --model gpt-4o --dataset tests.yaml
```

### OpenAI

```bash
agenteval eval \
  --endpoint https://api.openai.com/v1 \
  --model gpt-4o \
  --api-key $OPENAI_API_KEY \
  --dataset tests.yaml
```

### Ollama (local)

```bash
agenteval eval \
  --endpoint http://localhost:11434/v1 \
  --model llama3.1 \
  --dataset tests.yaml
```

### LM Studio

```bash
agenteval eval \
  --endpoint http://localhost:1234/v1 \
  --model local-model \
  --dataset tests.yaml
```

### Groq

```bash
agenteval eval \
  --endpoint https://api.groq.com/openai/v1 \
  --model llama-3.1-70b-versatile \
  --api-key $GROQ_API_KEY \
  --dataset tests.yaml
```

### Together.ai

```bash
agenteval eval \
  --endpoint https://api.together.xyz/v1 \
  --model meta-llama/Llama-3-70b-chat-hf \
  --api-key $TOGETHER_API_KEY \
  --dataset tests.yaml
```

---

## Piping & Composition

The CLI follows Unix conventions: **data goes to stdout, messages to stderr**.

```bash
# Pipe JSON to jq
agenteval eval --azure --model gpt-4o --dataset tests.yaml | jq '.passRate'

# Save JSON and view Markdown
agenteval eval --azure --model gpt-4o --dataset tests.yaml -o results.json
agenteval eval --azure --model gpt-4o --dataset tests.yaml --format md

# Compare two models (run both, diff)
agenteval eval --azure --model gpt-4o --dataset tests.yaml -o gpt4o.json
agenteval eval --azure --model gpt-4o-mini --dataset tests.yaml -o gpt4omini.json
```

---

## Troubleshooting

| Error | Cause | Fix |
|-------|-------|-----|
| `Specify --endpoint <url> or --azure` | No endpoint provided | Add `--endpoint` or `--azure` |
| `Dataset not found` | File doesn't exist | Check `--dataset` path |
| `Dataset is empty` | File has no test cases | Ensure YAML/JSON has `data:` entries |
| `Unknown format 'xxx'` | Unsupported export format | Use: json, junit, xml, markdown, md, trx, csv |
| `Azure endpoint required` | Missing Azure config | Set `AZURE_OPENAI_ENDPOINT` env var |
| `Azure API key required` | Missing Azure key | Set `AZURE_OPENAI_API_KEY` env var |

---

## Architecture

The CLI is a thin shell over the same libraries you use in C# code:

```
agenteval eval --azure --model gpt-4o --dataset tests.yaml --format junit
       │
       ▼
  ┌─────────────────┐
  │  EvalCommand     │  Parse options, validate
  └────────┬────────┘
           ▼
  ┌─────────────────┐
  │ EndpointFactory  │  Create IChatClient (Azure or OpenAI-compatible)
  └────────┬────────┘
           ▼
  ┌─────────────────┐
  │ AsEvaluableAgent │  IChatClient → IStreamableAgent (zero boilerplate)
  └────────┬────────┘
           ▼
  ┌─────────────────┐
  │ DatasetLoader    │  YAML/JSON/JSONL/CSV → List<TestCase>
  └────────┬────────┘
           ▼
  ┌─────────────────────────┐
  │ MAFEvaluationHarness    │  Run tests, track tools & performance
  └────────┬────────────────┘
           ▼
  ┌─────────────────┐
  │ ExportHandler    │  Format routing → stdout or file
  └─────────────────┘
```

All evaluation logic lives in the shared libraries — the CLI adds no custom evaluation code.

## See Also

- [Getting Started](getting-started.md) - C# library quickstart
- [Export Formats](export.md) - Detailed format documentation
- [Cross-Framework Evaluation](cross-framework.md) - Using with any LLM provider
