# Sample Datasets

This directory contains example datasets for use with the AgentEval library.

## Formats Supported

AgentEval supports multiple dataset formats:
- **YAML** (`.yaml`, `.yml`) - Human-readable, supports comments
- **JSON** (`.json`) - Standard JSON arrays
- **JSONL** (`.jsonl`) - JSON Lines, one object per line
- **CSV** (`.csv`) - Comma-separated values

## Sample Files

### travel-agent.yaml
Agentic evaluation dataset for a travel booking agent. Demonstrates:
- Ground truth tool calls (`ground_truth` with `name` and `arguments`)
- Alternative field format (`function` + `arguments`)
- Expected tool lists
- Different input field aliases (`input`, `question`, `prompt`)
- Metadata for test categorization

### rag-qa.yaml
RAG (Retrieval-Augmented Generation) evaluation dataset. Demonstrates:
- Context documents for grounding
- Q&A format with expected answers
- Category-based organization
- Metadata for filtering

### dataloader-showcase.jsonl
JSONL dataset showcasing all DatasetTestCase properties. Demonstrates:
- `evaluation_criteria`, `tags`, `passing_score`
- Ground truth tool calls
- Field aliases (`question`/`answer`, `documents`/`tools`)
- Custom metadata (`difficulty`, `source`)

### dataloader-showcase.csv
CSV dataset showcasing pipe-separated multi-value fields. Demonstrates:
- Pipe-separated `evaluation_criteria`, `tags`, `expected_tools`
- Numeric `passing_score` column
- Context documents in CSV format

## Field Aliases

AgentEval supports flexible field naming:

| Canonical Field | Aliases |
|----------------|---------|
| `input` | `question`, `prompt`, `query` |
| `expected` | `expected_output`, `answer`, `response` |
| `context` | `contexts`, `documents` |
| `expected_tools` | `tools` |

## Additional Properties

| Property | Type | Description |
|----------|------|-------------|
| `evaluation_criteria` | `string[]` | Criteria for AI-powered evaluation |
| `tags` | `string[]` | Tags for categorizing/filtering test cases |
| `passing_score` | `int` | Minimum score (0-100) to pass; defaults to 70 |
| `metadata` | `object` | Custom key-value pairs; unknown columns/properties auto-collected |

> In CSV format, multi-value fields use pipe (`|`) as separator (e.g., `tag1|tag2`).

## Root Element Aliases

YAML/JSON files can use different root elements:
- `data` - Generic data array
- `examples` - Example test cases
- `samples` - Sample test cases
- `testCases` / `test_cases` - Explicit test cases

## Usage

```csharp
using AgentEval.DataLoaders;

// Load from any supported format
var loader = DatasetLoaderFactory.CreateFromExtension(".yaml");
var testCases = await loader.LoadAsync("travel-agent.yaml");

// Use in evaluation
foreach (var testCase in testCases)
{
    var result = await harness.RunEvaluationAsync(agent, testCase);
}
```

## Creating Custom Datasets

Minimal test case:
```yaml
- id: my_test
  input: What is 2+2?
  expected: 4
```

Full-featured test case:
```yaml
- id: complete_example
  category: Agentic
  input: Book a flight to Paris
  expected: I'll book that flight for you.
  ground_truth:
    name: book_flight
    arguments:
      destination: Paris
  expected_tools:
    - book_flight
    - check_availability
  context:
    - User prefers morning flights
    - Budget is $500
  evaluation_criteria:
    - Must confirm booking details
    - Should mention flight options
  tags:
    - travel
    - booking
  passing_score: 85
  metadata:
    priority: high
    author: test-team
```
