# Generate API Documentation for AgentEval

## Quick Start

Run one of these commands from the repository root:

### Option 1: PowerShell (Recommended)
```powershell
.\build-documentation.ps1
```

### Option 2: Windows Batch
```cmd
build-documentation.bat
```

### Option 3: Manual Steps
```cmd
# 1. Build the project with XML documentation
dotnet build src\AgentEval\AgentEval.csproj -c Release

# 2. Verify XML files were generated
dir /s /b src\AgentEval\bin\Release\*.xml

# 3. Install docfx tool (if needed)
dotnet tool install -g docfx

# 4. Generate API metadata
cd docs
docfx metadata

# 5. Build HTML documentation
docfx build

# 6. View the documentation
cd _site
start index.html
```

## What Gets Generated

After running the build, you'll have:

1. **XML Documentation Files** (3 files, one per framework):
   - `src\AgentEval\bin\Release\net8.0\AgentEval.xml`
   - `src\AgentEval\bin\Release\net9.0\AgentEval.xml`
   - `src\AgentEval\bin\Release\net10.0\AgentEval.xml`

2. **API Metadata YAML** (in `docs\api\`):
   - One YAML file per namespace (10+ namespaces)
   - Contains all public types, methods, properties

3. **HTML Documentation Website** (in `docs\_site\`):
   - Searchable API reference
   - Namespace navigation
   - Type documentation with inherited members
   - Method signatures and XML comments

## Configuration Files

- **`src\AgentEval\AgentEval.csproj`** - Enabled `GenerateDocumentationFile`
- **`docs\docfx.json`** - DocFX configuration for metadata extraction and HTML generation

## Troubleshooting

### XML files not generated
- Check that the build succeeded: `dotnet build src\AgentEval\AgentEval.csproj -c Release`
- Verify `GenerateDocumentationFile` is set to `true` in the csproj

### docfx command not found
- Install the tool: `dotnet tool install -g docfx`
- Or install locally: `dotnet tool install docfx`

### YAML files not generated
- Run `docfx metadata` from the `docs` folder
- Check `docs\docfx.json` configuration

### HTML files not generated
- Run `docfx build` from the `docs` folder
- Check for errors in the console output

## Viewing the Documentation

### Local Preview
```cmd
cd docs
docfx serve _site
```
Then open http://localhost:8080 in your browser.

### Direct File Access
Open `docs\_site\index.html` in any web browser.

## CI/CD Integration

To automatically build and publish documentation on every commit, add a GitHub Actions workflow:

```yaml
name: Documentation

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build-docs:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'
    
    - name: Build with XML docs
      run: dotnet build src/AgentEval/AgentEval.csproj -c Release
    
    - name: Install DocFX
      run: dotnet tool install -g docfx
    
    - name: Generate docs
      run: |
        cd docs
        docfx metadata
        docfx build
    
    - name: Deploy to GitHub Pages
      if: github.event_name == 'push' && github.ref == 'refs/heads/main'
      uses: peaceiris/actions-gh-pages@v3
      with:
        github_token: ${{ secrets.GITHUB_TOKEN }}
        publish_dir: ./docs/_site
```

## Next Steps

After generating the documentation:

1. **Review the API reference** - Check that all public APIs are documented
2. **Add more XML comments** - Enhance method descriptions, parameter docs, and examples
3. **Add conceptual docs** - Create tutorial pages in `docs\` folder
4. **Configure GitHub Pages** - Publish to https://yourname.github.io/AgentEval/

## Resources

- [DocFX Documentation](https://dotnet.github.io/docfx/)
- [XML Documentation Comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/)
- [GitHub Pages](https://pages.github.com/)
