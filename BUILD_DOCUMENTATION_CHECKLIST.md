# AgentEval Documentation Build - Execution Checklist

## Pre-Build Verification ✓

- [x] Project file exists: `src\AgentEval\AgentEval.csproj`
- [x] XML documentation enabled: `<GenerateDocumentationFile>true</GenerateDocumentationFile>`
- [x] docfx.json configuration exists: `docs\docfx.json`
- [x] Target frameworks configured: net8.0, net9.0, net10.0
- [x] Build automation scripts created

---

## Build Process Steps

### STEP 1: Build Release Configuration
```
Command: dotnet build src\AgentEval\AgentEval.csproj --configuration Release
Expected: Build succeeds with 0 errors
Runtime: ~30-60 seconds
Status: ⏳ Ready to execute
```

**Verification after execution:**
- [ ] Exit code = 0
- [ ] No compiler errors
- [ ] Output shows "Build succeeded"

**Expected XML output paths:**
- [ ] `src\AgentEval\bin\Release\net8.0\AgentEval.xml`
- [ ] `src\AgentEval\bin\Release\net9.0\AgentEval.xml`
- [ ] `src\AgentEval\bin\Release\net10.0\AgentEval.xml`

---

### STEP 2: Verify XML Documentation Files
```
Command: Get-ChildItem -Path "src\AgentEval\bin\Release" -Include "*.xml" -Recurse
Expected: Lists .xml files in Release output
Status: ⏳ Ready to execute after Step 1
```

**Verification checklist:**
- [ ] At least 3 .xml files found (one per framework)
- [ ] Each file size > 0 KB
- [ ] Files are readable XML format

**Sample expected output:**
```
src\AgentEval\bin\Release\net8.0\AgentEval.xml
src\AgentEval\bin\Release\net9.0\AgentEval.xml
src\AgentEval\bin\Release\net10.0\AgentEval.xml
```

---

### STEP 3: Install DocFX Tool
```
Command: dotnet tool install -g docfx
Expected: docfx installed globally (or already present)
Runtime: ~30-120 seconds (first time only)
Status: ⏳ Ready to execute
```

**Verification checklist:**
- [ ] Installation succeeds or tool already installed
- [ ] `dotnet tool list -g | findstr docfx` shows docfx
- [ ] `docfx --version` returns version info

**Success message:**
```
Tool 'docfx' (version x.x.x) was successfully installed.
```

---

### STEP 4: Generate API Metadata
```
Command: cd docs && docfx metadata && cd ..
Expected: YAML files generated in docs\api
Runtime: ~10-30 seconds
Status: ⏳ Ready to execute after Step 3
```

**What happens:**
1. Reads all .csproj files from ../src
2. Extracts XML documentation from compiled assemblies
3. Generates YAML metadata files
4. Creates table of contents

**Verification checklist:**
- [ ] Command completes without errors
- [ ] Output shows "Build succeeded"
- [ ] docs\api folder is created/populated

**Expected output message:**
```
Build succeeded with 0 warning(s).
```

---

### STEP 5: Verify Generated YAML Files
```
Command: Get-ChildItem -Path "docs\api" -Include "*.yml" -Recurse | Sort-Object Name
Expected: Lists all generated .yml files
Status: ⏳ Ready to execute after Step 4
```

**Expected files to find:**
- [ ] `docs\api\toc.yml` (main table of contents)
- [ ] `docs\api\AgentEval.yml` (root namespace)
- [ ] `docs\api\AgentEval.Adapters.yml`
- [ ] `docs\api\AgentEval.Assertions.yml`
- [ ] `docs\api\AgentEval.Benchmarks.yml`
- [ ] `docs\api\AgentEval.Core.yml`
- [ ] `docs\api\AgentEval.Embeddings.yml`
- [ ] `docs\api\AgentEval.MAF.yml`
- [ ] `docs\api\AgentEval.Metrics.yml`
- [ ] `docs\api\AgentEval.Models.yml`
- [ ] `docs\api\AgentEval.Testing.yml`

