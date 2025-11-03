# CSnakes Code Generation Troubleshooting

This guide helps troubleshoot CSnakes source code generation issues.

## Problem: "PdfExtractor does not exist in the current context"

This error means CSnakes hasn't generated the Python wrapper code yet.

## Solution Steps

### Step 1: Restore NuGet Packages

```bash
cd src/IkeaDocuScan.PdfTools
dotnet restore
```

Verify both packages are installed:
- ✅ `CSnakes.Runtime` (1.2.1)
- ✅ `CSnakes.Runtime.SourceGeneration` (1.2.1)

### Step 2: Clean and Rebuild

```bash
dotnet clean
dotnet build -v detailed
```

The `-v detailed` flag shows detailed build output including source generator messages.

### Step 3: Check Build Output

Look for CSnakes messages in the build output:

**✅ Success looks like:**
```
CSnakes: Generating code for pdf_extractor.py
CSnakes: Generated PdfExtractor.g.cs
```

**❌ Problem looks like:**
```
No messages from CSnakes at all
```

### Step 4: Verify Python File Configuration

Check `IkeaDocuScan.PdfTools.csproj`:

```xml
<ItemGroup>
  <!-- MUST be AdditionalFiles -->
  <AdditionalFiles Include="Python\*.py" />
</ItemGroup>
```

### Step 5: Check Python File Exists

Verify the file exists:
```bash
ls Python/pdf_extractor.py
```

Should show: `Python/pdf_extractor.py`

### Step 6: Check for Generated Files

After a successful build, check for generated files:

```bash
# Windows PowerShell
Get-ChildItem -Recurse -Filter "*PdfExtractor*.g.cs"

# Or check the obj folder
dir obj\Debug\net9.0\generated\CSnakes.Runtime.SourceGeneration\
```

**Expected:** You should see generated C# files with names like:
- `PdfExtractor.g.cs`
- `Python.PdfExtractor.g.cs`

### Step 7: Enable Source Generator Diagnostics

Add to `.csproj`:

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

This forces generated files to be written to disk where you can inspect them.

### Step 8: Check Visual Studio

If using Visual Studio:

1. **Clean Solution**: Build → Clean Solution
2. **Close Visual Studio**
3. **Delete bin and obj folders** manually
4. **Reopen Visual Studio**
5. **Rebuild Solution**

Sometimes Visual Studio caches source generator output incorrectly.

## Common Issues

### Issue 1: Source Generator Not Running

**Symptom:** No CSnakes messages in build output

**Cause:** Source generator package not installed or configured incorrectly

**Fix:**
1. Ensure `CSnakes.Runtime.SourceGeneration` package is referenced
2. Ensure it has the correct attributes:
   ```xml
   <PackageReference Include="CSnakes.Runtime.SourceGeneration" Version="1.2.1">
     <PrivateAssets>all</PrivateAssets>
     <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
   </PackageReference>
   ```

### Issue 2: Python File Not Found

**Symptom:** CSnakes runs but doesn't generate code

**Cause:** Python file not marked as `AdditionalFiles`

**Fix:**
```xml
<ItemGroup>
  <AdditionalFiles Include="Python\*.py" />
</ItemGroup>
```

### Issue 3: Python Syntax Errors

**Symptom:** Build succeeds but no code generated

**Cause:** Python file has syntax errors that CSnakes can't parse

**Fix:**
1. Check Python file for syntax errors
2. Ensure all functions have type hints:
   ```python
   def extract_text_from_path(pdf_path: str) -> str:
       ...
   ```

### Issue 4: .NET SDK Version

**Symptom:** Build fails with cryptic errors

**Cause:** Using incompatible .NET SDK version

**Fix:**
```bash
dotnet --version
```

Should be ≥ 9.0. If not:
1. Download .NET 9 SDK
2. Update `global.json` (if it exists) or remove it

### Issue 5: Generated Class Name Mismatch

**Symptom:** PdfExtractor class not found

**Cause:** CSnakes generates a different class name

**Solution:**
1. Check generated files in `obj/` folder
2. Look for the actual generated class name
3. Update `PdfTextExtractor.cs` to use the correct name:
   ```csharp
   // If generated as "Python_PdfExtractor"
   using var module = Python_PdfExtractor.Import();

   // If generated as "pdf_extractor"
   using var module = pdf_extractor.Import();
   ```

## Verification Checklist

After fixing, verify:

- [ ] `dotnet restore` succeeds
- [ ] `dotnet build` shows CSnakes messages
- [ ] Generated files exist in `obj/` folder
- [ ] No compilation errors
- [ ] Tests can find the generated class

## Manual Verification

Create a simple test file `TestCSnakes.cs`:

```csharp
using CSnakes.Runtime;

namespace IkeaDocuScan.PdfTools.Tests;

public class TestCSnakes
{
    public void VerifyGeneration()
    {
        // This should compile if CSnakes generated code
        using var module = PdfExtractor.Import();

        // Test with a simple file
        var result = module.ExtractTextFromPath("test.pdf");
        Console.WriteLine($"Result: {result}");
    }
}
```

If this compiles, CSnakes is working!

## CSnakes Documentation

For more information:
- **Official Docs**: https://tonybaloney.github.io/CSnakes/
- **GitHub**: https://github.com/tonybaloney/CSnakes
- **NuGet**: https://www.nuget.org/packages/CSnakes.Runtime

## Alternative: Check What CSnakes Actually Generated

1. Add this to .csproj temporarily:
   ```xml
   <PropertyGroup>
     <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
   </PropertyGroup>
   ```

2. Build the project

3. Check: `obj/Debug/net9.0/generated/CSnakes.Runtime.SourceGeneration/`

4. Open the generated `.g.cs` files to see:
   - What class names were generated
   - What method names were created
   - How to call them

## Still Not Working?

If after all these steps CSnakes still isn't generating code, there may be a compatibility issue with CSnakes 1.2.1 and .NET 9.

**Workaround options:**

1. **Use IronPython or Python.NET** instead
2. **Call Python directly** via `Process.Start()`
3. **Wait for CSnakes update** for .NET 9 support
4. **Try CSnakes 1.3.x or later** (check NuGet for newer versions)

---

**Last Updated:** 2025-01-27
