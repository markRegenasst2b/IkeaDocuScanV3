namespace IkeaDocuScan.PdfTools.Tests;

/// <summary>
/// Helper utilities for unit tests.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Gets the path to the Data folder containing test PDF files.
    /// </summary>
    public static string DataFolderPath => Path.Combine(Directory.GetCurrentDirectory(), "Data");

    /// <summary>
    /// Gets the full path to a test file in the Data folder.
    /// </summary>
    /// <param name="fileName">The name of the test file.</param>
    /// <returns>The full path to the test file.</returns>
    public static string GetTestFilePath(string fileName)
    {
        return Path.Combine(DataFolderPath, fileName);
    }

    /// <summary>
    /// Verifies that a test file exists.
    /// </summary>
    /// <param name="fileName">The name of the test file.</param>
    /// <returns>True if the file exists, false otherwise.</returns>
    public static bool TestFileExists(string fileName)
    {
        return File.Exists(GetTestFilePath(fileName));
    }

    /// <summary>
    /// Loads a test PDF file as a byte array.
    /// </summary>
    /// <param name="fileName">The name of the test file.</param>
    /// <returns>The file content as a byte array.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the test file doesn't exist.</exception>
    public static byte[] LoadTestFileAsBytes(string fileName)
    {
        string path = GetTestFilePath(fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Test file not found: {fileName}. Please add PDF samples to the Data folder.", path);
        }
        return File.ReadAllBytes(path);
    }

    /// <summary>
    /// Loads a test PDF file as a stream.
    /// </summary>
    /// <param name="fileName">The name of the test file.</param>
    /// <returns>A stream containing the file content.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the test file doesn't exist.</exception>
    public static Stream LoadTestFileAsStream(string fileName)
    {
        string path = GetTestFilePath(fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Test file not found: {fileName}. Please add PDF samples to the Data folder.", path);
        }
        return File.OpenRead(path);
    }

    /// <summary>
    /// Creates a corrupted PDF file for testing error handling.
    /// </summary>
    /// <param name="fileName">The name of the file to create.</param>
    /// <returns>The path to the created file.</returns>
    public static string CreateCorruptedPdf(string fileName = "corrupted_test.pdf")
    {
        string path = GetTestFilePath(fileName);

        // Create a file with PDF header but invalid content
        string content = "%PDF-1.4\nThis is not valid PDF content\n%%EOF";
        File.WriteAllText(path, content);

        return path;
    }

    /// <summary>
    /// Cleans up temporary test files created during testing.
    /// </summary>
    /// <param name="fileName">The name of the file to delete.</param>
    public static void CleanupTestFile(string fileName)
    {
        string path = GetTestFilePath(fileName);
        if (File.Exists(path))
        {
            try
            {
                File.Delete(path);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    /// <summary>
    /// Lists all PDF files in the Data folder.
    /// </summary>
    /// <returns>Array of PDF file names.</returns>
    public static string[] GetAllTestPdfFiles()
    {
        if (!Directory.Exists(DataFolderPath))
        {
            return Array.Empty<string>();
        }

        return Directory.GetFiles(DataFolderPath, "*.pdf")
            .Select(Path.GetFileName)
            .Where(name => name != null)
            .ToArray()!;
    }

    /// <summary>
    /// Checks if the Data folder has any PDF files available for testing.
    /// </summary>
    /// <returns>True if PDF files are available, false otherwise.</returns>
    public static bool HasTestPdfFiles()
    {
        return GetAllTestPdfFiles().Length > 0;
    }
}
