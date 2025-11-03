# IkeaDocuScan.PdfTools - Project Structure

Complete overview of the project structure and file organization.

## Directory Tree

```
IkeaDocuScan.PdfTools/
├── .gitignore                          # Git ignore rules
├── IkeaDocuScan.PdfTools.sln          # Visual Studio solution file
├── README.md                           # Main documentation
├── SETUP_GUIDE.md                      # Detailed setup instructions
├── API_REFERENCE.md                    # Complete API documentation
├── CHANGELOG.md                        # Version history
├── PROJECT_STRUCTURE.md                # This file
│
├── src/
│   └── IkeaDocuScan.PdfTools/
│       ├── IkeaDocuScan.PdfTools.csproj   # Project file with CSnakes config
│       ├── GlobalUsings.cs                 # Global using directives
│       │
│       ├── Python/                         # Python modules (embedded)
│       │   ├── pdf_extractor.py            # PyPDF2 wrapper functions
│       │   └── requirements.txt            # Python dependencies (PyPDF2)
│       │
│       ├── PdfTextExtractor.cs             # Main public API class
│       │
│       ├── Models/                         # Data transfer objects
│       │   ├── PdfMetadata.cs              # PDF metadata DTO
│       │   └── PdfComparisonResult.cs      # Comparison result DTO
│       │
│       └── Exceptions/                     # Custom exceptions
│           └── PdfToolsException.cs        # Exception hierarchy
│
└── tests/
    └── IkeaDocuScan.PdfTools.Tests/
        ├── IkeaDocuScan.PdfTools.Tests.csproj  # Test project file
        ├── GlobalUsings.cs                      # Global using directives for tests
        ├── README.md                            # Test project documentation
        ├── PdfTextExtractorTests.cs            # Main test class
        ├── TestHelpers.cs                      # Test utility methods
        │
        └── Data/                               # Test PDF files directory
            ├── README.md                       # Test data documentation
            ├── .gitignore                      # Allow PDFs in this folder
            └── [PDF test files]                # User-provided test PDFs
```

## File Descriptions

### Root Level

#### .gitignore
Git ignore file configured for:
- Visual Studio files (`.vs/`, `*.user`, `*.suo`)
- Build outputs (`bin/`, `obj/`)
- Python virtual environments (`.venv/`, `__pycache__/`)
- CSnakes generated files
- NuGet packages

#### IkeaDocuScan.PdfTools.sln
Visual Studio solution file that contains the `IkeaDocuScan.PdfTools` project.

#### README.md
Main documentation file containing:
- Feature overview
- Installation instructions
- Usage examples (basic and advanced)
- Full-text search integration example
- Error handling patterns
- Performance considerations
- Troubleshooting guide

#### SETUP_GUIDE.md
Detailed setup and deployment guide with:
- Prerequisites and requirements
- Build instructions
- CSnakes configuration explanation
- Python environment setup
- IkeaDocuScan integration examples
- Deployment to Windows Server
- Comprehensive troubleshooting section

#### API_REFERENCE.md
Complete API documentation including:
- All public methods with signatures
- Parameter descriptions
- Return values
- Exception documentation
- Usage examples for each method
- Best practices

#### CHANGELOG.md
Version history and release notes:
- Current version: 1.0.0 (2025-01-27)
- Feature additions
- Known limitations
- Planned features

#### PROJECT_STRUCTURE.md
This file - overview of project organization.

---

### src/IkeaDocuScan.PdfTools/

#### IkeaDocuScan.PdfTools.csproj

Project file configured with:
- Target framework: `net9.0`
- NuGet package: `CSnakes.Runtime` version 1.2.1
- Python files marked as `AdditionalFiles` for code generation
- Python files and requirements copied to output directory
- XML documentation generation enabled

**Key Configuration:**
```xml
<PackageReference Include="CSnakes.Runtime" Version="1.2.1" />
<AdditionalFiles Include="Python\*.py" />
<None Include="Python\*.py" CopyToOutputDirectory="PreserveNewest" />
<None Include="Python\requirements.txt" CopyToOutputDirectory="PreserveNewest" />
```

#### GlobalUsings.cs

Global using directives for common namespaces:
- `System`
- `System.IO`
- `System.Threading`
- `System.Threading.Tasks`

Reduces boilerplate imports across the project.

---

### Python Directory

#### Python/pdf_extractor.py

Python module containing PDF processing functions:

**Functions:**
- `extract_text_from_path(pdf_path: str) -> str`
  - Extracts text from PDF file path
  - Handles file not found errors

