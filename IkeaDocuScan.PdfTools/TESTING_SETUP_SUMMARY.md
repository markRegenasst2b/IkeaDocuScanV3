# Test Project Setup Summary

This document summarizes the test infrastructure created for IkeaDocuScan.PdfTools.

## What Was Created

### Test Project Structure

A complete xUnit test project has been added to the solution with the following components:

```
tests/IkeaDocuScan.PdfTools.Tests/
├── IkeaDocuScan.PdfTools.Tests.csproj  ✅ Test project file
├── GlobalUsings.cs                      ✅ Common imports
├── README.md                            ✅ Testing documentation
├── PdfTextExtractorTests.cs            ✅ Main test class (placeholder)
├── TestHelpers.cs                      ✅ Utility methods
└── Data/
    ├── README.md                       ✅ Test data documentation
    └── .gitignore                      ✅ Allow PDF files
```

### Test Project Features

#### 1. Testing Framework (xUnit)
- **xUnit** 2.9.2 - Modern testing framework
- **FluentAssertions** 6.12.1 - Readable, fluent assertions
- **Microsoft.NET.Test.Sdk** 17.11.1 - Test platform
- **coverlet.collector** 6.0.2 - Code coverage support

#### 2. Project Configuration
- ✅ References `IkeaDocuScan.PdfTools` library
- ✅ Configured to copy Data folder to output
- ✅ Added to solution file
- ✅ Ready for .NET 9.0/10.0

#### 3. Test Helpers (`TestHelpers.cs`)

Provides utility methods for tests:
- `GetTestFilePath(fileName)` - Get path to test PDF
- `TestFileExists(fileName)` - Check if test file exists
- `LoadTestFileAsBytes(fileName)` - Load PDF as byte array
- `LoadTestFileAsStream(fileName)` - Load PDF as stream
- `CreateCorruptedPdf(fileName)` - Create corrupted PDF for testing
- `CleanupTestFile(fileName)` - Clean up temporary files
- `GetAllTestPdfFiles()` - List all test PDFs
- `HasTestPdfFiles()` - Check if test data available

#### 4. Main Test Class (`PdfTextExtractorTests.cs`)

Placeholder test class with:
- Helper methods for file access
- Comments outlining test categories to implement:
  - Text extraction tests
  - Input type tests (path, bytes, stream)
  - Error handling tests
  - Metadata tests
  - Comparison tests
  - Performance tests

#### 5. Data Folder Setup

**Location**: `tests/IkeaDocuScan.PdfTools.Tests/Data/`

**Purpose**: Store sample PDF files for testing

**Documentation**: Comprehensive README explaining:
- Required test files
- How to create test PDFs
- How to access files in tests
- File organization best practices

**Configuration**:
- `.gitignore` configured to **allow** PDF files in this folder
- Files automatically copied to output directory during build

### Required Test PDF Files

Users need to add these sample PDFs to the `Data/` folder:

1. **ValidTextPdf.pdf** - Standard PDF with text
2. **EncryptedPdf.pdf** - Password-protected PDF
3. **CorruptedPdf.pdf** - Corrupted/invalid PDF
4. **ScannedPdf.pdf** - Scanned image (no text layer)
5. **MultipagePdf.pdf** - Multi-page document
6. **SmallPdf.pdf** - Small file for quick tests
7. **PdfWithMetadata.pdf** - PDF with complete metadata
8. **ComparisonPdf1.pdf** & **ComparisonPdf2.pdf** - For comparison tests
9. **LargePdf.pdf** (optional) - For performance testing

## How to Use

### Step 1: Add Test PDF Files

Add sample PDF files to the `Data/` folder:

```bash
cd tests/IkeaDocuScan.PdfTools.Tests/Data/
# Copy your PDF test files here
```

See `Data/README.md` for detailed instructions on creating test files.

### Step 2: Build the Solution

```bash
cd IkeaDocuScan.PdfTools
dotnet build
```

### Step 3: Write Your Tests

Add test methods to `PdfTextExtractorTests.cs`:

```csharp
[Fact]
public void ExtractText_ValidPdf_ReturnsText()
{
    // Arrange
    string path = TestHelpers.GetTestFilePath("ValidTextPdf.pdf");

    // Skip if file doesn't exist
    if (!File.Exists(path))
        return;

    // Act
    string text = PdfTextExtractor.ExtractText(path);

    // Assert
    text.Should().NotBeNullOrEmpty();
}
```

### Step 4: Run Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Test Examples

### Basic Text Extraction Test

```csharp
[Fact]
public void ExtractText_FromFilePath_ReturnsText()
{
    string path = TestHelpers.GetTestFilePath("ValidTextPdf.pdf");
    string text = PdfTextExtractor.ExtractText(path);
    text.Should().NotBeNullOrEmpty();
}
```

