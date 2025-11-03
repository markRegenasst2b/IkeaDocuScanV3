# IkeaDocuScan.PdfTools

A .NET library for extracting text from PDF documents using embedded Python and PyPDF2. No external Python installation required!

## Features

- **Text Extraction**: Extract all text content from PDF files
- **Multiple Input Types**: Support for file paths, byte arrays, and streams
- **PDF Comparison**: Compare text content between two PDF files
- **Metadata Reading**: Extract PDF metadata (title, author, page count, etc.)
- **No External Dependencies**: Python runtime is embedded using CSnakes
- **Async/Await Support**: Full async API for all operations
- **Comprehensive Error Handling**: Specific exceptions for encrypted, corrupted, or invalid PDFs

## Requirements

- .NET 9.0 or later (compatible with .NET 10)
- Windows Server (tested and supported)
- No Python installation required (embedded via CSnakes)

## Installation

### From Source

1. Clone or copy the `IkeaDocuScan.PdfTools` solution to your development environment
2. Build the solution:
   ```bash
   dotnet build
   ```
3. Reference the project in your application or copy the built assemblies

### NuGet Package

_Coming soon - currently source distribution only_

## Testing

The solution includes a comprehensive unit test project using xUnit.

### Running Tests

```bash
# Build and run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Adding Test Data

Before running tests, add sample PDF files to the `tests/IkeaDocuScan.PdfTools.Tests/Data/` folder. See the README in that folder for details on required test files.

For more information, see [tests/IkeaDocuScan.PdfTools.Tests/README.md](tests/IkeaDocuScan.PdfTools.Tests/README.md).

## Initial Setup

### 1. Python Environment Setup

The first time you use the library, CSnakes will set up the embedded Python environment. This happens automatically on first use.

CSnakes will:
- Extract the embedded Python runtime
- Install PyPDF2 from `requirements.txt`
- Set up the Python virtual environment

This process takes a few seconds on first run but only needs to happen once.

### 2. Configuration (Optional)

By default, CSnakes handles all Python configuration. For advanced scenarios, you can customize the Python environment location and other settings through CSnakes configuration.

## Usage Examples

### Basic Text Extraction from File

```csharp
using IkeaDocuScan.PdfTools;
using IkeaDocuScan.PdfTools.Exceptions;

try
{
    // Extract text from a PDF file
    string text = PdfTextExtractor.ExtractText(@"C:\Documents\sample.pdf");
    Console.WriteLine($"Extracted {text.Length} characters");
    Console.WriteLine(text);
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"PDF not found: {ex.Message}");
}
catch (PdfEncryptedException ex)
{
    Console.WriteLine($"PDF is encrypted: {ex.Message}");
}
catch (PdfCorruptedException ex)
{
    Console.WriteLine($"PDF is corrupted: {ex.Message}");
}
catch (PdfToolsException ex)
{
    Console.WriteLine($"Error processing PDF: {ex.Message}");
}
```

### Async Text Extraction

```csharp
// Async version - recommended for large PDFs
string text = await PdfTextExtractor.ExtractTextAsync(@"C:\Documents\large.pdf");
```

### Extract from Byte Array (Database Scenario)

```csharp
// Common scenario: PDF stored in database as varbinary(max)
byte[] pdfBytes = await GetPdfFromDatabase(documentId);
string text = PdfTextExtractor.ExtractText(pdfBytes);

// Or async
string textAsync = await PdfTextExtractor.ExtractTextAsync(pdfBytes);
```

### Extract from Stream

```csharp
// From a file stream
using var fileStream = File.OpenRead(@"C:\Documents\sample.pdf");
string text = PdfTextExtractor.ExtractText(fileStream);

// From HTTP response
using var httpClient = new HttpClient();
using var responseStream = await httpClient.GetStreamAsync("https://example.com/document.pdf");
string text = await PdfTextExtractor.ExtractTextAsync(responseStream);

// From memory stream
using var memoryStream = new MemoryStream(pdfBytes);
string text = PdfTextExtractor.ExtractText(memoryStream);
```

### Get PDF Metadata

```csharp
using IkeaDocuScan.PdfTools.Models;