- `extract_text_from_bytes(pdf_bytes: bytes) -> str`
  - Extracts text from byte array
  - Common for database scenarios

- `_extract_text_from_stream(stream) -> str`
  - Internal helper for stream-based extraction
  - Handles encrypted PDFs
  - Extracts text from all pages

- `get_pdf_info(pdf_path: str) -> dict`
  - Returns metadata dictionary
  - Includes page count, encryption status, metadata fields

- `compare_pdf_text(pdf_path1: str, pdf_path2: str) -> dict`
  - Compares two PDFs
  - Returns similarity metrics

**Error Handling:**
- Encrypted PDF detection
- Corrupted PDF handling
- Per-page error handling (continues on page errors)

#### Python/requirements.txt

Python dependencies:
```
PyPDF2==3.0.1
```

CSnakes installs these packages on first run.

---

### C# Source Files

#### PdfTextExtractor.cs

Main public API class providing:

**Text Extraction (Sync):**
- `ExtractText(string filePath)`
- `ExtractText(byte[] pdfBytes)`
- `ExtractText(Stream pdfStream)`

**Text Extraction (Async):**
- `ExtractTextAsync(string filePath, CancellationToken)`
- `ExtractTextAsync(byte[] pdfBytes, CancellationToken)`
- `ExtractTextAsync(Stream pdfStream, CancellationToken)`

**Metadata:**
- `GetMetadata(string filePath)`
- `GetMetadataAsync(string filePath, CancellationToken)`

**Comparison:**
- `Compare(string filePath1, string filePath2)`
- `CompareAsync(string filePath1, string filePath2, CancellationToken)`

**Features:**
- Full XML documentation
- Comprehensive error handling
- Input validation
- Exception translation from Python to .NET

**Current State:**
Contains placeholder implementations with `NotImplementedException`. After building with CSnakes, replace placeholders with actual generated code calls.

---

### Models Directory

#### Models/PdfMetadata.cs

DTO for PDF metadata information.

**Properties:**
- `NumberOfPages` (int): Page count
- `IsEncrypted` (bool): Encryption status
- `Title` (string?): Document title
- `Author` (string?): Document author
- `Subject` (string?): Document subject
- `Creator` (string?): Creator application
- `Producer` (string?): Producer application

Nullable string properties for optional metadata fields.

#### Models/PdfComparisonResult.cs

DTO for PDF comparison results.

**Properties:**
- `Text1Length` (int): Character count of first PDF
- `Text2Length` (int): Character count of second PDF
- `Text1Lines` (int): Line count of first PDF
- `Text2Lines` (int): Line count of second PDF
- `IsIdentical` (bool): True if texts are identical
- `LengthDifference` (int): Absolute character difference
- `SimilarityRatio` (double): Similarity from 0.0 to 1.0
- `SimilarityPercentage` (double): Computed percentage (0-100)

---

### Exceptions Directory

#### Exceptions/PdfToolsException.cs

Exception hierarchy for PDF processing errors.

**Classes:**

1. **PdfToolsException**
   - Base exception for all PDF operations
   - Inherits from `System.Exception`
   - Three constructors: default, message, message + inner exception

2. **PdfEncryptedException**
   - Thrown for encrypted/password-protected PDFs
   - Inherits from `PdfToolsException`
   - Default message: "PDF is encrypted and cannot be read without a password"

3. **PdfCorruptedException**
   - Thrown for corrupted or invalid PDFs
   - Inherits from `PdfToolsException`
   - Default message: "PDF file is corrupted or cannot be read"

**Exception Hierarchy:**
```
Exception
└── PdfToolsException
    ├── PdfEncryptedException
    └── PdfCorruptedException
```

---

## Build Output Structure

After building, the output directory contains:

```
bin/Debug/net9.0/
├── IkeaDocuScan.PdfTools.dll
├── IkeaDocuScan.PdfTools.xml        # XML documentation
├── IkeaDocuScan.PdfTools.pdb        # Debug symbols
├── CSnakes.Runtime.dll               # CSnakes runtime
├── [CSnakes dependencies]
└── Python/
    ├── pdf_extractor.py
    └── requirements.txt
```

On first run, CSnakes creates:
```
[ApplicationDirectory]/.csnakes/
├── [Python runtime files]
└── [Virtual environment with PyPDF2]
```

---

## Code Generation

### CSnakes Source Generation

During build, CSnakes:
1. Analyzes `pdf_extractor.py` (marked as `AdditionalFiles`)
2. Generates C# wrapper classes from Python functions
3. Uses Python type hints to create strongly-typed methods

### Generated Code Location

