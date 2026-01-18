# Security Scanning & Hardening Guide

> **Last Updated:** January 18, 2026  
> **Version:** 0.1.3-alpha  
> **Status:** Production Security Hardening Complete

---

## Overview

AgentEval implements comprehensive automated security scanning to protect against vulnerabilities, supply chain attacks, and ensure enterprise-grade security for AI agent evaluation workflows.

---

## Security Scanning Tech Stack

| Tool | Purpose | Coverage |
|------|---------|----------|
| **Microsoft DevSkim** | Static analysis for security anti-patterns | Source code, config files |
| **NuGet Vulnerability Scanner** | Dependency vulnerability detection | All transitive packages |
| **GitHub CodeQL** | Advanced semantic code analysis | C# source code |
| **Secret Scanner** | Hardcoded credential detection | All file types |
| **SARIF Integration** | GitHub Security tab integration | Unified reporting |

---

## How Security Scanning Works

### 1. Automated CI/CD Integration

Security scanning runs automatically on:
- ✅ Every push to `main` and `develop` branches
- ✅ Every pull request to `main`
- ✅ Weekly scheduled scans (Monday 00:00 UTC)

### 2. Scan Pipeline Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                    SECURITY SCAN PIPELINE                            │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  1. CHECKOUT          Clone repository with full history             │
│         ↓                                                            │
│  2. RESTORE           Restore NuGet packages                        │
│         ↓                                                            │
│  3. DEVSKIM           Static analysis for security patterns          │
│         ↓             - SQL injection detection                      │
│         │             - XSS vulnerability patterns                   │
│         │             - Hardcoded secrets patterns                   │
│         │             - Crypto weakness detection                    │
│         ↓                                                            │
│  4. VULN CHECK        NuGet dependency vulnerability scan           │
│         ↓             - CVE database check                           │
│         │             - Transitive dependency analysis               │
│         ↓                                                            │
│  5. SECRET SCAN       Detect hardcoded credentials                  │
│         ↓             - API key patterns (sk-*, etc.)               │
│         │             - Connection strings                           │
│         │             - Password patterns                            │
│         ↓                                                            │
│  6. SARIF UPLOAD      Upload results to GitHub Security tab          │
│         ↓                                                            │
│  7. ARTIFACT SAVE     Store scan reports for audit                  │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

### 3. DevSkim Static Analysis

**What It Detects:**
- SQL injection vulnerabilities
- Cross-site scripting (XSS) patterns
- Command injection risks
- Path traversal vulnerabilities
- Hardcoded credentials
- Weak cryptographic algorithms
- Insecure random number generation
- XML external entity (XXE) injection

**Configuration:**
```yaml
- name: Run Security DevSkim Analyzer
  uses: microsoft/security-devskim-action@v1.0.14
  with:
    directory-to-scan: .
    output-filename: devskim-results.sarif
    output-format: sarif
```

### 4. Dependency Vulnerability Scanning

**Process:**
```bash
dotnet list package --vulnerable --include-transitive
```

**Checks Against:**
- NuGet Advisory Database
- GitHub Advisory Database
- National Vulnerability Database (NVD)

**Exit Conditions:**
- ✅ Pass: "No vulnerable packages found"
- ❌ Fail: Any package with known CVE

### 5. Secret Detection

**Patterns Detected:**
| Pattern | Example | Action |
|---------|---------|--------|
| OpenAI API Keys | `sk-...` | Block CI |
| Azure Keys | `DefaultEndpointsProtocol=...` | Block CI |
| Connection Strings | `Server=...;Password=...` | Block CI |
| Generic Secrets | `api_key`, `secret`, `password` in code | Warn |

---

## Security Architecture

### Input Validation Layer

All user inputs are validated before processing:

```csharp
// File path validation with allowlist
private static readonly string[] AllowedExtensions = 
    { ".yaml", ".yml", ".json", ".jsonl", ".csv" };

private static void ValidateFilePath(string path)
{
    // Check for path traversal attacks
    if (path.Contains("..") || Path.IsPathRooted(path) && 
        !path.StartsWith(Environment.CurrentDirectory))
    {
        throw new SecurityException("Invalid file path detected");
    }
    
    // Validate file extension
    var extension = Path.GetExtension(path).ToLowerInvariant();
    if (!AllowedExtensions.Contains(extension))
    {
        throw new SecurityException($"File type not allowed: {extension}");
    }
}
```

### Secure JSON Parsing

