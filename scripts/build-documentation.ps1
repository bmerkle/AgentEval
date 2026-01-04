# AgentEval Documentation Build Script (PowerShell)
# This script performs all documentation generation steps

$ErrorActionPreference = "Stop"

Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "AgentEval Documentation Build Script" -ForegroundColor Cyan
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host ""

try {
    # Step 1: Build the project in Release configuration
    Write-Host "[Step 1] Building AgentEval project in Release configuration..." -ForegroundColor Green
    Write-Host "Command: dotnet build src\AgentEval\AgentEval.csproj --configuration Release"
    Write-Host ""
    
    dotnet build src\AgentEval\AgentEval.csproj --configuration Release
    
    Write-Host ""
    Write-Host "[✓] Build completed successfully" -ForegroundColor Green
    Write-Host ""
    
    # Step 2: Check for XML documentation files
    Write-Host "[Step 2] Checking for XML documentation files in bin folders..." -ForegroundColor Green
    Write-Host ""
    
    $xmlFiles = @(Get-ChildItem -Path "src\AgentEval\bin\Release" -Include "*.xml" -Recurse -ErrorAction SilentlyContinue)
    
    if ($xmlFiles.Count -eq 0) {
        Write-Host "WARNING: No XML documentation files found in bin\Release folders" -ForegroundColor Yellow
    } else {
        foreach ($file in $xmlFiles) {
            Write-Host "Found: $($file.FullName)"
        }
        Write-Host "[✓] XML documentation files found" -ForegroundColor Green
    }
    Write-Host ""
    
    # Step 3: Install docfx tool globally (if not already installed)
    Write-Host "[Step 3] Checking and installing docfx tool globally..." -ForegroundColor Green
    
    $docfxInstalled = $false
    try {
        $toolList = dotnet tool list -g 2>$null
        if ($toolList -like "*docfx*") {
            $docfxInstalled = $true
        }
    } catch {
        $docfxInstalled = $false
    }
    
    if (-not $docfxInstalled) {
        Write-Host "docfx not found, installing..."
        dotnet tool install -g docfx
    } else {
        Write-Host "[✓] docfx is already installed" -ForegroundColor Green
    }
    Write-Host ""
    
    # Step 4: Navigate to docs folder and run docfx metadata
    Write-Host "[Step 4] Generating API metadata with docfx..." -ForegroundColor Green
    Write-Host "Command: docfx metadata"
    Write-Host ""
    
    Push-Location "docs"
    docfx metadata
    Pop-Location
    
    Write-Host "[✓] API metadata generation completed" -ForegroundColor Green
    Write-Host ""
    
    # Step 5: Check the generated API YAML files
    Write-Host "[Step 5] Checking generated API YAML files..." -ForegroundColor Green
    Write-Host ""
    
    if (Test-Path "docs\api") {
        $ymlFiles = @(Get-ChildItem -Path "docs\api" -Include "*.yml" -Recurse -ErrorAction SilentlyContinue)
        
        if ($ymlFiles.Count -eq 0) {
            Write-Host "WARNING: No YAML files found in docs\api folder" -ForegroundColor Yellow
        } else {
            foreach ($file in $ymlFiles) {
                Write-Host "Found: $($file.FullName)"
            }
            Write-Host "[✓] API YAML files generated successfully" -ForegroundColor Green
        }
    } else {
        Write-Host "WARNING: docs\api folder not found" -ForegroundColor Yellow
    }
    Write-Host ""
    
    # Step 6: Build HTML documentation
    Write-Host "[Step 6] Building HTML documentation with docfx..." -ForegroundColor Green
    Write-Host "Command: docfx build"
    Write-Host ""
    
    Push-Location "docs"
    docfx build
    Pop-Location
    
    Write-Host "[✓] HTML documentation build completed" -ForegroundColor Green
    Write-Host ""
    
    # Step 7: Summary
    Write-Host "================================================================" -ForegroundColor Cyan
    Write-Host "[✓] Documentation generation completed successfully!" -ForegroundColor Green
    Write-Host "================================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Generated files:" -ForegroundColor Yellow
    Write-Host "- XML documentation: src\AgentEval\bin\Release\*\AgentEval.xml"
    Write-Host "- API metadata YAML: docs\api\"
    Write-Host "- HTML documentation: docs\_site\"
    Write-Host ""
    Write-Host "To view the documentation:" -ForegroundColor Yellow
    Write-Host "  Navigate to: docs\_site\index.html in your browser"
    Write-Host ""
}
catch {
    Write-Host ""
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    exit 1
}
