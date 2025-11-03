# API Reference - IkeaDocuScan.PdfTools

Complete API documentation for the IkeaDocuScan.PdfTools library.

## Namespace: IkeaDocuScan.PdfTools

### PdfTextExtractor Class

Main class for PDF text extraction and manipulation operations.

#### Text Extraction Methods

##### ExtractText(string filePath)

Extracts all text content from a PDF file.

```csharp
public static string ExtractText(string filePath)
```

**Parameters:**
- `filePath` (string): The full path to the PDF file

**Returns:**
- `string`: The extracted text content. Returns empty string if no text found.

**Exceptions:**
- `ArgumentNullException`: When filePath is null or empty
- `FileNotFoundException`: When the PDF file does not exist
- `PdfEncryptedException`: When the PDF is encrypted
- `PdfCorruptedException`: When the PDF is corrupted
- `PdfToolsException`: When any other error occurs

**Example:**
```csharp
string text = PdfTextExtractor.ExtractText(@"C:\Documents\sample.pdf");
```

---

##### ExtractTextAsync(string filePath, CancellationToken cancellationToken = default)

Asynchronously extracts all text content from a PDF file.

```csharp
public static Task<string> ExtractTextAsync(
    string filePath,
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `filePath` (string): The full path to the PDF file
- `cancellationToken` (CancellationToken): Optional cancellation token

**Returns:**
- `Task<string>`: Task containing the extracted text content

**Exceptions:**
- Same as `ExtractText(string)`

**Example:**
```csharp
string text = await PdfTextExtractor.ExtractTextAsync(@"C:\Documents\sample.pdf");
```

---

##### ExtractText(byte[] pdfBytes)

Extracts all text content from a PDF provided as a byte array.

```csharp
public static string ExtractText(byte[] pdfBytes)
```

**Parameters:**
- `pdfBytes` (byte[]): The PDF file content as a byte array

**Returns:**
- `string`: The extracted text content

**Exceptions:**
- `ArgumentNullException`: When pdfBytes is null
- `ArgumentException`: When pdfBytes is empty
- `PdfEncryptedException`: When the PDF is encrypted
- `PdfCorruptedException`: When the PDF is corrupted
- `PdfToolsException`: When any other error occurs

**Example:**
```csharp
byte[] pdfData = await GetPdfFromDatabase(documentId);
string text = PdfTextExtractor.ExtractText(pdfData);
```

---

##### ExtractTextAsync(byte[] pdfBytes, CancellationToken cancellationToken = default)

Asynchronously extracts all text content from a PDF provided as a byte array.

```csharp
public static Task<string> ExtractTextAsync(
    byte[] pdfBytes,
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `pdfBytes` (byte[]): The PDF file content as a byte array
- `cancellationToken` (CancellationToken): Optional cancellation token

**Returns:**
- `Task<string>`: Task containing the extracted text content

**Exceptions:**
- Same as `ExtractText(byte[])`

**Example:**
```csharp
byte[] pdfData = await GetPdfFromDatabase(documentId);
string text = await PdfTextExtractor.ExtractTextAsync(pdfData);
```

---

##### ExtractText(Stream pdfStream)

Extracts all text content from a PDF provided as a stream.

```csharp
public static string ExtractText(Stream pdfStream)
```

**Parameters:**
- `pdfStream` (Stream): A stream containing the PDF file content

**Returns:**
- `string`: The extracted text content

**Exceptions:**
- `ArgumentNullException`: When pdfStream is null
- `PdfEncryptedException`: When the PDF is encrypted
- `PdfCorruptedException`: When the PDF is corrupted
- `PdfToolsException`: When any other error occurs

**Example:**
```csharp
using var fileStream = File.OpenRead(@"C:\Documents\sample.pdf");
string text = PdfTextExtractor.ExtractText(fileStream);
```

---

##### ExtractTextAsync(Stream pdfStream, CancellationToken cancellationToken = default)

Asynchronously extracts all text content from a PDF provided as a stream.

```csharp
public static Task<string> ExtractTextAsync(
    Stream pdfStream,
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `pdfStream` (Stream): A stream containing the PDF file content
- `cancellationToken` (CancellationToken): Optional cancellation token

**Returns:**
- `Task<string>`: Task containing the extracted text content

**Exceptions:**
- Same as `ExtractText(Stream)`

**Example:**
```csharp
using var fileStream = File.OpenRead(@"C:\Documents\sample.pdf");
string text = await PdfTextExtractor.ExtractTextAsync(fileStream);
```

---

#### Metadata Methods

##### GetMetadata(string filePath)

Gets metadata information about a PDF file.

```csharp
public static PdfMetadata GetMetadata(string filePath)
```

**Parameters:**
- `filePath` (string): The full path to the PDF file

**Returns:**
- `PdfMetadata`: Object containing PDF metadata

**Exceptions:**
- `ArgumentNullException`: When filePath is null or empty
- `FileNotFoundException`: When the PDF file does not exist
- `PdfToolsException`: When an error occurs reading metadata

**Example:**
```csharp
PdfMetadata metadata = PdfTextExtractor.GetMetadata(@"C:\Documents\sample.pdf");
Console.WriteLine($"Pages: {metadata.NumberOfPages}");
Console.WriteLine($"Author: {metadata.Author}");
```

---

##### GetMetadataAsync(string filePath, CancellationToken cancellationToken = default)

Asynchronously gets metadata information about a PDF file.

```csharp
public static Task<PdfMetadata> GetMetadataAsync(
    string filePath,
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `filePath` (string): The full path to the PDF file
- `cancellationToken` (CancellationToken): Optional cancellation token

**Returns:**
- `Task<PdfMetadata>`: Task containing the PDF metadata

**Exceptions:**
- Same as `GetMetadata(string)`

**Example:**
```csharp
PdfMetadata metadata = await PdfTextExtractor.GetMetadataAsync(@"C:\Documents\sample.pdf");
```

---

#### Comparison Methods

##### Compare(string filePath1, string filePath2)

Compares the text content of two PDF files.

```csharp
public static PdfComparisonResult Compare(string filePath1, string filePath2)
```

**Parameters:**
- `filePath1` (string): The path to the first PDF file
- `filePath2` (string): The path to the second PDF file

**Returns:**
- `PdfComparisonResult`: Object containing comparison metrics

**Exceptions:**
- `ArgumentNullException`: When either file path is null or empty
- `FileNotFoundException`: When either PDF file does not exist
- `PdfToolsException`: When an error occurs during comparison

**Example:**
```csharp
PdfComparisonResult result = PdfTextExtractor.Compare(
    @"C:\Documents\v1.pdf",
    @"C:\Documents\v2.pdf"
);
Console.WriteLine($"Similarity: {result.SimilarityPercentage:F2}%");
Console.WriteLine($"Identical: {result.IsIdentical}");
```

---

##### CompareAsync(string filePath1, string filePath2, CancellationToken cancellationToken = default)

Asynchronously compares the text content of two PDF files.

```csharp
public static Task<PdfComparisonResult> CompareAsync(
    string filePath1,
    string filePath2,
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `filePath1` (string): The path to the first PDF file
- `filePath2` (string): The path to the second PDF file
- `cancellationToken` (CancellationToken): Optional cancellation token

**Returns:**
- `Task<PdfComparisonResult>`: Task containing the comparison metrics

**Exceptions:**
- Same as `Compare(string, string)`

**Example:**
```csharp
PdfComparisonResult result = await PdfTextExtractor.CompareAsync(
    @"C:\Documents\v1.pdf",
    @"C:\Documents\v2.pdf"
);
```

---

## Namespace: IkeaDocuScan.PdfTools.Models

### PdfMetadata Class

Contains metadata information about a PDF document.

#### Properties

```csharp
public class PdfMetadata
{
    public int NumberOfPages { get; set; }
    public bool IsEncrypted { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Subject { get; set; }
    public string? Creator { get; set; }
    public string? Producer { get; set; }
}
```

**Property Descriptions:**

- `NumberOfPages`: The number of pages in the PDF document
- `IsEncrypted`: True if the PDF is encrypted
- `Title`: The title metadata field (may be null)
- `Author`: The author metadata field (may be null)
- `Subject`: The subject metadata field (may be null)
- `Creator`: The creator application (may be null)
- `Producer`: The producer application (may be null)

**Example:**
```csharp
PdfMetadata metadata = PdfTextExtractor.GetMetadata(filePath);
if (metadata.IsEncrypted)
{
    Console.WriteLine("PDF is encrypted");
}
Console.WriteLine($"Document has {metadata.NumberOfPages} pages");
if (!string.IsNullOrEmpty(metadata.Author))
{
    Console.WriteLine($"Author: {metadata.Author}");
}
```

---

### PdfComparisonResult Class

Contains the results of comparing two PDF documents.

#### Properties

```csharp
public class PdfComparisonResult
{
    public int Text1Length { get; set; }
    public int Text2Length { get; set; }
    public int Text1Lines { get; set; }
    public int Text2Lines { get; set; }
    public bool IsIdentical { get; set; }
    public int LengthDifference { get; set; }
    public double SimilarityRatio { get; set; }
    public double SimilarityPercentage { get; }
}
```

**Property Descriptions:**

- `Text1Length`: Total character count of the first PDF's text
- `Text2Length`: Total character count of the second PDF's text
- `Text1Lines`: Number of lines in the first PDF's text
- `Text2Lines`: Number of lines in the second PDF's text
- `IsIdentical`: True if both PDFs have identical text content
- `LengthDifference`: Absolute difference in character count
- `SimilarityRatio`: Similarity ratio from 0.0 to 1.0
- `SimilarityPercentage`: Similarity as percentage (0 to 100), computed from SimilarityRatio

**Example:**
```csharp
PdfComparisonResult result = PdfTextExtractor.Compare(file1, file2);

if (result.IsIdentical)
{
    Console.WriteLine("PDFs are identical");
}
else
{
    Console.WriteLine($"Similarity: {result.SimilarityPercentage:F2}%");
    Console.WriteLine($"Character difference: {result.LengthDifference}");
    Console.WriteLine($"PDF 1: {result.Text1Length} chars, {result.Text1Lines} lines");
    Console.WriteLine($"PDF 2: {result.Text2Length} chars, {result.Text2Lines} lines");
}
```

---

## Namespace: IkeaDocuScan.PdfTools.Exceptions

### PdfToolsException Class

Base exception for all PDF processing errors.

```csharp
public class PdfToolsException : Exception
```

**Constructors:**
```csharp
public PdfToolsException()
public PdfToolsException(string message)
public PdfToolsException(string message, Exception innerException)
```

**Usage:**
```csharp
try
{
    string text = PdfTextExtractor.ExtractText(filePath);
}
catch (PdfToolsException ex)
{
    // Handle any PDF processing error
    Logger.LogError(ex, "PDF processing failed");
}
```

---

### PdfEncryptedException Class

Exception thrown when a PDF is encrypted and cannot be read.

```csharp
public class PdfEncryptedException : PdfToolsException
```

**Constructors:**
```csharp
public PdfEncryptedException()
public PdfEncryptedException(string message)
public PdfEncryptedException(string message, Exception innerException)
```

**Usage:**
```csharp
try
{
    string text = PdfTextExtractor.ExtractText(filePath);
}
catch (PdfEncryptedException)
{
    // Handle encrypted PDF - may need password support
    Console.WriteLine("PDF is password-protected");
}
```

---

### PdfCorruptedException Class

Exception thrown when a PDF is corrupted or cannot be parsed.

```csharp
public class PdfCorruptedException : PdfToolsException
```

**Constructors:**
```csharp
public PdfCorruptedException()
public PdfCorruptedException(string message)
public PdfCorruptedException(string message, Exception innerException)
```

**Usage:**
```csharp
try
{
    string text = PdfTextExtractor.ExtractText(filePath);
}
catch (PdfCorruptedException)
{
    // Handle corrupted PDF - file may need repair
    Console.WriteLine("PDF file is corrupted");
}
```

---

## Exception Hierarchy

```
Exception
└── PdfToolsException
    ├── PdfEncryptedException
    └── PdfCorruptedException
```

## Best Practices

### Error Handling Pattern

```csharp
try
{
    string text = await PdfTextExtractor.ExtractTextAsync(filePath);
    // Process text
}
catch (FileNotFoundException)
{
    // Handle missing file
}
catch (PdfEncryptedException)
{
    // Handle encrypted PDF
}
catch (PdfCorruptedException)
{
    // Handle corrupted PDF
}
catch (PdfToolsException ex)
{
    // Handle other PDF errors
    Logger.LogError(ex, "PDF processing error");
}
```

### Async/Await Usage

For better performance, especially with large files or I/O operations:

```csharp
// Prefer async methods
string text = await PdfTextExtractor.ExtractTextAsync(filePath);

// Instead of synchronous
string text = PdfTextExtractor.ExtractText(filePath);
```

### Cancellation Support

All async methods support cancellation:

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    string text = await PdfTextExtractor.ExtractTextAsync(
        filePath,
        cts.Token
    );
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation timed out");
}
```

### Null Handling

String properties in models may be null:

```csharp
PdfMetadata metadata = PdfTextExtractor.GetMetadata(filePath);

// Safe access
string title = metadata.Title ?? "Untitled";

// Or use null-conditional operator
Console.WriteLine($"Author: {metadata.Author ?? "Unknown"}");
```

---

## Version Information

- **Library Version**: 1.0.0
- **Target Framework**: .NET 9.0+
- **API Stability**: Stable
- **Last Updated**: 2025-01-27

---

## See Also

- [README.md](README.md) - Usage examples and getting started
- [SETUP_GUIDE.md](SETUP_GUIDE.md) - Detailed setup and deployment instructions
- [CHANGELOG.md](CHANGELOG.md) - Version history and changes