PdfMetadata metadata = PdfTextExtractor.GetMetadata(@"C:\Documents\sample.pdf");

Console.WriteLine($"Title: {metadata.Title}");
Console.WriteLine($"Author: {metadata.Author}");
Console.WriteLine($"Pages: {metadata.NumberOfPages}");
Console.WriteLine($"Encrypted: {metadata.IsEncrypted}");
Console.WriteLine($"Creator: {metadata.Creator}");
Console.WriteLine($"Producer: {metadata.Producer}");
```

### Compare Two PDFs

```csharp
using IkeaDocuScan.PdfTools.Models;

PdfComparisonResult result = PdfTextExtractor.Compare(
    @"C:\Documents\version1.pdf",
    @"C:\Documents\version2.pdf"
);

Console.WriteLine($"Identical: {result.IsIdentical}");
Console.WriteLine($"Similarity: {result.SimilarityPercentage:F2}%");
Console.WriteLine($"Text 1 Length: {result.Text1Length} chars");
Console.WriteLine($"Text 2 Length: {result.Text2Length} chars");
Console.WriteLine($"Difference: {result.LengthDifference} chars");
Console.WriteLine($"Lines: {result.Text1Lines} vs {result.Text2Lines}");
```

### Full-Text Search Integration

```csharp
public class DocumentSearchService
{
    public async Task<List<SearchResult>> SearchDocuments(string searchTerm)
    {
        var results = new List<SearchResult>();

        // Get all PDF documents from database
        var documents = await GetAllPdfDocuments();

        foreach (var doc in documents)
        {
            try
            {
                // Extract text from PDF bytes
                string text = await PdfTextExtractor.ExtractTextAsync(doc.FileContent);

                // Search for term (case-insensitive)
                if (text.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    // Find all occurrences
                    int count = CountOccurrences(text, searchTerm);

                    results.Add(new SearchResult
                    {
                        DocumentId = doc.Id,
                        DocumentName = doc.Name,
                        MatchCount = count,
                        Preview = GetPreview(text, searchTerm)
                    });
                }
            }
            catch (PdfToolsException ex)
            {
                // Log error but continue with other documents
                Logger.LogWarning($"Could not search document {doc.Id}: {ex.Message}");
            }
        }

        return results.OrderByDescending(r => r.MatchCount).ToList();
    }

    private int CountOccurrences(string text, string term)
    {
        int count = 0;
        int index = 0;

        while ((index = text.IndexOf(term, index, StringComparison.OrdinalIgnoreCase)) != -1)
        {
            count++;
            index += term.Length;
        }

        return count;
    }

    private string GetPreview(string text, string term, int contextLength = 100)
    {
        int index = text.IndexOf(term, StringComparison.OrdinalIgnoreCase);
        if (index == -1) return string.Empty;

        int start = Math.Max(0, index - contextLength);
        int length = Math.Min(text.Length - start, contextLength * 2 + term.Length);

        string preview = text.Substring(start, length);
        return start > 0 ? "..." + preview : preview;
    }
}
```

## Error Handling

The library provides specific exception types for different error scenarios:

### Exception Hierarchy

```
Exception
└── PdfToolsException (base exception for all PDF operations)
    ├── PdfEncryptedException (PDF requires password)
    └── PdfCorruptedException (PDF is damaged or invalid)