Generated files typically appear in:
- `obj/Debug/net9.0/generated/` (or similar)
- Compiled into the assembly
- Not committed to source control

### Using Generated Code

After build, update `PdfTextExtractor.cs` to call generated wrappers:

```csharp
// Before (placeholder)
throw new NotImplementedException("CSnakes code generation required...");

// After (with generated code)
return PdfExtractor.ExtractTextFromPath(filePath);
```

---

## Integration Points

### With IkeaDocuScan Solution

To integrate with the main IkeaDocuScan application:

1. **Add Project Reference:**
   ```xml
   <ProjectReference Include="..\..\IkeaDocuScan.PdfTools\src\IkeaDocuScan.PdfTools\IkeaDocuScan.PdfTools.csproj" />
   ```

2. **Use in Services:**
   ```csharp
   using IkeaDocuScan.PdfTools;

   public class DocumentService
   {
       public async Task<string> ExtractText(byte[] pdfBytes)
       {
           return await PdfTextExtractor.ExtractTextAsync(pdfBytes);
       }
   }
   ```

3. **Full-Text Search:**
   Implement search indexing using extracted text

4. **Document Comparison:**
   Use compare functionality for version tracking

---

## Dependencies

### NuGet Dependencies
- `CSnakes.Runtime` (1.2.1)

### Python Dependencies
- `PyPDF2` (3.0.1)

### Runtime Dependencies
- .NET 9.0 runtime (or .NET 10)
- Windows Server (tested platform)
- Internet connection (first run only, for PyPDF2 installation)
- Write permissions (for Python environment initialization)

---

## Size Estimates

### Source Code
- Total source files: ~15 files
- Lines of code: ~1,500 LOC
  - C#: ~1,200 LOC
  - Python: ~200 LOC
  - Documentation: ~2,000 lines

### Build Output
- Assembly size: ~50-100 KB (C# code only)
- With CSnakes: ~500 KB - 1 MB
- Python environment (after first run): ~50-100 MB
  - Python runtime: ~30 MB
  - PyPDF2 and dependencies: ~5 MB
  - Virtual environment overhead: ~15 MB

---

## Development Workflow

### 1. Modify Python Code
Edit `Python/pdf_extractor.py`

### 2. Update Type Hints
Ensure all functions have proper type hints for CSnakes

### 3. Rebuild
```powershell
dotnet build
```

### 4. Update C# Wrapper
Modify `PdfTextExtractor.cs` to use generated code

### 5. Test
Create unit tests or integration tests

### 6. Document
Update README, API_REFERENCE, or CHANGELOG as needed

---

## Testing Strategy

### Unit Tests (TODO)
- Test each extraction method
- Test error handling (encrypted, corrupted PDFs)
- Test metadata extraction
- Test comparison functionality
- Mock CSnakes generated code for testing

### Integration Tests (TODO)
- Test with real PDF files
- Test with various PDF types (text, images, mixed)
- Test large file handling
- Test async operations with cancellation

### Performance Tests (TODO)
- Benchmark extraction speed
- Test memory usage with large files
- Test concurrent operations

---

## Maintenance

### Updating PyPDF2

1. Edit `Python/requirements.txt`
2. Delete `.csnakes/` directory
3. Rebuild project
4. Test thoroughly

### Updating CSnakes

1. Update package version in `.csproj`
2. Restore NuGet packages
3. Rebuild and verify generated code
4. Test all functionality

### Adding New Features

1. Add Python function to `pdf_extractor.py`
2. Add C# wrapper method to `PdfTextExtractor.cs`
3. Update documentation (README, API_REFERENCE)
4. Add usage examples
5. Update CHANGELOG

---

## Future Enhancements

### Planned Structure Changes

When adding new features, consider:

1. **OCR Support:**
   - New Python file: `ocr_extractor.py`
   - New C# class: `PdfOcrExtractor.cs`

2. **Advanced Diff:**
   - New Python file: `pdf_diff.py`
   - New model: `PdfDetailedDiff.cs`

3. **Table Extraction:**
   - New Python file: `table_extractor.py`
   - New model: `PdfTable.cs`

---

## Contributing Guidelines

### Code Style
- Follow C# coding conventions
- Use XML documentation for all public APIs
- Follow PEP 8 for Python code
- Keep functions focused and single-purpose

### Documentation
- Update README for user-facing changes
- Update API_REFERENCE for API changes
- Update CHANGELOG for all changes
- Include code examples

### Version Control
- Clear commit messages
- Branch for features/fixes
- Update version numbers appropriately

---

**Last Updated:** 2025-01-27
**Version:** 1.0.0
**Maintained By:** IkeaDocuScan Development Team
