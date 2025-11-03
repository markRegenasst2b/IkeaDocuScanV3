# IkeaDocuScan.PdfTools - Setup Guide

This guide provides detailed instructions for setting up, building, and deploying the PdfTools library.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Initial Build](#initial-build)
3. [Understanding CSnakes](#understanding-csnakes)
4. [Python Environment](#python-environment)
5. [Integration with IkeaDocuScan](#integration-with-ikeadocuscan)
6. [Deployment](#deployment)
7. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Software

- **.NET 9.0 SDK** or later (compatible with .NET 10)
  - Download: https://dotnet.microsoft.com/download
  - Verify: `dotnet --version`

- **Visual Studio 2022** (version 17.8 or later) OR **Visual Studio Code**
  - Visual Studio 2022: Recommended for full IDE experience
  - VS Code: Requires C# Dev Kit extension

- **Windows Server** (production environment)
  - Windows Server 2019 or later recommended

### Optional Software

- **Git** (for version control)
- **NuGet CLI** (for package management)

### No Python Required

One of the key benefits of CSnakes is that you **do not need Python installed** on your development or production machines. Python runtime is embedded within the .NET application.

---

## Initial Build

### Step 1: Open Solution

```powershell
# Navigate to the solution directory
cd IkeaDocuScan.PdfTools

# Open in Visual Studio
start IkeaDocuScan.PdfTools.sln

# OR open in VS Code
code .
```

### Step 2: Restore NuGet Packages

```powershell
# Restore all NuGet dependencies
dotnet restore
```

This will download:
- CSnakes.Runtime (version 1.2.1)
- All dependencies

### Step 3: Build the Project

```powershell
# Build in Debug mode
dotnet build

# OR build in Release mode
dotnet build --configuration Release
```

### Step 4: Verify Build Output

After a successful build, check the output directory:

```
src/IkeaDocuScan.PdfTools/bin/Debug/net9.0/
├── IkeaDocuScan.PdfTools.dll      # Main library
├── IkeaDocuScan.PdfTools.xml      # XML documentation
├── CSnakes.Runtime.dll             # CSnakes runtime
├── Python/
│   ├── pdf_extractor.py            # Python module
│   └── requirements.txt            # Python dependencies
└── [other dependencies]
```

---

## Understanding CSnakes

### What is CSnakes?

CSnakes is a .NET Source Generator and Runtime that embeds Python code and libraries into C# applications without requiring external Python installation.

### How CSnakes Works

1. **Build Time (Source Generation)**
   - CSnakes analyzes Python files marked as `AdditionalFiles`
   - Generates C# wrapper classes based on Python function signatures
   - Uses Python type hints to create strongly-typed C# methods

2. **Runtime (First Execution)**
   - On first use, CSnakes initializes the embedded Python environment
   - Extracts Python runtime files
   - Creates a virtual environment
   - Installs packages from `requirements.txt` (PyPDF2)

3. **Subsequent Executions**
   - Python environment is already initialized
   - Fast startup and execution

### Generated Code

CSnakes generates wrapper code from `pdf_extractor.py`. After building, you'll see generated files (typically in `obj/` directory or as part of the compilation).

The generated code provides C# methods that correspond to your Python functions:
- `extract_text_from_path(str) -> str` becomes a C# method
- `extract_text_from_bytes(bytes) -> str` becomes a C# method
- `get_pdf_info(str) -> dict` becomes a C# method
- etc.

### Configuration in .csproj

Key configuration in `IkeaDocuScan.PdfTools.csproj`:

```xml
<!-- Python files marked as AdditionalFiles for code generation -->
<AdditionalFiles Include="Python\*.py" />

<!-- Python files copied to output directory -->
<None Include="Python\*.py" CopyToOutputDirectory="PreserveNewest" />
<None Include="Python\requirements.txt" CopyToOutputDirectory="PreserveNewest" />

<!-- CSnakes NuGet package -->
<PackageReference Include="CSnakes.Runtime" Version="1.2.1" />
```

---

## Python Environment

### First Run Initialization

On the first call to any PdfTools method, CSnakes performs one-time initialization:

1. **Python Runtime Extraction** (~30-60 seconds)
   - Extracts embedded Python runtime
   - Creates virtual environment in application directory

2. **Package Installation** (~10-30 seconds)
   - Reads `requirements.txt`
   - Installs PyPDF2 using pip

3. **Module Import** (~1-2 seconds)
   - Imports Python modules
   - Creates callable Python objects

**Total first-run overhead: 1-2 minutes**

### Subsequent Runs

After initialization, startup is fast:
- Environment check: <1 second
- Module import: <1 second

### Environment Location

By default, CSnakes creates the Python environment in:
```
[Application Directory]/.csnakes/
```

This directory contains:
- Python runtime files
- Virtual environment
- Installed packages (PyPDF2)

### Managing Python Environment

#### View Environment

```powershell
# Check if environment exists
dir .csnakes
```

#### Reset Environment

If you encounter Python-related issues, reset the environment:

```powershell
# Delete the environment directory
Remove-Item -Recurse -Force .csnakes

# Next run will recreate it
```

#### Update PyPDF2 Version

To use a different PyPDF2 version:

1. Edit `Python/requirements.txt`:
   ```
   PyPDF2==3.0.1  # Change version here
   ```

2. Delete environment and rebuild:
   ```powershell
   Remove-Item -Recurse -Force .csnakes
   dotnet build
   ```

---

## Integration with IkeaDocuScan

### Option 1: Project Reference

Add a project reference in your consuming application:

```xml
<ItemGroup>
  <ProjectReference Include="..\IkeaDocuScan.PdfTools\src\IkeaDocuScan.PdfTools\IkeaDocuScan.PdfTools.csproj" />
</ItemGroup>
```

### Option 2: Binary Reference

Copy built assemblies and reference them:

```xml
<ItemGroup>
  <Reference Include="IkeaDocuScan.PdfTools">
    <HintPath>..\..\lib\IkeaDocuScan.PdfTools.dll</HintPath>
  </Reference>
</ItemGroup>
```

**Important**: When using binary references, ensure you also copy:
- `CSnakes.Runtime.dll`
- `Python/` directory with all Python files

### Integration Example

In your IkeaDocuScan application:

```csharp
// In DocumentService.cs or similar
using IkeaDocuScan.PdfTools;
using IkeaDocuScan.PdfTools.Models;
using IkeaDocuScan.PdfTools.Exceptions;

public class DocumentService
{
    public async Task<string> ExtractPdfText(int documentId)
    {
        // Get PDF from database (varbinary(max))
        var document = await _dbContext.Documents
            .Include(d => d.DocumentFile)
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (document?.DocumentFile?.FileContent == null)
            throw new InvalidOperationException("Document not found or has no file content");

        try
        {
            // Extract text using PdfTools
            string text = await PdfTextExtractor.ExtractTextAsync(
                document.DocumentFile.FileContent
            );

            return text;
        }
        catch (PdfEncryptedException)
        {
            // Log and handle encrypted PDFs
            _logger.LogWarning("Document {DocumentId} is encrypted", documentId);
            throw;
        }
        catch (PdfToolsException ex)
        {
            // Log and handle other PDF errors
            _logger.LogError(ex, "Error extracting text from document {DocumentId}", documentId);
            throw;
        }
    }

    public async Task<List<Document>> SearchDocuments(string searchTerm)
    {
        var allPdfs = await _dbContext.Documents
            .Include(d => d.DocumentFile)
            .Where(d => d.DocumentFile != null)
            .ToListAsync();

        var matches = new List<Document>();

        foreach (var doc in allPdfs)
        {
            try
            {
                string text = await PdfTextExtractor.ExtractTextAsync(
                    doc.DocumentFile.FileContent
                );

                if (text.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    matches.Add(doc);
                }
            }
            catch (PdfToolsException ex)
            {
                _logger.LogWarning(ex, "Could not search document {DocumentId}", doc.Id);
                // Continue with other documents
            }
        }

        return matches;
    }
}
```

---

## Deployment

### Deployment to Windows Server

#### Step 1: Publish the Application

```powershell
# Publish in Release mode
dotnet publish --configuration Release --output ./publish

# Or with specific runtime
dotnet publish --configuration Release --runtime win-x64 --output ./publish
```

#### Step 2: Copy Files to Server

Copy the entire `publish/` directory to the server, ensuring:
- All `.dll` files
- `Python/` directory
- Configuration files

#### Step 3: First Run on Server

The first time the application runs on the server:
1. CSnakes will initialize Python environment (1-2 minutes)
2. Requires internet connection for PyPDF2 download
3. Requires write permissions in application directory

#### Step 4: Verify Installation

Create a test script to verify the library works:

```csharp
// TestPdfTools.cs
using IkeaDocuScan.PdfTools;

try
{
    Console.WriteLine("Testing PdfTools library...");

    // Test with a sample PDF
    string text = PdfTextExtractor.ExtractText(@"C:\TestFiles\sample.pdf");

    Console.WriteLine($"Success! Extracted {text.Length} characters");
    Console.WriteLine("First 200 characters:");
    Console.WriteLine(text.Substring(0, Math.Min(200, text.Length)));
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack: {ex.StackTrace}");
}
```

### IIS Deployment Considerations

If deploying to IIS:

1. **App Pool Identity**: Ensure the application pool identity has:
   - Write permissions to application directory (for Python environment)
   - Network access (for first-time PyPDF2 download)

2. **32-bit vs 64-bit**: Use 64-bit Python runtime (default)
   - Set "Enable 32-Bit Applications" to `False` in IIS App Pool

3. **Timeout Settings**: First run takes 1-2 minutes
   - Increase IIS startup timeout if needed

### Offline Deployment

For servers without internet access:

1. **Pre-initialize Environment** on a development machine:
   ```powershell
   # Run the application once to create .csnakes directory
   dotnet run --project TestApp
   ```

2. **Copy Environment** to deployment package:
   - Include `.csnakes/` directory in deployment
   - This contains Python runtime and installed packages

3. **Deploy** entire package to server

---

## Troubleshooting

### Build Errors

#### Error: "CSnakes.Runtime not found"

**Solution**:
```powershell
dotnet restore
dotnet clean
dotnet build
```

#### Error: "Python file not found during build"

**Solution**: Check `.csproj` configuration:
```xml
<AdditionalFiles Include="Python\*.py" />
<None Include="Python\*.py" CopyToOutputDirectory="PreserveNewest" />
```

### Runtime Errors

#### Error: "Python environment initialization failed"

**Causes**:
- No internet connection (first run)
- No write permissions
- Corrupted environment

**Solution**:
```powershell
# Delete environment and retry
Remove-Item -Recurse -Force .csnakes

# Verify write permissions
icacls . /grant "Users:(OI)(CI)F"

# Run application
dotnet run
```

#### Error: "Module 'PyPDF2' not found"

**Cause**: Python environment initialization incomplete

**Solution**:
```powershell
# Reset environment
Remove-Item -Recurse -Force .csnakes

# Verify requirements.txt is in output directory
dir bin\Debug\net9.0\Python\requirements.txt

# Rebuild
dotnet clean
dotnet build
```

#### Error: "Import error" on first run

**Cause**: Network issue during package installation

**Solution**:
- Check internet connection
- Check firewall settings
- Try offline deployment method

### Performance Issues

#### Slow first run

**Expected**: First run takes 1-2 minutes for Python environment setup.

**If excessive** (>5 minutes):
- Check network speed
- Verify no antivirus interference
- Check disk I/O performance

#### Slow subsequent runs

**Not expected**: Subsequent runs should be fast (<2 seconds startup)

**Solution**:
- Check if environment is being recreated each time
- Verify `.csnakes/` directory persists between runs
- Check application directory permissions

### Python-Related Issues

#### Want to use different Python version

CSnakes embeds a specific Python version. To use a different version, you would need to configure CSnakes accordingly (consult CSnakes documentation).

#### Want to add more Python packages

Edit `requirements.txt`:
```
PyPDF2==3.0.1
reportlab==4.0.0  # Add additional packages
pillow==10.1.0
```

Then reset environment:
```powershell
Remove-Item -Recurse -Force .csnakes
dotnet build
```

---

## Advanced Configuration

### Custom Python Environment Location

You can configure CSnakes to use a custom location for the Python environment (consult CSnakes documentation for details).

### Python Debug Mode

For troubleshooting Python issues, enable Python debug logging (consult CSnakes documentation).

### Performance Tuning

1. **Parallel Processing**: Process multiple PDFs concurrently
2. **Caching**: Cache extracted text to avoid reprocessing
3. **Async/Await**: Use async methods for I/O operations

---

## Getting Help

### Resources

- **CSnakes Documentation**: https://tonybaloney.github.io/CSnakes/
- **PyPDF2 Documentation**: https://pypdf2.readthedocs.io/
- **IkeaDocuScan Team**: Internal support

### Reporting Issues

When reporting issues, include:
1. Error message and stack trace
2. .NET version (`dotnet --version`)
3. Operating system and version
4. Steps to reproduce
5. CSnakes environment status (does `.csnakes/` exist?)

---

## Summary Checklist

- [ ] .NET 9.0 SDK installed
- [ ] Solution builds successfully
- [ ] Python environment initializes on first run
- [ ] Can extract text from sample PDF
- [ ] Deployment package includes Python files
- [ ] Application pool has correct permissions (IIS)
- [ ] Internet access available for first run OR offline deployment prepared

---

**Last Updated**: 2025-01-27
**Version**: 1.0.0
