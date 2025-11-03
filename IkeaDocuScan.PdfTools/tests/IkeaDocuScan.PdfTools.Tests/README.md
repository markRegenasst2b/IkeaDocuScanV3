# IkeaDocuScan.PdfTools.Tests

Unit test project for the IkeaDocuScan.PdfTools library.

## Overview

This project contains comprehensive unit tests for all public APIs in the PdfTools library, including:
- Text extraction functionality
- PDF metadata reading
- PDF comparison features
- Error handling scenarios
- Various input types (file paths, byte arrays, streams)

## Test Framework

- **xUnit** 2.9.2 - Testing framework
- **FluentAssertions** 6.12.1 - Readable assertions
- **Microsoft.NET.Test.Sdk** 17.11.1 - Test platform
- **coverlet.collector** 6.0.2 - Code coverage

## Project Structure

```
IkeaDocuScan.PdfTools.Tests/
├── Data/                           # Test PDF files
│   ├── README.md                   # Documentation for test files
│   ├── ValidTextPdf.pdf           # (to be added)
│   ├── EncryptedPdf.pdf           # (to be added)
│   └── ...                         # Other test PDFs
├── PdfTextExtractorTests.cs       # Main test class
├── TestHelpers.cs                  # Helper utilities
├── GlobalUsings.cs                 # Global using directives
└── README.md                       # This file
```

## Setup

### 1. Add Test PDF Files

Before running tests, add sample PDF files to the `Data/` folder. See `Data/README.md` for detailed instructions on required test files.

**Minimum required files:**
- `ValidTextPdf.pdf` - A standard PDF with text content
- `EncryptedPdf.pdf` - A password-protected PDF
- `CorruptedPdf.pdf` - A corrupted PDF file
- `ScannedPdf.pdf` - A scanned document without text layer

### 2. Build the Solution

```bash
dotnet build
```

This will:
- Build the PdfTools library
- Build the test project
- Copy test data files to output directory

### 3. Run Tests

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~PdfTextExtractorTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~ExtractText_ValidPdf_ReturnsText"
```

### 4. View Code Coverage

After running tests with coverage:

```bash
# Install report generator (one time)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML coverage report
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

# Open report
start coveragereport/index.html
```

## Writing Tests

### Test Naming Convention

Follow the pattern: `MethodName_Scenario_ExpectedResult`

Examples:
- `ExtractText_ValidPdf_ReturnsText()`
- `ExtractText_EncryptedPdf_ThrowsPdfEncryptedException()`
- `ExtractText_NullPath_ThrowsArgumentNullException()`

### Using Test Helpers

The `TestHelpers` class provides utilities for working with test files:

```csharp
[Fact]
public void Example_Test()
{
    // Get path to test file
    string path = TestHelpers.GetTestFilePath("ValidTextPdf.pdf");

    // Load as byte array
    byte[] bytes = TestHelpers.LoadTestFileAsBytes("ValidTextPdf.pdf");

    // Load as stream
    using var stream = TestHelpers.LoadTestFileAsStream("ValidTextPdf.pdf");

    // Check if file exists
    bool exists = TestHelpers.TestFileExists("ValidTextPdf.pdf");
}
```

### FluentAssertions Examples

Use FluentAssertions for readable assertions:

```csharp
// String assertions
text.Should().NotBeNullOrEmpty();
text.Should().Contain("expected content");
text.Should().StartWith("PDF");
text.Length.Should().BeGreaterThan(100);

// Boolean assertions
result.IsIdentical.Should().BeTrue();
metadata.IsEncrypted.Should().BeFalse();

// Numeric assertions
metadata.NumberOfPages.Should().Be(5);
comparison.SimilarityRatio.Should().BeInRange(0.0, 1.0);

// Exception assertions
Action act = () => PdfTextExtractor.ExtractText(null!);
act.Should().Throw<ArgumentNullException>();

var exception = Assert.Throws<PdfEncryptedException>(
    () => PdfTextExtractor.ExtractText(encryptedPath)
);
exception.Message.Should().Contain("encrypted");
```

### Async Test Examples

```csharp
[Fact]
public async Task ExtractTextAsync_ValidPdf_ReturnsText()
{
    // Arrange
    string path = TestHelpers.GetTestFilePath("ValidTextPdf.pdf");

    // Act
    string text = await PdfTextExtractor.ExtractTextAsync(path);

    // Assert
    text.Should().NotBeNullOrEmpty();
}