**Sample structure of generated files:**
```
docs/api/
├── toc.yml                    (Main TOC - references all namespaces)
├── AgentEval.yml             (Root namespace info)
├── AgentEval.Adapters.yml    (Adapters namespace)
├── AgentEval.Assertions.yml  (Assertions namespace)
├── AgentEval.Core.yml        (Core namespace)
├── AgentEval.MAF.yml         (MAF integration)
├── AgentEval.Metrics.yml     (Metrics namespace)
├── AgentEval.Models.yml      (Models namespace)
└── AgentEval.Testing.yml     (Testing namespace)
```

---

### STEP 6: Build HTML Documentation
```
Command: cd docs && docfx build && cd ..
Expected: Complete HTML site generated in docs\_site
Runtime: ~20-60 seconds
Status: ⏳ Ready to execute after Step 5
```

**What happens:**
1. Processes all .md and .yml files
2. Applies templates and styling (default + modern)
3. Generates searchable HTML pages
4. Creates static website in _site folder
5. Includes search functionality

**Verification checklist:**
- [ ] Command completes without critical errors
- [ ] Output shows "Build succeeded"
- [ ] docs\_site folder is created/populated

**Expected output message:**
```
Build succeeded with X warning(s).
```

**Generated site structure:**
```
docs/_site/
├── index.html                 (Main page)
├── api/
│   ├── index.html            (API index)
│   ├── AgentEval.html
│   ├── AgentEval.*.html      (Individual namespace pages)
│   └── *.html                (Individual type pages)
├── articles/                  (Manual documentation)
├── search.html                (Search page)
├── search-index.json          (Search data)
└── (CSS, JS, images, etc)
```

---

### STEP 7: Verification & Error Reporting

```
Status: ⏳ Ready to execute after Step 6
```

**Success Indicators - All should be TRUE:**

- [ ] Build step completed without errors
- [ ] XML files exist in src\AgentEval\bin\Release\*
- [ ] docfx installed successfully
- [ ] API YAML files generated in docs\api\
- [ ] HTML documentation generated in docs\_site\
- [ ] Main index.html file exists and is valid
- [ ] No critical errors in docfx build output

**Error Categories to Check For:**

| Error Type | Location | Severity | Action |
|------------|----------|----------|--------|
| Compilation errors | Build output | CRITICAL | Fix source code, rebuild |
| Missing XML files | src\AgentEval\bin\Release | CRITICAL | Check GenerateDocumentationFile setting |
| docfx not installed | Command output | CRITICAL | Run: dotnet tool install -g docfx |
| Missing YAML files | docs\api | CRITICAL | Check docfx metadata output |
| HTML build errors | docs build output | CRITICAL | Check docfx.json configuration |
| Documentation warnings | docfx output | WARNING | Non-blocking, document missing comments later |
| Broken links | docfx build output | WARNING | Can be fixed in documentation |

---

## Automated Execution Options

### Option A: PowerShell Script (Recommended)
```powershell
cd C:\git\joslat\AgentEval
.\build-documentation.ps1
```

**Advantages:**
- Color-coded output
- Better error handling
- Cross-platform compatible
- Detailed progress reporting

**Expected runtime:** 2-3 minutes

---

### Option B: Batch Script
```batch
cd C:\git\joslat\AgentEval
build-documentation.bat
```

**Advantages:**
- Native Windows compatibility
- No dependency on PowerShell version
- Simple execution

**Expected runtime:** 2-3 minutes

---

### Option C: Manual Step-by-Step
Execute each step individually as documented above

**Advantages:**
- Full control and visibility
- Can troubleshoot each step
- Easier to debug

**Expected runtime:** 3-5 minutes

---

## File Inventory After Successful Build

### Source Files (Already Exist)
- ✓ `src\AgentEval\AgentEval.csproj`
- ✓ `docs\docfx.json`
- ✓ `docs\index.md`
- ✓ `docs\toc.yml`

### Generated Files (Created by Build Process)

**Step 1 Output:**
- [ ] `src\AgentEval\bin\Release\net8.0\AgentEval.xml`
- [ ] `src\AgentEval\bin\Release\net8.0\AgentEval.dll`
- [ ] `src\AgentEval\bin\Release\net9.0\AgentEval.xml`
- [ ] `src\AgentEval\bin\Release\net9.0\AgentEval.dll`
- [ ] `src\AgentEval\bin\Release\net10.0\AgentEval.xml`
- [ ] `src\AgentEval\bin\Release\net10.0\AgentEval.dll`

**Step 4 Output:**
- [ ] `docs\api\toc.yml`
- [ ] `docs\api\AgentEval.*.yml` (multiple files)

