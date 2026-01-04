@echo off
REM Build AgentEval project and generate documentation
REM This script performs all documentation generation steps

setlocal enabledelayedexpansion

echo ================================================================
echo AgentEval Documentation Build Script
echo ================================================================
echo.

REM Step 1: Build the project in Release configuration
echo [Step 1] Building AgentEval project in Release configuration...
echo Command: dotnet build src\AgentEval\AgentEval.csproj --configuration Release
echo.
dotnet build src\AgentEval\AgentEval.csproj --configuration Release
if %errorlevel% neq 0 (
    echo.
    echo ERROR: Build failed with exit code %errorlevel%
    exit /b 1
)
echo.
echo [✓] Build completed successfully
echo.

REM Step 2: Check for XML documentation files
echo [Step 2] Checking for XML documentation files in bin folders...
echo.
set "found_docs=0"
for /r "src\AgentEval\bin\Release" %%f in (*.xml) do (
    echo Found: %%f
    set "found_docs=1"
)
if "!found_docs!"=="0" (
    echo WARNING: No XML documentation files found in bin\Release folders
) else (
    echo [✓] XML documentation files found
)
echo.

REM Step 3: Install docfx tool globally (if not already installed)
echo [Step 3] Checking and installing docfx tool globally...
echo Command: dotnet tool list -g ^| findstr docfx
dotnet tool list -g 2>nul | findstr docfx >nul 2>&1
if %errorlevel% neq 0 (
    echo docfx not found, installing...
    dotnet tool install -g docfx
    if %errorlevel% neq 0 (
        echo ERROR: Failed to install docfx
        exit /b 1
    )
) else (
    echo [✓] docfx is already installed
)
echo.

REM Step 4: Navigate to docs folder and run docfx metadata
echo [Step 4] Generating API metadata with docfx...
echo Command: docfx metadata
cd docs
docfx metadata
if %errorlevel% neq 0 (
    echo ERROR: docfx metadata generation failed with exit code %errorlevel%
    cd ..
    exit /b 1
)
cd ..
echo [✓] API metadata generation completed
echo.

REM Step 5: Check the generated API YAML files
echo [Step 5] Checking generated API YAML files...
echo.
set "found_yaml=0"
if exist "docs\api" (
    for /r "docs\api" %%f in (*.yml) do (
        echo Found: %%f
        set "found_yaml=1"
    )
    if "!found_yaml!"=="0" (
        echo WARNING: No YAML files found in docs\api folder
    ) else (
        echo [✓] API YAML files generated successfully
    )
) else (
    echo WARNING: docs\api folder not found
)
echo.

REM Step 6: Build HTML documentation
echo [Step 6] Building HTML documentation with docfx...
echo Command: docfx build
cd docs
docfx build
if %errorlevel% neq 0 (
    echo ERROR: docfx build failed with exit code %errorlevel%
    cd ..
    exit /b 1
)
cd ..
echo [✓] HTML documentation build completed
echo.

REM Step 7: Summary
echo ================================================================
echo [✓] Documentation generation completed successfully!
echo ================================================================
echo.
echo Generated files:
echo - XML documentation: src\AgentEval\bin\Release\*\AgentEval.xml
echo - API metadata YAML: docs\api\
echo - HTML documentation: docs\_site\
echo.
echo To view the documentation:
echo   Navigate to: docs\_site\index.html in your browser
echo.
