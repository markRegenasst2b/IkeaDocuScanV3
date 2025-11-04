using FluentAssertions;
using IkeaDocuScan.PdfTools.Exceptions;
using IkeaDocuScan.PdfTools.Models;
using Xunit;

namespace IkeaDocuScan.PdfTools.Tests;

/// <summary>
/// Unit tests for the PdfTextExtractor class.
/// </summary>
public class PdfTextExtractorTests
{
    private const string DataFolder = "Data";

    /// <summary>
    /// Helper method to get the full path to a test file in the Data folder.
    /// </summary>
    private static string GetTestFilePath(string fileName)
    {
        return Path.Combine(Directory.GetCurrentDirectory(), DataFolder, fileName);
    }

    /// <summary>
    /// Helper method to verify that a test file exists.
    /// </summary>
    private static bool TestFileExists(string fileName)
    {
        return File.Exists(GetTestFilePath(fileName));
    }

    #region Text Extraction - File Path Tests

    [Fact]
    public void ExtractText_SmallPdf_ReturnsText()
    {
        // Arrange
        string pdfPath = GetTestFilePath("SmallPdf.pdf");

        // Skip if file doesn't exist
        if (!File.Exists(pdfPath))
            return;

        // Act
        string text = PdfTextExtractor.ExtractText(pdfPath);

        // Assert
        text.Should().NotBeNullOrEmpty();
        text.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ExtractText_MultipagePdf_ReturnsText()
    {
        // Arrange
        string pdfPath = GetTestFilePath("MultipagePdf.pdf");

        // Skip if file doesn't exist
        if (!File.Exists(pdfPath))
            return;

        // Act
        string text = PdfTextExtractor.ExtractText(pdfPath);

        // Assert
        text.Should().NotBeNullOrEmpty();
        text.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ExtractText_LargePdf_ReturnsText()
    {
        // Arrange
        string pdfPath = GetTestFilePath("LargePdf.pdf");

        // Skip if file doesn't exist
        if (!File.Exists(pdfPath))
            return;

        // Act
        string text = PdfTextExtractor.ExtractText(pdfPath);

        // Assert
        text.Should().NotBeNullOrEmpty();
        text.Length.Should().BeGreaterThan(0);
    }

    #endregion

    #region Text Extraction - Byte Array Tests

    [Fact]
    public void ExtractText_FromByteArray_ReturnsText()
    {
        // Arrange
        string pdfPath = GetTestFilePath("SmallPdf.pdf");

        // Skip if file doesn't exist
        if (!File.Exists(pdfPath))
            return;

        byte[] pdfBytes = File.ReadAllBytes(pdfPath);

        // Act
        string text = PdfTextExtractor.ExtractText(pdfBytes);

        // Assert
        text.Should().NotBeNullOrEmpty();
        text.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ExtractText_FromByteArray_MatchesFilePathResult()
    {
        // Arrange
        string pdfPath = GetTestFilePath("SmallPdf.pdf");

        // Skip if file doesn't exist
        if (!File.Exists(pdfPath))
            return;

        byte[] pdfBytes = File.ReadAllBytes(pdfPath);

        // Act
        string textFromPath = PdfTextExtractor.ExtractText(pdfPath);
        string textFromBytes = PdfTextExtractor.ExtractText(pdfBytes);

        // Assert
        textFromBytes.Should().Be(textFromPath);
    }

    #endregion

    #region Text Extraction - Stream Tests

    [Fact]
    public void ExtractText_FromStream_ReturnsText()
    {
        // Arrange
        string pdfPath = GetTestFilePath("SmallPdf.pdf");

        // Skip if file doesn't exist
        if (!File.Exists(pdfPath))
            return;

        using var fileStream = File.OpenRead(pdfPath);

        // Act
        string text = PdfTextExtractor.ExtractText(fileStream);

        // Assert
        text.Should().NotBeNullOrEmpty();
        text.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ExtractText_FromMemoryStream_ReturnsText()
    {
        // Arrange
        string pdfPath = GetTestFilePath("SmallPdf.pdf");

        // Skip if file doesn't exist
        if (!File.Exists(pdfPath))
            return;

        byte[] pdfBytes = File.ReadAllBytes(pdfPath);
        using var memoryStream = new MemoryStream(pdfBytes);

        // Act
        string text = PdfTextExtractor.ExtractText(memoryStream);

        // Assert
        text.Should().NotBeNullOrEmpty();
        text.Length.Should().BeGreaterThan(0);
    }

    #endregion

    #region Async Text Extraction Tests

    [Fact]
    public async Task ExtractTextAsync_SmallPdf_ReturnsText()
    {
        // Arrange
        string pdfPath = GetTestFilePath("SmallPdf.pdf");

        // Skip if file doesn't exist
        if (!File.Exists(pdfPath))
            return;

        // Act
        string text = await PdfTextExtractor.ExtractTextAsync(pdfPath);

        // Assert
        text.Should().NotBeNullOrEmpty();
        text.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExtractTextAsync_FromByteArray_ReturnsText()
    {
        // Arrange
        string pdfPath = GetTestFilePath("SmallPdf.pdf");

        // Skip if file doesn't exist
        if (!File.Exists(pdfPath))
            return;

        byte[] pdfBytes = File.ReadAllBytes(pdfPath);

        // Act
        string text = await PdfTextExtractor.ExtractTextAsync(pdfBytes);

        // Assert
        text.Should().NotBeNullOrEmpty();
        text.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExtractTextAsync_FromStream_ReturnsText()
    {
        // Arrange
        string pdfPath = GetTestFilePath("SmallPdf.pdf");

        // Skip if file doesn't exist
        if (!File.Exists(pdfPath))
            return;

        using var fileStream = File.OpenRead(pdfPath);

        // Act
        string text = await PdfTextExtractor.ExtractTextAsync(fileStream);

        // Assert
        text.Should().NotBeNullOrEmpty();
        text.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExtractTextAsync_WithCancellation_CanBeCancelled()
    {
        // Arrange
        string pdfPath = GetTestFilePath("LargePdf.pdf");

        // Skip if file doesn't exist
        if (!File.Exists(pdfPath))
            return;

        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await PdfTextExtractor.ExtractTextAsync(pdfPath, cts.Token);
        });
    }

    #endregion

    #region Error Handling Tests - Null/Empty Inputs

    [Fact]
    public void ExtractText_NullPath_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => PdfTextExtractor.ExtractText((string)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("filePath");
    }

    [Fact]
    public void ExtractText_EmptyPath_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => PdfTextExtractor.ExtractText(string.Empty);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("filePath");
    }

    [Fact]
    public void ExtractText_WhitespacePath_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => PdfTextExtractor.ExtractText("   ");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("filePath");
    }

    [Fact]
    public void ExtractText_NullByteArray_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => PdfTextExtractor.ExtractText((byte[])null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("pdfBytes");
    }

    [Fact]
    public void ExtractText_EmptyByteArray_ThrowsArgumentException()
    {
        // Act
        Action act = () => PdfTextExtractor.ExtractText(Array.Empty<byte>());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("pdfBytes");
    }

    [Fact]
    public void ExtractText_NullStream_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => PdfTextExtractor.ExtractText((Stream)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("pdfStream");
    }

    #endregion

    #region Error Handling Tests - File Not Found

    [Fact]
    public void ExtractText_NonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        string nonExistentPath = GetTestFilePath("NonExistent.pdf");

        // Act
        Action act = () => PdfTextExtractor.ExtractText(nonExistentPath);

        // Assert
        act.Should().Throw<FileNotFoundException>()
            .WithMessage("*not found*");
    }

    #endregion

    #region Error Handling Tests - Encrypted PDF

    [Fact]
    public void ExtractText_EncryptedPdf_ThrowsPdfEncryptedException()
    {
        // Arrange
        string pdfPath = GetTestFilePath("EncryptedPdf.pdf");

        // Skip if file doesn't exist
        if (!File.Exists(pdfPath))
            return;

        // Act
        Action act = () => PdfTextExtractor.ExtractText(pdfPath);

        // Assert
        act.Should().Throw<PdfEncryptedException>()
            .WithMessage("*encrypted*");
    }

    #endregion

    #region Error Handling Tests - Corrupted PDF

    [Fact]
    public void ExtractText_CorruptedPdf_ThrowsPdfCorruptedOrToolsException()
    {
        // Arrange
        string pdfPath = GetTestFilePath("Corrupted.pdf");

        // Skip if file doesn't exist
        if (!File.Exists(pdfPath))
            return;

        // Act
        Action act = () => PdfTextExtractor.ExtractText(pdfPath);

        // Assert
        // May throw either PdfCorruptedException or PdfToolsException depending on the corruption
        // Using Throw<PdfToolsException>() will match PdfToolsException and all derived types
        act.Should().Throw<PdfToolsException>();
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void GetMetadata_ValidPdf_ReturnsMetadata()
    {
        // Arrange
        string pdfPath = GetTestFilePath("PdfWithMetadata.pdf");

        // Skip if file doesn't exist
        if (!File.Exists(pdfPath))
            return;

        // Act
        PdfMetadata metadata = PdfTextExtractor.GetMetadata(pdfPath);

        // Assert
        metadata.Should().NotBeNull();
        metadata.NumberOfPages.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetMetadata_MultipagePdf_ReturnsCorrectPageCount()
    {
        // Arrange
        string pdfPath = GetTestFilePath("MultipagePdf.pdf");

        // Skip if file doesn't exist
        if (!File.Exists(pdfPath))
            return;

        // Act
        PdfMetadata metadata = PdfTextExtractor.GetMetadata(pdfPath);

        // Assert
        metadata.NumberOfPages.Should().BeGreaterThan(1);
    }

    [Fact]
    public void GetMetadata_SmallPdf_IsNotEncrypted()
    {
        // Arrange
        string pdfPath = GetTestFilePath("SmallPdf.pdf");

        // Skip if file doesn't exist
        if (!File.Exists(pdfPath))
            return;

        // Act
        PdfMetadata metadata = PdfTextExtractor.GetMetadata(pdfPath);

        // Assert
        metadata.IsEncrypted.Should().BeFalse();
    }

    [Fact]
    public void GetMetadata_NullPath_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => PdfTextExtractor.GetMetadata(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("filePath");
    }

    [Fact]
    public void GetMetadata_NonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        string nonExistentPath = GetTestFilePath("NonExistent.pdf");

        // Act
        Action act = () => PdfTextExtractor.GetMetadata(nonExistentPath);

        // Assert
        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public async Task GetMetadataAsync_ValidPdf_ReturnsMetadata()
    {
        // Arrange
        string pdfPath = GetTestFilePath("SmallPdf.pdf");

        // Skip if file doesn't exist
        if (!File.Exists(pdfPath))
            return;

        // Act
        PdfMetadata metadata = await PdfTextExtractor.GetMetadataAsync(pdfPath);

        // Assert
        metadata.Should().NotBeNull();
        metadata.NumberOfPages.Should().BeGreaterThan(0);
    }

    #endregion

    #region Comparison Tests

    [Fact]
    public void Compare_SameFile_ReturnsIsIdenticalTrue()
    {
        // Arrange
        string pdfPath = GetTestFilePath("SmallPdf.pdf");

        // Skip if file doesn't exist
        if (!File.Exists(pdfPath))
            return;

        // Act
        PdfComparisonResult result = PdfTextExtractor.Compare(pdfPath, pdfPath);

        // Assert
        result.Should().NotBeNull();
        result.IsIdentical.Should().BeTrue();
        result.SimilarityRatio.Should().Be(1.0);
        result.SimilarityPercentage.Should().Be(100.0);
        result.LengthDifference.Should().Be(0);
    }

    [Fact]
    public void Compare_TwoDifferentPdfs_ReturnsValidSimilarityMetrics()
    {
        // Arrange
        string pdfPath1 = GetTestFilePath("ComparisonPdf1.pdf");
        string pdfPath2 = GetTestFilePath("ComparisonPdf2.pdf");

        // Skip if files don't exist
        if (!File.Exists(pdfPath1) || !File.Exists(pdfPath2))
            return;

        // Act
        PdfComparisonResult result = PdfTextExtractor.Compare(pdfPath1, pdfPath2);

        // Assert
        result.Should().NotBeNull();
        result.Text1Length.Should().BeGreaterThan(0);
        result.Text2Length.Should().BeGreaterThan(0);
        result.SimilarityRatio.Should().BeInRange(0.0, 1.0);
        result.SimilarityPercentage.Should().BeInRange(0.0, 100.0);
    }

    [Fact]
    public void Compare_IdenticalPdfs_HasZeroLengthDifference()
    {
        // Arrange
        string pdfPath1 = GetTestFilePath("ComparisonPdf1.pdf");
        string pdfPath2 = GetTestFilePath("ComparisonPdf1.pdf");

        // Skip if file doesn't exist
        if (!File.Exists(pdfPath1))
            return;

        // Act
        PdfComparisonResult result = PdfTextExtractor.Compare(pdfPath1, pdfPath2);

        // Assert
        result.LengthDifference.Should().Be(0);
        result.Text1Length.Should().Be(result.Text2Length);
        result.Text1Lines.Should().Be(result.Text2Lines);
    }

    [Fact]
    public void Compare_NullFirstPath_ThrowsArgumentNullException()
    {
        // Arrange
        string pdfPath2 = GetTestFilePath("SmallPdf.pdf");

        // Act
        Action act = () => PdfTextExtractor.Compare(null!, pdfPath2);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("filePath1");
    }

    [Fact]
    public void Compare_NullSecondPath_ThrowsArgumentNullException()
    {
        // Arrange
        string pdfPath1 = GetTestFilePath("SmallPdf.pdf");

        // Act
        Action act = () => PdfTextExtractor.Compare(pdfPath1, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("filePath2");
    }

    [Fact]
    public void Compare_FirstFileNotFound_ThrowsFileNotFoundException()
    {
        // Arrange
        string nonExistentPath = GetTestFilePath("NonExistent.pdf");
        string pdfPath2 = GetTestFilePath("SmallPdf.pdf");

        // Act
        Action act = () => PdfTextExtractor.Compare(nonExistentPath, pdfPath2);

        // Assert
        act.Should().Throw<FileNotFoundException>()
            .WithMessage("*first*", "should mention it's the first file");
    }

    [Fact]
    public void Compare_SecondFileNotFound_ThrowsFileNotFoundException()
    {
        // Arrange
        string pdfPath1 = GetTestFilePath("SmallPdf.pdf");
        string nonExistentPath = GetTestFilePath("NonExistent.pdf");

        // Skip if first file doesn't exist
        if (!File.Exists(pdfPath1))
            return;

        // Act
        Action act = () => PdfTextExtractor.Compare(pdfPath1, nonExistentPath);

        // Assert
        act.Should().Throw<FileNotFoundException>()
            .WithMessage("*second*", "should mention it's the second file");
    }

    [Fact]
    public async Task CompareAsync_ValidPdfs_ReturnsComparisonResult()
    {
        // Arrange
        string pdfPath1 = GetTestFilePath("SmallPdf.pdf");
        string pdfPath2 = GetTestFilePath("SmallPdf.pdf");

        // Skip if file doesn't exist
        if (!File.Exists(pdfPath1))
            return;

        // Act
        PdfComparisonResult result = await PdfTextExtractor.CompareAsync(pdfPath1, pdfPath2);

        // Assert
        result.Should().NotBeNull();
        result.IsIdentical.Should().BeTrue();
    }

    #endregion

    #region Theory Tests - Multiple PDFs

    [Theory]
    [InlineData("SmallPdf.pdf")]
    [InlineData("MultipagePdf.pdf")]
    [InlineData("LargePdf.pdf")]
    [InlineData("PdfWithMetadata.pdf")]
    public void ExtractText_VariousValidPdfs_ReturnsNonEmptyText(string fileName)
    {
        // Arrange
        string pdfPath = GetTestFilePath(fileName);

        // Skip if file doesn't exist
        if (!File.Exists(pdfPath))
            return;

        // Act
        string text = PdfTextExtractor.ExtractText(pdfPath);

        // Assert
        text.Should().NotBeNullOrEmpty();
        text.Length.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("SmallPdf.pdf")]
    [InlineData("MultipagePdf.pdf")]
    [InlineData("PdfWithMetadata.pdf")]
    public void GetMetadata_VariousValidPdfs_ReturnsValidMetadata(string fileName)
    {
        // Arrange
        string pdfPath = GetTestFilePath(fileName);

        // Skip if file doesn't exist
        if (!File.Exists(pdfPath))
            return;

        // Act
        PdfMetadata metadata = PdfTextExtractor.GetMetadata(pdfPath);

        // Assert
        metadata.Should().NotBeNull();
        metadata.NumberOfPages.Should().BeGreaterThan(0);
        metadata.IsEncrypted.Should().BeFalse();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void ExtractText_SmallPdf_CompletesQuickly()
    {
        // Arrange
        string pdfPath = GetTestFilePath("SmallPdf.pdf");

        // Skip if file doesn't exist
        if (!File.Exists(pdfPath))
            return;

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        string text = PdfTextExtractor.ExtractText(pdfPath);
        stopwatch.Stop();

        // Assert
        text.Should().NotBeNullOrEmpty();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, "small PDF should extract in less than 5 seconds");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ExtractText_ThenCompare_WorksTogether()
    {
        // Arrange
        string pdfPath = GetTestFilePath("SmallPdf.pdf");

        // Skip if file doesn't exist
        if (!File.Exists(pdfPath))
            return;

        // Act
        string text = PdfTextExtractor.ExtractText(pdfPath);
        PdfComparisonResult comparison = PdfTextExtractor.Compare(pdfPath, pdfPath);

        // Assert
        text.Should().NotBeNullOrEmpty();
        comparison.IsIdentical.Should().BeTrue();
        comparison.Text1Length.Should().Be(text.Length);
    }

    [Fact]
    public void GetMetadata_ThenExtractText_WorksTogether()
    {
        // Arrange
        string pdfPath = GetTestFilePath("MultipagePdf.pdf");

        // Skip if file doesn't exist
        if (!File.Exists(pdfPath))
            return;

        // Act
        PdfMetadata metadata = PdfTextExtractor.GetMetadata(pdfPath);
        string text = PdfTextExtractor.ExtractText(pdfPath);

        // Assert
        metadata.NumberOfPages.Should().BeGreaterThan(0);
        text.Should().NotBeNullOrEmpty();
    }

    #endregion
}
