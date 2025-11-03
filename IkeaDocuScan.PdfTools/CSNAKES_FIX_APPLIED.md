# CSnakes Fix Applied - Host Builder Pattern

## Problem

CSnakes wasn't generating the Python wrapper code because we were missing the proper initialization pattern.

## Root Cause

CSnakes requires using the **Microsoft.Extensions.Hosting** builder pattern to:
1. Register the Python environment as a service
2. Trigger code generation for Python modules
3. Provide access to Python modules through `IPythonEnvironment`

## Solution Applied

### 1. Added Host Builder Pattern

**File:** `PdfTextExtractor.cs`

Added lazy initialization using the host builder pattern from the CSnakes getting started guide:

```csharp
private static readonly Lazy<IPythonEnvironment> _pythonEnv = new(() =>
{
    var pythonHomePath = AppContext.BaseDirectory;

    var builder = Host.CreateApplicationBuilder();
    builder.Services
        .WithPython()
        .WithHome(pythonHomePath)
        .WithVirtualEnvironment(Path.Combine(pythonHomePath, ".venv"));

    var app = builder.Build();
    return app.Services.GetRequiredService<IPythonEnvironment>();
});

private static IPythonEnvironment PythonEnv => _pythonEnv.Value;
```

### 2. Updated Module Access Pattern

**Changed from:**
```csharp
using var module = PdfExtractor.Import();  // ❌ Incorrect
```

**To:**
```csharp
using var module = PythonEnv.PdfExtractor();  // ✅ Correct
```

CSnakes generates extension methods on `IPythonEnvironment` for each Python module.

### 3. Added Required Package

**File:** `IkeaDocuScan.PdfTools.csproj`

Added Microsoft.Extensions.Hosting:
```xml
<PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.0" />
```

## How It Works

### Step 1: Build Time (Source Generation)

When you build the project:

1. CSnakes analyzer finds `Python/pdf_extractor.py` (marked as `AdditionalFiles`)
2. Parses Python functions with type hints:
   - `extract_text_from_path(pdf_path: str) -> str`
   - `extract_text_from_bytes(pdf_bytes: bytes) -> str`
   - `get_pdf_info(pdf_path: str) -> dict`
   - `compare_pdf_text(pdf_path1: str, pdf_path2: str) -> dict`
3. Generates C# extension methods on `IPythonEnvironment`:
   - `env.PdfExtractor()` returns a module interface
   - Module has methods: `ExtractTextFromPath()`, `ExtractTextFromBytes()`, etc.

### Step 2: Runtime (First Call)

On first use of `PdfTextExtractor`:

1. Lazy initializer creates the host and configures Python:
   - Sets Python home to application directory
   - Creates virtual environment in `.venv` folder
   - Installs dependencies from `requirements.txt` (PyPDF2)
2. Returns configured `IPythonEnvironment`
3. Subsequent calls reuse the same environment (thread-safe via `Lazy<T>`)

### Step 3: Method Calls

When calling `ExtractText()`:

1. Get Python environment: `PythonEnv.PdfExtractor()`
2. Call generated method: `module.ExtractTextFromPath(filePath)`
3. CSnakes marshals C# string → Python str → back to C# string
4. Module is disposed after use (`using var`)

## Python File Requirements

For CSnakes to generate code, Python functions **must** have type hints:

✅ **Correct:**
```python
def extract_text_from_path(pdf_path: str) -> str:
    ...
```

❌ **Won't generate:**
```python
def extract_text_from_path(pdf_path):  # No type hints
    ...
```

## Virtual Environment

On first run, CSnakes will:
1. Create `.venv/` folder in the application directory
2. Install PyPDF2 from `requirements.txt`
3. Cache the environment for future runs

**Location:** `bin/Debug/net9.0/.venv/` (or `bin/Release/net9.0/.venv/`)

## Testing the Fix

### Step 1: Restore and Build

```bash
dotnet restore
dotnet clean
dotnet build
```

### Step 2: Check for Generated Code

```powershell
# Look for generated files
Get-ChildItem -Path "obj" -Recurse -Filter "*.g.cs" | Select-Object FullName
```

Should show files like:
- `PdfExtractor.g.cs`
- Extension methods for `IPythonEnvironment`

### Step 3: Run a Test

```bash
dotnet test
```

The test should now:
- ✅ Compile without "PdfExtractor does not exist" errors
- ✅ Initialize Python environment on first run (takes 1-2 minutes)
- ✅ Run PDF extraction (if test PDFs are present)

## Expected Behavior

### First Run
```
Building...
CSnakes: Analyzing pdf_extractor.py
CSnakes: Generating extension methods for IPythonEnvironment
CSnakes: Generated PdfExtractor module wrapper
Build succeeded.

Running test...
[First test execution - may take 1-2 minutes]
- Creating Python virtual environment
- Installing PyPDF2==3.0.1
- Importing pdf_extractor module
Test: PASS (or SKIP if no PDF file)
```

### Subsequent Runs
```
Building...
Build succeeded.

Running test...
[Fast execution - environment already exists]
Test: PASS
```

## Troubleshooting

### Still Getting "PdfExtractor does not exist"?

Check that generated files exist:
```bash
dir obj\Debug\net9.0\generated\CSnakes.Runtime /s
```

If no files, check build output for CSnakes errors.

### Python Environment Issues?

Delete the virtual environment and rebuild:
```bash
Remove-Item -Recurse -Force bin\Debug\net9.0\.venv
Remove-Item -Recurse -Force bin\Release\net9.0\.venv
dotnet clean
dotnet build
```

### Type Hint Errors?

Ensure all Python functions have proper type hints. CSnakes only generates code for functions with complete type annotations.

## Reference

This fix is based on the CSnakes getting started example:

```csharp
// From CSnakes docs
var builder = Host.CreateApplicationBuilder(args);
builder.Services
    .WithPython()
    .WithHome(pythonHomePath)
    .WithVirtualEnvironment(Path.Combine(pythonHomePath, ".venv"));

var app = builder.Build();
var env = app.Services.GetRequiredService<IPythonEnvironment>();

// Call Python module
var result = env.ModuleName().FunctionName();
```

---

**Last Updated:** 2025-01-27
**Status:** Ready to test