```

### Recommended Error Handling Pattern

```csharp
try
{
    string text = PdfTextExtractor.ExtractText(filePath);
}
catch (FileNotFoundException)
{
    // Handle missing file
}
catch (PdfEncryptedException)
{
    // Handle encrypted PDF - may need password support
}
catch (PdfCorruptedException)
{
    // Handle corrupted PDF - may need repair or alternative processing
}
catch (PdfToolsException ex)
{
    // Handle other PDF-specific errors
    Logger.LogError(ex, "PDF processing error");
}
```

## Limitations

### Current Limitations

1. **Text-Only Extraction**: Only extracts existing text layers (no OCR)
   - Scanned images without text layers will return empty strings
   - For OCR support, consider using Tesseract or Azure Computer Vision

2. **No Password Support**: Encrypted PDFs cannot be processed
   - Future versions may add password support

3. **Basic Comparison**: Document comparison uses simple character-based similarity
   - Consider using more advanced diff libraries for detailed comparisons

### Known Issues

- **Performance**: Large PDFs (>100MB) may take several seconds to process
- **Memory**: PDF content is loaded into memory during processing
- **Encoding**: Some PDF encodings may not be fully supported by PyPDF2

## Performance Considerations

### Best Practices

1. **Use Async Methods**: For I/O-bound operations, especially with large files
   ```csharp
   string text = await PdfTextExtractor.ExtractTextAsync(filePath);
   ```

2. **Stream Processing**: For large files, prefer stream-based APIs when possible

3. **Caching**: Cache extracted text to avoid repeated processing
   ```csharp
   var cache = new MemoryCache(new MemoryCacheOptions());
   string cacheKey = $"pdf_text_{documentId}";

   string text = cache.GetOrCreate(cacheKey, entry =>
   {
       entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
       return PdfTextExtractor.ExtractText(pdfBytes);
   });
   ```

4. **Parallel Processing**: Process multiple PDFs concurrently
   ```csharp
   var tasks = pdfFiles.Select(async file =>
   {
       var text = await PdfTextExtractor.ExtractTextAsync(file);
       return new { File = file, Text = text };
   });

   var results = await Task.WhenAll(tasks);
   ```

## Architecture

### Technology Stack

- **.NET 9.0**: Modern .NET framework
- **CSnakes**: Embeds Python runtime in .NET applications
- **PyPDF2 3.0.1**: Python library for PDF manipulation
- **CPython**: Standard Python runtime (embedded)

### How It Works

1. **Build Time**: CSnakes analyzes Python files and generates C# wrapper code
2. **Runtime**: First call initializes embedded Python environment
3. **Execution**: C# calls are translated to Python function calls via C-API
4. **Results**: Python return values are converted to .NET types

### Project Structure

```
IkeaDocuScan.PdfTools/
├── src/
│   └── IkeaDocuScan.PdfTools/
│       ├── Python/
│       │   ├── pdf_extractor.py      # Python implementation
│       │   └── requirements.txt       # Python dependencies
│       ├── Models/
│       │   ├── PdfMetadata.cs         # Metadata DTO
│       │   └── PdfComparisonResult.cs # Comparison result DTO
│       ├── Exceptions/
│       │   └── PdfToolsException.cs   # Custom exceptions
│       └── PdfTextExtractor.cs        # Main API class
└── README.md
```

## Troubleshooting

### Issue: "CSnakes code generation required"

**Cause**: Project hasn't been built yet, or build failed

**Solution**:
```bash
dotnet build
```

### Issue: Python environment initialization fails

**Cause**: Missing dependencies or network issues during setup

**Solution**:
1. Ensure internet connection for PyPDF2 download
2. Check write permissions in application directory
3. Review CSnakes logs for detailed error messages

### Issue: "PDF is encrypted" error

**Cause**: PDF requires password

**Solution**: This version doesn't support encrypted PDFs. Remove encryption first using Adobe Acrobat or similar tools.

### Issue: No text extracted from PDF

**Cause**: PDF contains only images (scanned document)

**Solution**: The PDF doesn't have text layers. Consider using OCR:
- Tesseract OCR
- Azure Computer Vision
- AWS Textract

## Contributing

This is an internal IkeaDocuScan library. For issues or feature requests, contact the development team.

## License

Internal use only - IkeaDocuScan project

## Version History

### Version 1.0.0 (2025-01-27)
- Initial release
- Text extraction from file paths, byte arrays, and streams
- PDF metadata extraction
- PDF comparison functionality
- Comprehensive error handling
- Full async/await support
- XML documentation

## Support

For questions or issues:
- Check this README first
- Review the XML documentation comments in code
- Contact the IkeaDocuScan development team

## Future Enhancements

Potential features for future versions:
- OCR support for scanned documents
- Password-protected PDF support
- Page-by-page text extraction
- Table extraction
- Form field extraction
- PDF to Markdown conversion
- Advanced text comparison with diff visualization
- Performance optimizations for very large PDFs
