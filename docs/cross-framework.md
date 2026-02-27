# Cross-Framework Evaluation

AgentEval evaluates **any AI agent** regardless of the framework used to build it. The key abstraction is `IChatClient` from Microsoft.Extensions.AI — a universal interface that Azure OpenAI, OpenAI, Ollama, Semantic Kernel, LM Studio, Groq, and dozens of other providers implement.

## The Universal Adapter Pattern

```csharp
// One line to make any IChatClient evaluable:
IStreamableAgent agent = chatClient.AsEvaluableAgent(
    name: "MyAgent",
    systemPrompt: "You are a helpful assistant.");
```

The `AsEvaluableAgent()` extension method wraps any `IChatClient` into an `IStreamableAgent` — the interface AgentEval uses for all evaluation operations. No MAF boilerplate, no framework-specific adapters needed.

## Supported Providers

| Provider | Code | Notes |
|----------|------|-------|
| **Azure OpenAI** | `new AzureOpenAIClient(endpoint, key).GetChatClient(model).AsIChatClient()` | Production workloads |
| **OpenAI** | `new OpenAIClient(key).GetChatClient(model).AsIChatClient()` | Direct OpenAI API |
| **Semantic Kernel** | `kernel.GetRequiredService<IChatCompletionService>()` | SK agents with tools |
| **Ollama** | `new OllamaChatClient("http://localhost:11434", "llama3")` | Local models |
| **LM Studio** | `new OpenAIChatClient(new("lm-studio"), new("http://localhost:1234/v1"))` | Local models |
| **Groq** | `new OpenAIChatClient(new(key), new("https://api.groq.com/openai/v1"))` | Fast inference |
| **Together.ai** | `new OpenAIChatClient(new(key), new("https://api.together.xyz/v1"))` | Open models |
| **vLLM** | `new OpenAIChatClient(new("vllm"), new("http://localhost:8000/v1"))` | Self-hosted |
| **Any OpenAI-compat** | `EndpointFactory.CreateOpenAICompatible(url, model, key)` | Universal fallback |

## Framework Integration Examples

### Microsoft Agent Framework (MAF)

MAF agents get first-class support through `MAFAgentAdapter`:

```csharp
using AgentEval.MAF;
using Microsoft.Agents.AI;

// Create a MAF agent with tools
var agent = new ChatClientAgent(chatClient, new ChatClientAgentOptions
{
    Name = "TravelAgent",
    ChatOptions = new ChatOptions
    {
        Instructions = "You are a travel booking assistant.",
        Tools = [
            AIFunctionFactory.Create(SearchFlights),
            AIFunctionFactory.Create(BookFlight)
        ]
    }
});

// Wrap for evaluation
var evaluable = new MAFAgentAdapter(agent);

// Evaluate with full tool tracking
var harness = new MAFEvaluationHarness(verbose: true);
var result = await harness.RunEvaluationAsync(evaluable, testCase, new EvaluationOptions
{
    TrackTools = true,
    TrackPerformance = true
});

// Assert tools were called correctly
result.ToolUsage!.Should()
    .HaveCalledTool("SearchFlights", because: "must search before booking")
        .BeforeTool("BookFlight")
    .And()
    .HaveNoErrors();
```

### Semantic Kernel

Semantic Kernel plugins bridge to AgentEval via `AIFunctionFactory.Create()`:

```csharp
using Microsoft.SemanticKernel;
using Microsoft.Extensions.AI;

// Build a Semantic Kernel with plugins
var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion("gpt-4o", endpoint, key)
    .Build();
kernel.Plugins.AddFromType<FlightPlugin>();

// Bridge SK plugins to M.E.AI tools — same class, both frameworks!
var plugin = new FlightPlugin();
var tools = new List<AITool>
{
    AIFunctionFactory.Create(plugin.SearchFlights),
    AIFunctionFactory.Create(plugin.BookFlight)
};

// Create IChatClient and agent with tools
var chatClient = azureClient.GetChatClient("gpt-4o").AsIChatClient();
var agent = new ChatClientAgent(chatClient, new ChatClientAgentOptions
{
    ChatOptions = new ChatOptions { Tools = tools }
});
var adapter = new MAFAgentAdapter(agent);
var result = await harness.RunEvaluationAsync(adapter, testCase);
```

See the [NuGetConsumer SK Demo](../samples/AgentEval.NuGetConsumer/SemanticKernelDemo.cs) for the full working example.

### Plain IChatClient (No Framework)

For simple Q&A or zero-boilerplate evaluation:

```csharp
using Microsoft.Extensions.AI;

// Any IChatClient works directly
IChatClient client = new AzureOpenAIClient(endpoint, key)
    .GetChatClient("gpt-4o")
    .AsIChatClient();

var agent = client.AsEvaluableAgent(
    name: "GPT-4o",
    systemPrompt: "Answer concisely.");

var result = await harness.RunEvaluationAsync(agent, testCase);
```

