# Build Instructions - IkeaDocuScan.PdfTools

## Quick Start

### Step 1: Navigate to Project Directory

```bash
cd C:\Users\markr\source\repos\markRegenasst2b\IkeaDocuScan-V3\IkeaDocuScan.PdfTools
```

### Step 2: Restore NuGet Packages

```bash
dotnet restore
```

**Expected output:**
```
Restored C:\Users\markr\source\repos\...\IkeaDocuScan.PdfTools.csproj (in X ms).
```

### Step 3: Clean Previous Build

```bash
dotnet clean
```

### Step 4: Build with Verbose Output

```bash
dotnet build -v detailed > build.log 2>&1
```

This saves the build output to `build.log` which you can inspect for CSnakes messages.

### Step 5: Check for Generated Files

```powershell
# Check if CSnakes generated code
Get-ChildItem -Path "src\IkeaDocuScan.PdfTools\obj" -Recurse -Filter "*.g.cs" | Select-Object FullName

# Or check the Generated folder
dir src\IkeaDocuScan.PdfTools\obj\Generated -Recurse
```

## What to Look For

### Success Indicators

✅ **Build succeeds** without errors about `PdfExtractor` not existing
✅ **Generated files** exist in `obj/Generated/CSnakes.Runtime/` or similar
✅ **build.log contains** messages from CSnakes source generator

### Failure Indicators

❌ **Error:** `CS0103: The name 'PdfExtractor' does not exist`
❌ **No generated files** in `obj/` folder
❌ **No CSnakes messages** in build.log

## If Build Fails

### Check 1: Verify Python File Exists

```bash
dir src\IkeaDocuScan.PdfTools\Python\pdf_extractor.py
```

Should show: `pdf_extractor.py`

### Check 2: Verify .csproj Configuration

Open `src/IkeaDocuScan.PdfTools/IkeaDocuScan.PdfTools.csproj` and verify:

```xml
<ItemGroup>
  <AdditionalFiles Include="Python\*.py" />
</ItemGroup>
```

### Check 3: Check build.log for Errors

```bash
notepad build.log
```

Search for:
- "CSnakes" - Should see messages from the generator
- "error" - Look for any errors related to source generation
- "pdf_extractor" - Should see it being processed

## Alternative: Manual Check of What CSnakes Should Generate

Based on `Python/pdf_extractor.py`, CSnakes should generate something like:

```csharp
namespace CSnakes.Runtime.Generated
{
    public static class PdfExtractor
    {
        public static IPdfExtractorModule Import() { ... }
    }

    public interface IPdfExtractorModule
    {
        string ExtractTextFromPath(string pdfPath);
        string ExtractTextFromBytes(byte[] pdfBytes);
        PyObject GetPdfInfo(string pdfPath);
        PyObject ComparePdfText(string pdfPath1, string pdfPath2);
    }
}
```

## If CSnakes Still Doesn't Generate Code

This might indicate a compatibility issue with CSnakes 1.2.1 and .NET 9 or Windows.

### Workaround Option 1: Use Older CSnakes Version

Try an older stable version that's known to work:

```xml
<PackageReference Include="CSnakes.Runtime" Version="1.0.17" />
```

### Workaround Option 2: Use Python.NET Instead

Replace CSnakes with Python.NET (pythonnet):

```xml
<PackageReference Include="Python.Runtime" Version="3.0.3" />
```

Then rewrite `PdfTextExtractor.cs` to use Python.NET API.

### Workaround Option 3: Direct Process Execution

Call Python directly as a subprocess:

```csharp
var psi = new ProcessStartInfo
{
    FileName = "python",
    Arguments = $"Python/pdf_extractor.py \"{filePath}\"",
    RedirectStandardOutput = true,
    UseShellExecute = false
};

using var process = Process.Start(psi);
string output = process.StandardOutput.ReadToEnd();
```

## Getting Help

1. **Check generated files location:**
   ```bash
   dir /s /b obj\*.g.cs
   ```

2. **Share build.log** if you need help debugging

3. **Check CSnakes version compatibility:**
   - CSnakes 1.2.1 supports .NET 8-9
   - Supports Python 3.9-3.13
   - Windows, macOS, Linux

## Next Steps After Successful Build

Once the build succeeds and code is generated:

1. **Open generated file** to see the actual class/method names
2. **Update PdfTextExtractor.cs** if names don't match
3. **Add PDF test files** to `tests/IkeaDocuScan.PdfTools.Tests/Data/`
4. **Run tests:** `dotnet test`

---

**Last Updated:** 2025-01-27