### Exception Handling Test

```csharp
[Fact]
public void ExtractText_EncryptedPdf_ThrowsPdfEncryptedException()
{
    string path = TestHelpers.GetTestFilePath("EncryptedPdf.pdf");

    Action act = () => PdfTextExtractor.ExtractText(path);

    act.Should().Throw<PdfEncryptedException>()
       .WithMessage("*encrypted*");
}
```

### Async Test

```csharp
[Fact]
public async Task ExtractTextAsync_ValidPdf_ReturnsText()
{
    string path = TestHelpers.GetTestFilePath("ValidTextPdf.pdf");
    string text = await PdfTextExtractor.ExtractTextAsync(path);
    text.Should().NotBeNullOrEmpty();
}
```

### Theory Test (Multiple Inputs)

```csharp
[Theory]
[InlineData("ValidTextPdf.pdf")]
[InlineData("MultipagePdf.pdf")]
public void ExtractText_VariousValidPdfs_ReturnsText(string fileName)
{
    string path = TestHelpers.GetTestFilePath(fileName);
    if (!File.Exists(path)) return; // Skip if not available

    string text = PdfTextExtractor.ExtractText(path);
    text.Should().NotBeNullOrEmpty();
}
```

### Byte Array Test

```csharp
[Fact]
public void ExtractText_FromByteArray_ReturnsText()
{
    byte[] pdfBytes = TestHelpers.LoadTestFileAsBytes("ValidTextPdf.pdf");
    string text = PdfTextExtractor.ExtractText(pdfBytes);
    text.Should().NotBeNullOrEmpty();
}
```

### Stream Test

```csharp
[Fact]
public void ExtractText_FromStream_ReturnsText()
{
    using var stream = TestHelpers.LoadTestFileAsStream("ValidTextPdf.pdf");
    string text = PdfTextExtractor.ExtractText(stream);
    text.Should().NotBeNullOrEmpty();
}
```

## Next Steps

### Immediate Actions

1. **Add PDF test files** to `Data/` folder
2. **Implement test methods** in `PdfTextExtractorTests.cs`
3. **Run tests** to verify functionality

### Recommended Test Coverage

Implement tests for all these scenarios:

**Text Extraction**:
- ✅ Valid PDF with text
- ✅ Multi-page PDF
- ✅ Scanned PDF (no text)
- ✅ From byte array
- ✅ From stream
- ✅ Async versions

**Error Handling**:
- ✅ Null path/bytes/stream
- ✅ Empty path/bytes
- ✅ Non-existent file
- ✅ Encrypted PDF
- ✅ Corrupted PDF

**Metadata**:
- ✅ Valid PDF metadata
- ✅ Page count
- ✅ Title, author, subject
- ✅ Encryption status

**Comparison**:
- ✅ Identical PDFs
- ✅ Different PDFs
- ✅ Same file with itself
- ✅ Similarity metrics

**Performance** (optional):
- ✅ Small file speed
- ✅ Large file handling
- ✅ Concurrent operations

### Code Coverage Goal

Aim for **80%+ code coverage** of the PdfTools library.

## Documentation

All documentation has been created:

1. **tests/IkeaDocuScan.PdfTools.Tests/README.md**
   - Comprehensive testing guide
   - Test writing patterns
   - FluentAssertions examples
   - CI/CD integration

2. **tests/IkeaDocuScan.PdfTools.Tests/Data/README.md**
   - Required test files
   - How to create test PDFs
   - File access in tests
   - Sample content suggestions

3. **Main README.md** (updated)
   - Added Testing section
   - Links to test documentation

4. **PROJECT_STRUCTURE.md** (updated)
   - Includes test project structure

## Visual Studio Integration

The test project is fully integrated with Visual Studio:

- **Test Explorer**: All tests appear in VS Test Explorer
- **Run/Debug**: Right-click tests to run or debug
- **Code Coverage**: Use VS code coverage tools
- **IntelliSense**: Full support for xUnit and FluentAssertions

## Command Line Testing

```bash
# Build solution
dotnet build

# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~PdfTextExtractorTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~ExtractText_ValidPdf_ReturnsText"

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"

# Generate code coverage report
dotnet test --collect:"XPlat Code Coverage"
```

## Summary

✅ **Complete test infrastructure created**
✅ **xUnit framework configured**
✅ **FluentAssertions for readable tests**
✅ **Helper utilities implemented**
✅ **Data folder ready for PDF samples**
✅ **Comprehensive documentation**
✅ **Integrated with solution**

**Status**: Ready for test implementation

**Next Action**: Add PDF test files to `Data/` folder and implement test methods

---

**Created**: 2025-01-27
**Version**: 1.0.0
