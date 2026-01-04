# AgentEval Documentation Build Guide

## Overview
This guide explains how to build the AgentEval project and generate comprehensive XML and HTML documentation.

## Prerequisites
- .NET SDK 8.0 or higher installed
- PowerShell or Command Prompt
- docfx tool (will be installed automatically if not present)

## Project Configuration
The AgentEval.csproj is configured with the following documentation settings:
- **XML Documentation**: Enabled via `<GenerateDocumentationFile>true</GenerateDocumentationFile>`
- **Target Frameworks**: net8.0, net9.0, net10.0
- **Documentation Suppression**: CS1591 warnings are suppressed for missing XML comments

## Build Steps

### Quick Start (Automated)

#### Option 1: Using Batch Script (Windows Command Prompt)
```batch
cd C:\git\joslat\AgentEval
build-documentation.bat
```

#### Option 2: Using PowerShell Script
```powershell
cd C:\git\joslat\AgentEval
.\build-documentation.ps1
```

### Manual Steps

If you prefer to run the steps individually:

#### Step 1: Build the Project
```bash
dotnet build src\AgentEval\AgentEval.csproj --configuration Release
```

**Expected Output:**
- Build succeeds with zero errors
- Compilation warnings may appear but are generally non-blocking
- Output will show the build time and summary

**Generated Files:**
- `src\AgentEval\bin\Release\net8.0\AgentEval.xml`
- `src\AgentEval\bin\Release\net9.0\AgentEval.xml`
- `src\AgentEval\bin\Release\net10.0\AgentEval.xml`

---

#### Step 2: Verify XML Documentation Files
```bash
# List XML files in Release output
Get-ChildItem -Path "src\AgentEval\bin\Release" -Include "*.xml" -Recurse
```

**Expected Output:**
- Should list .xml files for each target framework

---

#### Step 3: Install DocFX Tool
```bash
# Check if docfx is already installed
dotnet tool list -g | findstr docfx

# If not installed, install it
dotnet tool install -g docfx
```

**Expected Output:**
- If already installed: "docfx    <version>"
- If installing: "Tool 'docfx' (version x.x.x) was successfully installed."

---

#### Step 4: Generate API Metadata
```bash
cd docs
docfx metadata
cd ..
```

**What This Does:**
- Reads all .csproj files found in ../src
- Extracts API documentation from the compiled assemblies
- Generates YAML files in the api/ folder
- Creates table of contents (toc.yml) for API documentation

**Expected Output:**
- "Build succeeded with 0 warning(s)"
- Generated YAML files in `docs/api/` folder

**Generated Files:**
- `docs\api\AgentEval.*.yml` (one for each public namespace/type)
- `docs\api\toc.yml` (table of contents)

---

#### Step 5: Check Generated API YAML Files
```bash
# List generated API metadata
Get-ChildItem -Path "docs\api" -Include "*.yml" -Recurse
```

**Expected Output:**
- Multiple .yml files organized by namespace
- Each file contains the API documentation in YAML format

**Sample Files to Look For:**
- `docs\api\toc.yml` - Main API table of contents
- `docs\api\AgentEval.yml` - Root namespace documentation
- `docs\api\AgentEval.*.yml` - Subnamespace files

---

#### Step 6: Build HTML Documentation
```bash
cd docs
docfx build
cd ..
```

**What This Does:**
- Processes all .md and .yml files in the docs folder
- Combines manual documentation with API reference
- Generates static HTML files in _site/ folder
- Applies templates and styling

**Expected Output:**
- "Build succeeded with 0 warning(s)"
- Message indicating generation time and file count

**Generated Files:**
- `docs\_site\` - Complete static website
- `docs\_site\index.html` - Homepage
- `docs\_site\api\` - API reference pages
- `docs\_site\articles\` - Manual documentation pages

---

#### Step 7: Verification and Reporting

**Check for Errors:**
```bash
# Look for the build result
# Successful build will show: "Build succeeded"
# Failed build will show: "Build failed" with error details
```

**Error Categories:**

1. **Compilation Errors**
   - Indicates issues in the source code
   - Must be fixed before documentation can be generated

2. **Documentation Generation Errors**
   - Missing or malformed XML documentation
   - Broken cross-references
   - May be non-blocking depending on severity

3. **DocFX Errors**
   - Configuration issues in docfx.json
   - Missing or invalid template files
   - Invalid markdown syntax

---

## Viewing the Generated Documentation

### Local Preview
```bash
# Option 1: Open directly in browser
start docs\_site\index.html