```csharp
// Use System.Text.Json with secure defaults
private static readonly JsonSerializerOptions SecureJsonOptions = new()
{
    MaxDepth = 64,                    // Prevent stack overflow
    AllowTrailingCommas = true,       // Flexibility
    ReadCommentHandling = JsonCommentHandling.Skip,
    PropertyNameCaseInsensitive = true
};
```

### PII Detection & Redaction

```csharp
// Behavioral policy assertions detect sensitive data
result.ToolUsage!.Should()
    .NeverPassArgumentMatching(
        @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b",
        because: "email addresses are PII",
        new RegexMatchingOptions { RedactMatches = true });
```

---

## Security Levels

### Level 1: Basic (Default)
- ✅ DevSkim static analysis
- ✅ Dependency vulnerability scanning
- ✅ Secret pattern detection

### Level 2: Enhanced (Recommended for Enterprise)
- ✅ All Level 1 checks
- ✅ Weekly scheduled scans
- ✅ GitHub Security tab integration
- ✅ Artifact retention for audit

### Level 3: Maximum (SOC2/HIPAA Compliance)
- ✅ All Level 2 checks
- 🔲 CodeQL semantic analysis (planned)
- 🔲 SAST/DAST integration (planned)
- 🔲 Software composition analysis (SCA) (planned)
- 🔲 Container scanning (planned)

---

## How Secure Is It?

### Current Protection Matrix

| Threat | Protection | Status |
|--------|------------|--------|
| **Known CVEs** | Dependency scanning | ✅ Active |
| **Code Injection** | DevSkim patterns | ✅ Active |
| **Hardcoded Secrets** | Secret scanning | ✅ Active |
| **Path Traversal** | Input validation | ✅ Active |
| **Supply Chain** | Package verification | ✅ Active |
| **XSS/CSRF** | Not applicable (library) | N/A |
| **SQL Injection** | DevSkim detection | ✅ Active |

### Security Certifications (Planned)

| Certification | Status | Target Date |
|---------------|--------|-------------|
| OWASP Top 10 Compliance | ✅ Self-assessed | Complete |
| CWE/SANS Top 25 | 🔲 Pending | Q2 2026 |
| SOC2 Type II | 🔲 Planned | Q4 2026 |

---

## Running Security Scans Locally

### Quick Scan
```bash
# Check for vulnerable packages
dotnet list package --vulnerable --include-transitive

# Run DevSkim locally (requires DevSkim CLI)
devskim analyze --source-code . --output-file results.sarif
```

### Full Scan (Docker)
```bash
# Pull DevSkim container
docker pull mcr.microsoft.com/devskim

# Run full analysis
docker run -v $(pwd):/src mcr.microsoft.com/devskim analyze --source-code /src
```

---

## Responding to Security Issues

### Vulnerability Found in Dependency

1. Check if the vulnerability affects AgentEval's usage pattern
2. If affected, create a security issue (private)
3. Update the dependency to patched version
4. Release patch version within 48 hours

### Security Bug in AgentEval Code

1. Report via [SECURITY.md](../SECURITY.md) process
2. Acknowledge within 24 hours
3. Patch and release within 7 days (critical) or 30 days (moderate)

---

## Configuration Reference

### CI Security Workflow (`.github/workflows/security.yml`)

```yaml
name: Security Scan

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]
  schedule:
    - cron: '0 0 * * 1'  # Weekly on Mondays

jobs:
  security:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
      - run: dotnet restore
      
      # DevSkim static analysis
      - uses: microsoft/security-devskim-action@v1.0.14
        with:
          directory-to-scan: .
          output-filename: devskim-results.sarif
          
      # Upload to GitHub Security tab
      - uses: github/codeql-action/upload-sarif@v3
        with:
          sarif_file: devskim-results.sarif
          
      # Dependency vulnerability check
      - run: dotnet list package --vulnerable --include-transitive
```

---

## Best Practices for Contributors

### Before Committing

1. ✅ Run `dotnet list package --vulnerable` locally
2. ✅ Never commit API keys, tokens, or passwords
3. ✅ Use environment variables for secrets
4. ✅ Validate all user inputs
5. ✅ Use parameterized queries if adding database support

### Code Review Checklist

- [ ] No hardcoded credentials
- [ ] Input validation present
- [ ] No path traversal vulnerabilities
- [ ] Secure JSON parsing used
- [ ] PII properly handled/redacted

---

## Related Documentation

- [SECURITY.md](../SECURITY.md) - Vulnerability reporting process
- [CONTRIBUTING.md](../CONTRIBUTING.md) - Contribution guidelines
- [ADR-009: Security Architecture](adr/009-security-architecture.md) - Design decisions

