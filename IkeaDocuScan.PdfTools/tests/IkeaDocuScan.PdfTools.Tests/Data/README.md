# Test Data Directory

This directory contains sample PDF files used for unit testing the `IkeaDocuScan.PdfTools` library.

## Purpose

The PDF files in this directory are used to test various scenarios:
- Text extraction from valid PDFs
- Handling of encrypted/password-protected PDFs
- Handling of corrupted PDFs
- Handling of scanned PDFs (images only, no text layer)
- PDF comparison functionality
- Metadata extraction
- Different PDF versions and encodings

## Required Test Files

### Recommended Test Files to Add:

1. **ValidTextPdf.pdf**
   - A standard PDF with text content
   - Should contain multiple pages
   - Used for basic text extraction tests

2. **EncryptedPdf.pdf**
   - A password-protected PDF
   - Used to test `PdfEncryptedException` handling

3. **CorruptedPdf.pdf**
   - A corrupted or invalid PDF file
   - Used to test `PdfCorruptedException` handling

4. **ScannedPdf.pdf**
   - A scanned document (image-only, no text layer)
   - Used to test behavior when no text can be extracted

5. **MultipagePdf.pdf**
   - A PDF with multiple pages (5+ pages recommended)
   - Used to test page-by-page extraction

6. **SmallPdf.pdf**
   - A very small PDF (1 page, minimal content)
   - Used for quick tests and performance baselines

7. **LargePdf.pdf** (optional)
   - A large PDF file (10+ MB)
   - Used for performance and memory testing

8. **PdfWithMetadata.pdf**
   - A PDF with complete metadata (title, author, subject, etc.)
   - Used to test metadata extraction

9. **ComparisonPdf1.pdf** and **ComparisonPdf2.pdf**
   - Two similar PDFs for comparison testing
   - Should have some differences but similar structure

## File Organization

All PDF files should be placed directly in this `Data/` directory:

```
Data/
├── README.md (this file)
├── ValidTextPdf.pdf
├── EncryptedPdf.pdf
├── CorruptedPdf.pdf
├── ScannedPdf.pdf
├── MultipagePdf.pdf
├── SmallPdf.pdf
├── PdfWithMetadata.pdf
├── ComparisonPdf1.pdf
└── ComparisonPdf2.pdf
```

## Configuration

The test project is configured to copy all files from this directory to the output folder during build:

```xml
<None Include="Data\**\*" CopyToOutputDirectory="PreserveNewest" />
```

This ensures PDF files are available during test execution.

## Accessing Files in Tests

In your test methods, access the PDF files using relative paths:

```csharp
public class PdfTextExtractorTests
{
    private const string DataFolder = "Data";

    [Fact]
    public void ExtractText_ValidPdf_ReturnsText()
    {
        // Arrange
        string pdfPath = Path.Combine(DataFolder, "ValidTextPdf.pdf");

        // Act
        string text = PdfTextExtractor.ExtractText(pdfPath);

        // Assert
        text.Should().NotBeNullOrEmpty();
    }
}
```

Or use absolute paths:

```csharp
private string GetTestFilePath(string fileName)
{
    return Path.Combine(
        Directory.GetCurrentDirectory(),
        "Data",
        fileName
    );
}

[Fact]
public void Test_Example()
{
    string path = GetTestFilePath("ValidTextPdf.pdf");
    // Use path in test
}
```

## Creating Test Files

### Creating a Simple Valid PDF

You can create test PDFs using:
- **Microsoft Word**: Save as PDF
- **Online tools**: Various PDF generators
- **Code**: Use libraries like iTextSharp or PdfSharp
- **Adobe Acrobat**: For more control over metadata

### Creating an Encrypted PDF

1. Open any PDF in Adobe Acrobat
2. Go to File > Properties > Security
3. Set password protection
4. Save as `EncryptedPdf.pdf`

### Creating a Corrupted PDF

1. Create a valid PDF
2. Open in a hex editor
3. Modify a few bytes in the middle
4. Save as `CorruptedPdf.pdf`

Or create a text file with PDF header:
```
%PDF-1.4
This is not a valid PDF
%%EOF
```

### Creating a Scanned PDF

1. Scan a document using any scanner
2. Save directly as PDF without OCR
3. Save as `ScannedPdf.pdf`

## Sample Content Suggestions

### ValidTextPdf.pdf Content Example:
```
Title: Sample Document for Testing
Page 1:
This is a sample PDF document used for testing the IkeaDocuScan.PdfTools library.
It contains multiple lines of text that should be extracted successfully.

The quick brown fox jumps over the lazy dog.
This sentence contains all letters of the alphabet.

Page 2:
Second page content...
```

### PdfWithMetadata.pdf Metadata:
- **Title**: Test Document Title
- **Author**: Test Author Name
- **Subject**: Testing PDF Tools
- **Creator**: Microsoft Word
- **Keywords**: test, pdf, extraction

## Git Ignore

Note: PDF files in this directory should be committed to version control for testing purposes. The `.gitignore` at the solution root may need to be updated to allow PDFs in the `tests/` directory:

```gitignore
# Allow test PDF files
!tests/**/Data/*.pdf
```

## Size Considerations

Keep test PDF files reasonably small (< 1 MB each, except `LargePdf.pdf`) to avoid bloating the repository.

## License and Copyright

Ensure all PDF files used for testing are either:
- Created by you
- Public domain
- Licensed for testing purposes
- Sample files from open sources

Do not use copyrighted or proprietary documents.

## Notes for Test Authors

When writing tests:
1. Always check if the file exists before running the test
2. Use descriptive test names that indicate which file is being tested
3. Consider using `[Theory]` with `[InlineData]` for testing multiple files
4. Clean up any temporary files created during tests
5. Use `FluentAssertions` for readable assertions

Example:
```csharp
[Theory]
[InlineData("ValidTextPdf.pdf")]
[InlineData("MultipagePdf.pdf")]
public void ExtractText_ValidPdfs_ReturnsNonEmptyText(string fileName)
{
    // Arrange
    string path = GetTestFilePath(fileName);

    // Act
    string text = PdfTextExtractor.ExtractText(path);

    // Assert
    text.Should().NotBeNullOrEmpty();
    text.Length.Should().BeGreaterThan(0);
}
```

## Maintenance

- Review and update test files periodically
- Add new test files as new scenarios are discovered
- Document any special characteristics of test files here
- Remove obsolete test files that are no longer needed

---

**Last Updated**: 2025-01-27
**Maintained By**: IkeaDocuScan Development Team
