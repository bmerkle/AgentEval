# Building AgentEval Documentation

This guide explains how to build the AgentEval project and generate comprehensive API documentation using DocFX.

## Prerequisites

- .NET SDK 8.0 or higher
- PowerShell or Command Prompt
- docfx tool (installed automatically by build scripts)

## Quick Start

### Option 1: PowerShell Script (Recommended)

```powershell
cd C:\git\joslat\AgentEval
.\scripts\build-documentation.ps1
```

### Option 2: Batch Script (Windows CMD)

```batch
cd C:\git\joslat\AgentEval
scripts\build-documentation.bat
```

### Option 3: GitHub Actions

Documentation is automatically built and published to GitHub Pages on each release. See `.github/workflows/docs.yml`.

## Manual Build Steps

If you prefer to run steps individually:

### 1. Build the Project

```bash
dotnet build src\AgentEval\AgentEval.csproj --configuration Release
```

This generates XML documentation files in:
- `src\AgentEval\bin\Release\net8.0\AgentEval.xml`
- `src\AgentEval\bin\Release\net9.0\AgentEval.xml`
- `src\AgentEval\bin\Release\net10.0\AgentEval.xml`

### 2. Install DocFX

```bash
dotnet tool install -g docfx
```

### 3. Generate API Metadata

```bash
cd docs
docfx metadata
```

This creates YAML files in `docs/api/` from the compiled assemblies.

### 4. Build HTML Documentation

```bash
docfx build
```

This generates the static website in `docs/_site/`.

## Viewing Documentation

### Local Preview

```bash
# Open directly in browser
start docs\_site\index.html

# Or serve locally with hot reload
docfx serve docs\_site
```

### Generated Structure

```
docs/_site/
├── index.html              # Landing page
├── api/                    # API reference
│   ├── AgentEval.html
│   ├── AgentEval.Adapters.html
│   ├── AgentEval.Assertions.html
│   ├── AgentEval.Core.html
│   ├── AgentEval.Embeddings.html
│   ├── AgentEval.MAF.html
│   ├── AgentEval.Metrics.html
│   ├── AgentEval.Models.html
│   └── AgentEval.Testing.html
├── articles/               # Additional articles
└── search.html             # Search functionality
```

## Project Namespaces

| Namespace | Purpose |
|-----------|---------|
| `AgentEval.Adapters` | Framework adapters (MAF, etc.) |
| `AgentEval.Assertions` | Assertion and validation utilities |
| `AgentEval.Benchmarks` | Performance benchmarking tools |
| `AgentEval.Core` | Core evaluation framework |
| `AgentEval.Embeddings` | Embedding models and utilities |
| `AgentEval.MAF` | Microsoft Agent Framework integration |
| `AgentEval.Metrics` | Evaluation metrics and scoring |
| `AgentEval.Models` | Data models and structures |
| `AgentEval.Testing` | Testing utilities and helpers |

## Troubleshooting

### Build fails with compilation errors

1. Check error messages in build output
2. Fix source code issues
3. Rebuild and try again

### docfx metadata fails

1. Verify docfx is installed: `dotnet tool list -g`
2. Check `docs/docfx.json` configuration
3. Ensure all `.csproj` files are valid

### No YAML files generated

1. Verify XML documentation was generated during build
2. Ensure the csproj has `<GenerateDocumentationFile>true</GenerateDocumentationFile>`
3. Check docfx output for specific errors

## Clean Rebuild

```bash
# Clean build artifacts
dotnet clean src\AgentEval\AgentEval.csproj

# Remove old documentation
Remove-Item -Recurse -Force docs\api, docs\_site -ErrorAction SilentlyContinue

# Rebuild everything
dotnet build src\AgentEval\AgentEval.csproj --configuration Release
cd docs
docfx metadata
docfx build
```

## Configuration

### docfx.json

Located at `docs/docfx.json`. Key settings:

- `TargetFramework`: Set to net10.0 (latest)
- `dest`: API metadata output folder (`api/`)
- `output`: HTML output folder (`_site/`)
- `template`: Using `default` and `modern` templates
- Search is enabled

### AgentEval.csproj

Documentation is enabled via:

```xml
<GenerateDocumentationFile>true</GenerateDocumentationFile>
<NoWarn>$(NoWarn);CS1591</NoWarn>
```

## Additional Resources

- [DocFX Documentation](https://dotnet.github.io/docfx/)
- [.NET XML Documentation Comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/)