# Option 2: Serve locally (if docfx has built-in server)
docfx serve docs\_site
```

### Content Structure
```
docs\_site\
├── index.html              - Main landing page
├── api\                    - API reference documentation
│   ├── AgentEval.html
│   ├── AgentEval.Adapters.html
│   ├── AgentEval.Assertions.html
│   ├── AgentEval.Core.html
│   ├── AgentEval.Embeddings.html
│   ├── AgentEval.MAF.html
│   ├── AgentEval.Metrics.html
│   ├── AgentEval.Models.html
│   └── AgentEval.Testing.html
├── articles\               - Additional documentation articles
├── images\                 - Images and assets
└── search.html            - Search functionality (if enabled)
```

---

## Documentation Organization

### Namespaces
The AgentEval project is organized into the following namespaces:

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

---

## Troubleshooting

### Issue: Build fails with compilation errors
**Solution:**
1. Check the error messages in the build output
2. Fix the source code issues
3. Rebuild and try again

### Issue: docfx metadata fails
**Solution:**
1. Verify docfx is installed: `dotnet tool list -g`
2. Check docfx.json configuration
3. Ensure all .csproj files are valid

### Issue: No YAML files generated
**Solution:**
1. Verify that XML documentation was generated in Step 2
2. Ensure the csproj file has `<GenerateDocumentationFile>true</GenerateDocumentationFile>`
3. Check the docfx output for specific errors

### Issue: HTML build fails
**Solution:**
1. Check for errors in the metadata generation step
2. Verify all markdown files are valid
3. Check docfx templates are correct

### Issue: XML files not in expected location
**Solution:**
1. Build may have been in a different configuration
2. Check bin/Release folder for actual output
3. Verify target framework (net8.0, net9.0, or net10.0)

---

## Configuration Files

### docfx.json
Location: `docs/docfx.json`

```json
{
  "metadata": [
    {
      "src": [
        {
          "src": "../src",
          "files": ["**/*.csproj"]
        }
      ],
      "dest": "api",
      "properties": {
        "TargetFramework": "net10.0"
      }
    }
  ],
  "build": {
    "content": [
      {
        "files": ["**/*.md", "**/*.yml"],
        "exclude": ["_site/**"]
      }
    ],
    "resource": [
      {
        "files": ["images/**"]
      }
    ],
    "output": "_site",
    "template": ["default", "modern"],
    "globalMetadata": {
      "_appTitle": "AgentEval",
      "_appName": "AgentEval",
      "_appFooter": "AgentEval - .NET Agent Testing Framework",
      "_enableSearch": true
    }
  }
}
```

**Key Settings:**
- `TargetFramework`: Set to net10.0 (latest supported)
- `dest`: Output folder for API metadata (api/)
- `output`: HTML output folder (_site/)
- `template`: Using default and modern templates
- Search is enabled for documentation

### AgentEval.csproj
Location: `src/AgentEval/AgentEval.csproj`

**Documentation Settings:**
```xml
<!-- XML Documentation -->
<GenerateDocumentationFile>true</GenerateDocumentationFile>
<NoWarn>$(NoWarn);CS1591</NoWarn>
```

- `GenerateDocumentationFile`: Enables XML documentation generation
- `NoWarn`: Suppresses CS1591 (missing XML comment) warnings

---

## Advanced Options

### Rebuild from Scratch
```bash
# Clean build artifacts
dotnet clean src\AgentEval\AgentEval.csproj

# Remove old documentation
rmdir /s docs\api
rmdir /s docs\_site

# Rebuild everything
dotnet build src\AgentEval\AgentEval.csproj --configuration Release
cd docs
docfx metadata
docfx build
cd ..
```

### Generate Documentation for Specific Framework
Modify `docs/docfx.json`:
```json
"properties": {
  "TargetFramework": "net9.0"  // Change to desired framework
}
```

### Disable Documentation Warnings
In `src/AgentEval/AgentEval.csproj`, modify:
```xml
<NoWarn>$(NoWarn);CS1591;CA1018</NoWarn>
```

---

## Next Steps

1. **Review Generated Documentation**: Open `docs\_site\index.html` in a web browser
2. **Verify API References**: Check that all public APIs are documented
3. **Add Manual Documentation**: Create .md files in the docs folder
4. **Deploy**: Host the _site folder on a web server or GitHub Pages

---

## Additional Resources

- [DocFX Documentation](https://dotnet.github.io/docfx/)
- [.NET XML Documentation Comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/)
- [Markdown Guide](https://www.markdownguide.org/)

---

Generated: $(date)