### CLI (Any Provider, No Code)

The CLI wraps this pattern for terminal usage:

```bash
# Azure OpenAI
agenteval eval --azure --model gpt-4o --dataset tests.yaml

# Ollama (local)
agenteval eval --endpoint http://localhost:11434/v1 --model llama3 --dataset tests.yaml

# Groq
agenteval eval --endpoint https://api.groq.com/openai/v1 --model llama-3.1-70b \
  --api-key $GROQ_API_KEY --dataset tests.yaml
```

## Multi-Framework Model Comparison

Compare the same test cases across different providers and models:

```csharp
using AgentEval.Comparison;

// Define agent factories for different providers
var factories = new IAgentFactory[]
{
    new AzureOpenAIFactory("gpt-4o", endpoint, key),
    new AzureOpenAIFactory("gpt-4o-mini", endpoint, key),
    new OllamaFactory("llama3.1", "http://localhost:11434"),
};

// Run identical tests across all providers
var comparer = new ModelComparer(harness, statisticsCalculator: null);
foreach (var factory in factories)
{
    var agent = factory.CreateAgent();
    var result = await stochasticRunner.RunStochasticTestAsync(
        agent, testCase,
        new StochasticOptions(Runs: 5, SuccessRateThreshold: 0.8));
    results.Add((factory.ModelName, result));
}

results.PrintComparisonTable();
```

Output:
```
┌──────────────┬───────┬──────────┬──────────┬──────────┐
│ Model        │ Score │ Pass Rate│ Latency  │ Cost     │
├──────────────┼───────┼──────────┼──────────┼──────────┤
│ gpt-4o       │  94.2 │  100%    │  1.2s    │ $0.0045  │
│ gpt-4o-mini  │  87.5 │   80%    │  0.6s    │ $0.0008  │
│ llama3.1     │  72.1 │   60%    │  2.1s    │ $0.0000  │
└──────────────┴───────┴──────────┴──────────┴──────────┘
```

## Evaluation Capabilities by Integration Level

| Feature | Plain IChatClient | MAF Agent | Semantic Kernel |
|---------|:-:|:-:|:-:|
| Basic pass/fail | **Yes** | **Yes** | **Yes** |
| LLM-as-judge scoring | **Yes** | **Yes** | **Yes** |
| Performance metrics | **Yes** | **Yes** | **Yes** |
| Tool call tracking | No* | **Yes** | **Yes**† |
| Tool chain assertions | No* | **Yes** | **Yes**† |
| Streaming evaluation | **Yes** | **Yes** | **Yes** |
| Conversation testing | **Yes** | **Yes** | **Yes** |
| Trace record/replay | **Yes** | **Yes** | **Yes** |
| Stochastic evaluation | **Yes** | **Yes** | **Yes** |
| Model comparison | **Yes** | **Yes** | **Yes** |
| Red team security | **Yes** | **Yes** | **Yes** |

\* Tool tracking requires the agent to use `FunctionCallContent` in responses.  
† Via `AIFunctionFactory.Create()` bridge — SK plugin methods become M.E.AI tools tracked by AgentEval.

## The IChatClient Advantage

The `IChatClient` interface from `Microsoft.Extensions.AI` is the key enabler:

```
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│ Azure OpenAI │    │   Ollama     │    │  Semantic    │
│              │    │              │    │   Kernel     │
└──────┬───────┘    └──────┬───────┘    └──────┬───────┘
       │                   │                   │
       ▼                   ▼                   ▼
  ┌────────────────────────────────────────────────┐
  │              IChatClient interface              │
  │   GetResponseAsync() / GetStreamingResponseAsync()   │
  └────────────────────────┬───────────────────────┘
                           │
                           ▼
              ┌────────────────────────┐
              │  .AsEvaluableAgent()   │
              │  Zero-boilerplate wrap │
              └────────────┬───────────┘
                           │
                           ▼
              ┌────────────────────────┐
              │  AgentEval Evaluation  │
              │  Metrics, Assertions,  │
              │  Export, Comparison    │
              └────────────────────────┘
```

Any provider that speaks `IChatClient` gets the full AgentEval evaluation suite for free.

## See Also

- [Sample 27: Cross-Framework Evaluation](../samples/AgentEval.Samples/Sample27_CrossFrameworkEvaluation.cs) — Universal IChatClient adapter demo
- [NuGet Consumer SK Demo](../samples/AgentEval.NuGetConsumer/SemanticKernelDemo.cs) — Real Semantic Kernel integration
- [CLI Reference](cli.md) — Terminal-based evaluation for any provider
- [Model Comparison](model-comparison.md) — Compare models across providers
- [LLM-as-a-Judge](llm-as-judge.md) — Scoring with language models
- [Agentic Metrics](agentic-metrics.md) — Tool chain evaluation