[Fact]
public async Task ExtractTextAsync_WithCancellation_CanBeCancelled()
{
    // Arrange
    string path = TestHelpers.GetTestFilePath("LargePdf.pdf");
    var cts = new CancellationTokenSource();
    cts.CancelAfter(TimeSpan.FromMilliseconds(10));

    // Act & Assert
    await Assert.ThrowsAsync<OperationCanceledException>(async () =>
    {
        await PdfTextExtractor.ExtractTextAsync(path, cts.Token);
    });
}
```

### Theory Tests for Multiple Inputs

```csharp
[Theory]
[InlineData("ValidTextPdf.pdf")]
[InlineData("MultipagePdf.pdf")]
[InlineData("PdfWithMetadata.pdf")]
public void ExtractText_VariousValidPdfs_ReturnsText(string fileName)
{
    // Arrange
    string path = TestHelpers.GetTestFilePath(fileName);
    Skip.If(!File.Exists(path), $"Test file {fileName} not available");

    // Act
    string text = PdfTextExtractor.ExtractText(path);

    // Assert
    text.Should().NotBeNullOrEmpty();
}
```

### Skipping Tests When Files Are Missing

```csharp
[Fact]
public void ExtractText_ValidPdf_ReturnsText()
{
    // Arrange
    string path = TestHelpers.GetTestFilePath("ValidTextPdf.pdf");

    // Skip if test file not available
    if (!File.Exists(path))
    {
        // xUnit doesn't have Skip.If() by default
        // You can use a custom attribute or just return
        return;
    }

    // Act
    string text = PdfTextExtractor.ExtractText(path);

    // Assert
    text.Should().NotBeNullOrEmpty();
}
```

Or install `Xunit.SkippableFact`:

```bash
dotnet add package Xunit.SkippableFact
```

Then use:

```csharp
[SkippableFact]
public void ExtractText_ValidPdf_ReturnsText()
{
    string path = TestHelpers.GetTestFilePath("ValidTextPdf.pdf");
    Skip.If(!File.Exists(path), "Test file not available");

    // Test code...
}
```

## Test Categories

### 1. Text Extraction Tests

Test the core functionality of extracting text from PDFs.

```csharp
- ExtractText_ValidPdf_ReturnsText
- ExtractText_MultipagePdf_ReturnsAllPages
- ExtractText_ScannedPdf_ReturnsEmptyString
- ExtractText_FromByteArray_ReturnsText
- ExtractText_FromStream_ReturnsText
- ExtractTextAsync_ValidPdf_ReturnsText
```

### 2. Error Handling Tests

Verify proper exception handling.

```csharp
- ExtractText_NullPath_ThrowsArgumentNullException
- ExtractText_EmptyPath_ThrowsArgumentNullException
- ExtractText_NonExistentFile_ThrowsFileNotFoundException
- ExtractText_EncryptedPdf_ThrowsPdfEncryptedException
- ExtractText_CorruptedPdf_ThrowsPdfCorruptedException
- ExtractText_NullByteArray_ThrowsArgumentNullException
- ExtractText_EmptyByteArray_ThrowsArgumentException
- ExtractText_NullStream_ThrowsArgumentNullException
```

### 3. Metadata Tests

Test metadata extraction functionality.

```csharp
- GetMetadata_ValidPdf_ReturnsMetadata
- GetMetadata_ValidPdf_ReturnsCorrectPageCount
- GetMetadata_PdfWithMetadata_ReturnsTitleAndAuthor
- GetMetadata_EncryptedPdf_ReturnsIsEncryptedTrue
- GetMetadataAsync_ValidPdf_ReturnsMetadata
```

### 4. Comparison Tests

Test PDF comparison features.

```csharp
- Compare_IdenticalPdfs_ReturnsIsIdenticalTrue
- Compare_SameFile_ReturnsSimilarityOne
- Compare_DifferentPdfs_ReturnsValidSimilarityRatio
- Compare_DifferentPdfs_ReturnsSimilarityLessThanOne
- CompareAsync_ValidPdfs_ReturnsComparisonResult
```

### 5. Performance Tests (Optional)

Test performance characteristics.

```csharp
- ExtractText_SmallPdf_CompletesQuickly
- ExtractText_LargePdf_CompletesWithinTimeout
- ExtractTextAsync_MultipleConcurrent_HandlesParallelism
```

## Continuous Integration

### GitHub Actions Example

```yaml
name: Run Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: windows-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 9.0.x

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --no-restore

    - name: Test
      run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"

    - name: Upload coverage
      uses: codecov/codecov-action@v3
```

## Code Coverage Goals

Aim for:
- **Overall**: 80%+ coverage
- **Critical paths**: 100% coverage
  - Error handling
  - Public API methods
  - Argument validation

## Troubleshooting

### Tests Not Finding PDF Files

**Symptom**: Tests skip or fail because PDF files aren't found.

**Solution**:
1. Check that PDF files are in `Data/` folder
2. Verify `.csproj` has: `<None Include="Data\**\*" CopyToOutputDirectory="PreserveNewest" />`
3. Rebuild the project
4. Check output directory: `bin/Debug/net9.0/Data/`

### CSnakes Initialization Errors in Tests

**Symptom**: Tests fail with Python environment errors.

**Solution**:
1. Ensure CSnakes environment is initialized (run library once before tests)
2. Check that `.csnakes/` directory exists
3. Verify internet connection (first run needs to download PyPDF2)

### Tests Run Slowly

**Symptom**: Tests take a long time to execute.

**Solution**:
1. Use smaller test PDFs when possible
2. Consider parallel test execution: `dotnet test --parallel`
3. Cache CSnakes environment initialization

## Contributing

When adding new tests:
1. Follow naming conventions
2. Add XML documentation comments
3. Use FluentAssertions for readable assertions
4. Test both sync and async variants
5. Include error handling tests
6. Update this README if adding new test categories

## Resources

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/introduction)
- [.NET Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

---

**Last Updated**: 2025-01-27
**Version**: 1.0.0