**Step 6 Output:**
- [ ] `docs\_site\index.html`
- [ ] `docs\_site\api\*.html` (multiple files)
- [ ] `docs\_site\search.html`
- [ ] `docs\_site\search-index.json`
- [ ] `docs\_site\styles\*` (CSS files)
- [ ] `docs\_site\scripts\*` (JavaScript files)

### Utility Files (Created by This Process)
- ✓ `build-documentation.bat`
- ✓ `build-documentation.ps1`
- ✓ `DOCUMENTATION_BUILD_GUIDE.md`
- ✓ `BUILD_DOCUMENTATION_REPORT.md`
- ✓ `BUILD_DOCUMENTATION_CHECKLIST.md` (this file)

---

## Project Configuration Summary

### XML Documentation Settings
```xml
<GenerateDocumentationFile>true</GenerateDocumentationFile>
<NoWarn>$(NoWarn);CS1591</NoWarn>
```
- XML files generated: ✓ YES
- Missing comment warnings suppressed: ✓ YES

### Target Frameworks
- net8.0 ✓
- net9.0 ✓
- net10.0 ✓

### DocFX Configuration (docs\docfx.json)
- Metadata source: ../src (all .csproj files) ✓
- Target framework: net10.0 ✓
- Metadata destination: api/ ✓
- HTML output: _site/ ✓
- Templates: default + modern ✓
- Search enabled: ✓ YES

---

## Viewing the Documentation

After successful build:

```bash
# Open in default browser
start docs\_site\index.html

# Or manually navigate to:
file:///C:/git/joslat/AgentEval/docs/_site/index.html
```

**Documentation Contents:**
- Main index page with project overview
- API reference for all 10+ namespaces
- Class, interface, and method documentation
- XML documentation from source code
- Search functionality (searchable by type, member name, etc.)

---

## Troubleshooting Quick Reference

| Problem | Quick Fix |
|---------|-----------|
| "Build failed" | Check error message, fix code, retry |
| "No XML files" | Verify `GenerateDocumentationFile=true` in .csproj |
| "docfx: command not found" | Run: `dotnet tool install -g docfx` |
| "No YAML files generated" | Check Step 4 output, re-run metadata |
| "HTML build failed" | Check docfx.json syntax, verify paths |
| "docs\_site not created" | Check Step 6 output for errors |
| "Search not working" | Check that search-index.json was created |

---

## Success Criteria

Build is considered **SUCCESSFUL** when:

✓ All 7 steps complete without critical errors
✓ All XML files exist (3 files for 3 frameworks)
✓ All YAML files exist (11+ files in docs/api)
✓ All HTML files exist (docs/_site/index.html and api pages)
✓ No show-stopper errors in output
✓ Documentation is viewable in browser

Build is considered **INCOMPLETE** if:

✗ Any step fails with critical error
✗ Expected output files not found
✗ Show-stopper compilation errors present
✗ docfx build produces "Build failed" message

---

## Next Actions After Successful Build

1. **View Documentation**: Open `docs\_site\index.html` in browser
2. **Verify Content**: Check all namespaces are documented
3. **Test Search**: Search for types or methods
4. **Fix Issues**: Address any missing documentation
5. **Deploy**: Host on documentation server or GitHub Pages
6. **Publish**: Include in project release or CI/CD pipeline

---

## Important Notes

⚠️ **First Time Warning**: The first build may take longer (2-3 minutes) due to:
- Full compilation of all frameworks
- DocFX setup and tool verification
- Initial YAML generation
- Template processing

✓ **Subsequent Builds**: Will be faster if:
- Changes are minimal
- Only rebuilding specific files
- Using incremental builds

ℹ️ **Documentation Quality**: Depends on:
- Quality of XML comments in source code
- Completeness of implementation
- Manual documentation added to docs folder

---

## References

- Project: AgentEval v0.1.2-alpha
- Location: C:\git\joslat\AgentEval
- Documentation: DOCUMENTATION_BUILD_GUIDE.md
- Status Report: BUILD_DOCUMENTATION_REPORT.md

---

**READY TO BUILD** ✓

All prerequisites are in place. You can now execute the build using:

```powershell
.\build-documentation.ps1
```

or

```batch
build-documentation.bat
```
