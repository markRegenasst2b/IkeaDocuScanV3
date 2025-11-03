# Next Steps - CSnakes Code Generation

## What Was Fixed

### 1. Added Missing Source Generator Package ✅

**Updated:** `IkeaDocuScan.PdfTools.csproj`

Added the required CSnakes source generator package:
```xml
<PackageReference Include="CSnakes.Runtime.SourceGeneration" Version="1.2.1">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

### 2. Enabled Source Generator File Emission ✅

Added to help debug what CSnakes generates:
```xml
<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
<CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
```

### 3. Fixed PyObject Helper Methods ✅

Removed invalid dictionary access code (`Contains`, `[]` indexing) and simplified to only use `HasAttr` and `GetAttr`.

## What To Do Now

### Step 1: Restore Packages

```bash
cd src/IkeaDocuScan.PdfTools
dotnet restore
```

This will download the new `CSnakes.Runtime.SourceGeneration` package.

### Step 2: Clean and Rebuild

```bash
dotnet clean
dotnet build -v detailed
```

The `-v detailed` flag will show you CSnakes messages.

### Step 3: Check for Generated Code

After building, check:

```bash
# Windows
dir obj\Generated\CSnakes.Runtime.SourceGeneration\

# Should see generated .g.cs files
```

### Step 4: Inspect Generated Code

Open the generated files to see:
- What the generated class is called (might be `PdfExtractor`, `Python_PdfExtractor`, or `pdf_extractor`)
- What methods are available
- How to call them

### Step 5: Adjust PdfTextExtractor.cs If Needed

If the generated class has a different name, update the code:

**Current code:**
```csharp
using var module = PdfExtractor.Import();
```

**If generated as different name, change to:**
```csharp
using var module = ActualGeneratedClassName.Import();
```

## Expected Build Output

After running `dotnet build -v detailed`, you should see something like:

```
CSnakes: Processing Python file: pdf_extractor.py
CSnakes: Generating wrapper for module: pdf_extractor
CSnakes: Generated class: PdfExtractor
CSnakes: Generated methods:
  - ExtractTextFromPath(string) -> string
  - ExtractTextFromBytes(byte[]) -> string
  - GetPdfInfo(string) -> PyObject
  - ComparePdfText(string, string) -> PyObject
```

## If It Still Doesn't Work

See `CSNAKES_TROUBLESHOOTING.md` for detailed troubleshooting steps.

## Verification

After a successful build, these errors should be gone:
- ❌ ~~CS0103: 'PdfExtractor' does not exist~~
- ❌ ~~CS1929: 'PyObject' does not contain 'Contains'~~
- ❌ ~~CS0021: Cannot apply indexing with []~~

And the test should compile:
```bash
dotnet build  # Should succeed
dotnet test   # Should run (tests may fail if no PDF files yet)
```

---

**Last Updated:** 2025-01-27
