using FluentAssertions;
using Xunit;

namespace IkeaDocuScan.PdfTools.Tests;

/// <summary>
/// Diagnostic tests to debug ExtractTextFromBytes issue.
/// </summary>
public class DiagnosticTests
{
    private const string DataFolder = "Data";

    private static string GetTestFilePath(string fileName)
    {
        return Path.Combine(Directory.GetCurrentDirectory(), DataFolder, fileName);
    }

    [Fact]
    public void Diagnostic_CompareFilePathVsByteArray()
    {
        // Arrange
        string pdfPath = GetTestFilePath("SmallPdf.pdf");

        if (!File.Exists(pdfPath))
        {
            DebugLogger.Log("SKIP: SmallPdf.pdf not found");
            return;
        }

        DebugLogger.Log($"=== Starting Diagnostic Test ===");
        DebugLogger.Log($"PDF Path: {pdfPath}");
        DebugLogger.Log($"File exists: {File.Exists(pdfPath)}");

        byte[] pdfBytes = File.ReadAllBytes(pdfPath);
        DebugLogger.Log($"Loaded {pdfBytes.Length} bytes from file");
        DebugLogger.LogBytes("PDF bytes", pdfBytes, 20);

        // Act
        DebugLogger.Log("--- Extracting from file path ---");
        string textFromPath = PdfTextExtractor.ExtractText(pdfPath);
        DebugLogger.Log($"Result from path: {textFromPath.Length} characters");
        DebugLogger.Log($"First 100 chars: {(textFromPath.Length > 100 ? textFromPath.Substring(0, 100) : textFromPath)}");

        DebugLogger.Log("--- Extracting from byte array ---");
        string textFromBytes = PdfTextExtractor.ExtractText(pdfBytes);
        DebugLogger.Log($"Result from bytes: {textFromBytes.Length} characters");
        DebugLogger.Log($"First 100 chars: {(textFromBytes.Length > 100 ? textFromBytes.Substring(0, 100) : textFromBytes)}");

        // Assert
        DebugLogger.Log("--- Comparison ---");
        DebugLogger.Log($"Are they equal? {textFromPath == textFromBytes}");
        DebugLogger.Log($"Path length: {textFromPath.Length}, Bytes length: {textFromBytes.Length}");

        if (textFromBytes.Length == 0)
        {
            DebugLogger.Log("ERROR: ExtractTextFromBytes returned empty string!");
        }

        DebugLogger.Log($"=== Test Complete - Check log file: {DebugLogger.GetLogFilePath()} ===");

        // Assert
        textFromBytes.Should().NotBeEmpty("byte array extraction should return text");
        textFromBytes.Should().Be(textFromPath, "both methods should return the same text");
    }
}
