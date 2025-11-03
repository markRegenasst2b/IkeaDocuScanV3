using IkeaDocuScan.PdfTools.Exceptions;
using IkeaDocuScan.PdfTools.Models;
using CSnakes.Runtime;
using CSnakes.Runtime.Python;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IkeaDocuScan.PdfTools;

/// <summary>
/// Provides methods for extracting text from PDF documents using embedded Python and PyPDF2.
/// </summary>
/// <remarks>
/// This class uses CSnakes to embed Python runtime and PyPDF2 library for PDF text extraction.
/// No external Python installation is required.
/// </remarks>
public class PdfTextExtractor
{
    private static readonly Lazy<IPythonEnvironment> _pythonEnv = new(() =>
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services
            .WithPython()
            .WithHome(Path.Join(AppContext.BaseDirectory, "Python"))
            .FromRedistributable(CSnakes.Runtime.Locators.RedistributablePythonVersion.Python3_12)
            .WithVirtualEnvironment(".venv")
            .WithPipInstaller("./requirements.txt"); 

        var app = builder.Build();
        return app.Services.GetRequiredService<IPythonEnvironment>();
    });

    private static IPythonEnvironment PythonEnv => _pythonEnv.Value;

    /// <summary>
    /// Extracts all text content from a PDF file.
    /// </summary>
    /// <param name="filePath">The full path to the PDF file.</param>
    /// <returns>The extracted text content as a string. Returns an empty string if no text is found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the PDF file does not exist.</exception>
    /// <exception cref="PdfEncryptedException">Thrown when the PDF is encrypted and cannot be read.</exception>
    /// <exception cref="PdfCorruptedException">Thrown when the PDF is corrupted or cannot be parsed.</exception>
    /// <exception cref="PdfToolsException">Thrown when any other error occurs during text extraction.</exception>
    public static string ExtractText(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentNullException(nameof(filePath), "File path cannot be null or empty.");
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"PDF file not found: {filePath}", filePath);
        }

        try
        {
            // Call the CSnakes-generated Python wrapper
            // Access the Python module through the IPythonEnvironment
            using var module = PythonEnv.PdfExtractor();
            var result = module.ExtractTextFromPath(filePath);
            return result ?? string.Empty;
        }
        catch (FileNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return HandlePythonException(ex);
        }
    }

    /// <summary>
    /// Asynchronously extracts all text content from a PDF file.
    /// </summary>
    /// <param name="filePath">The full path to the PDF file.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the extracted text content.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the PDF file does not exist.</exception>
    /// <exception cref="PdfEncryptedException">Thrown when the PDF is encrypted and cannot be read.</exception>
    /// <exception cref="PdfCorruptedException">Thrown when the PDF is corrupted or cannot be parsed.</exception>
    /// <exception cref="PdfToolsException">Thrown when any other error occurs during text extraction.</exception>
    public static Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => ExtractText(filePath), cancellationToken);
    }

    /// <summary>
    /// Extracts all text content from a PDF provided as a byte array.
    /// </summary>
    /// <param name="pdfBytes">The PDF file content as a byte array.</param>
    /// <returns>The extracted text content as a string. Returns an empty string if no text is found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pdfBytes"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="pdfBytes"/> is empty.</exception>
    /// <exception cref="PdfEncryptedException">Thrown when the PDF is encrypted and cannot be read.</exception>
    /// <exception cref="PdfCorruptedException">Thrown when the PDF is corrupted or cannot be parsed.</exception>
    /// <exception cref="PdfToolsException">Thrown when any other error occurs during text extraction.</exception>
    public static string ExtractText(byte[] pdfBytes)
    {
        if (pdfBytes == null)
        {
            throw new ArgumentNullException(nameof(pdfBytes), "PDF bytes cannot be null.");
        }

        if (pdfBytes.Length == 0)
        {
            throw new ArgumentException("PDF bytes cannot be empty.", nameof(pdfBytes));
        }

        try
        {
            // Call the CSnakes-generated Python wrapper
            using var module = PythonEnv.PdfExtractor();
            var result = module.ExtractTextFromBytes(pdfBytes);
            return result ?? string.Empty;
        }
        catch (Exception ex)
        {
            return HandlePythonException(ex);
        }
    }

    /// <summary>
    /// Asynchronously extracts all text content from a PDF provided as a byte array.
    /// </summary>
    /// <param name="pdfBytes">The PDF file content as a byte array.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the extracted text content.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pdfBytes"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="pdfBytes"/> is empty.</exception>
    /// <exception cref="PdfEncryptedException">Thrown when the PDF is encrypted and cannot be read.</exception>
    /// <exception cref="PdfCorruptedException">Thrown when the PDF is corrupted or cannot be parsed.</exception>
    /// <exception cref="PdfToolsException">Thrown when any other error occurs during text extraction.</exception>
    public static Task<string> ExtractTextAsync(byte[] pdfBytes, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => ExtractText(pdfBytes), cancellationToken);
    }

    /// <summary>
    /// Extracts all text content from a PDF provided as a stream.
    /// </summary>
    /// <param name="pdfStream">A stream containing the PDF file content.</param>
    /// <returns>The extracted text content as a string. Returns an empty string if no text is found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pdfStream"/> is null.</exception>
    /// <exception cref="PdfEncryptedException">Thrown when the PDF is encrypted and cannot be read.</exception>
    /// <exception cref="PdfCorruptedException">Thrown when the PDF is corrupted or cannot be parsed.</exception>
    /// <exception cref="PdfToolsException">Thrown when any other error occurs during text extraction.</exception>
    public static string ExtractText(Stream pdfStream)
    {
        if (pdfStream == null)
        {
            throw new ArgumentNullException(nameof(pdfStream), "PDF stream cannot be null.");
        }

        using var memoryStream = new MemoryStream();
        pdfStream.CopyTo(memoryStream);
        return ExtractText(memoryStream.ToArray());
    }

    /// <summary>
    /// Asynchronously extracts all text content from a PDF provided as a stream.
    /// </summary>
    /// <param name="pdfStream">A stream containing the PDF file content.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the extracted text content.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pdfStream"/> is null.</exception>
    /// <exception cref="PdfEncryptedException">Thrown when the PDF is encrypted and cannot be read.</exception>
    /// <exception cref="PdfCorruptedException">Thrown when the PDF is corrupted or cannot be parsed.</exception>
    /// <exception cref="PdfToolsException">Thrown when any other error occurs during text extraction.</exception>
    public static async Task<string> ExtractTextAsync(Stream pdfStream, CancellationToken cancellationToken = default)
    {
        if (pdfStream == null)
        {
            throw new ArgumentNullException(nameof(pdfStream), "PDF stream cannot be null.");
        }

        using var memoryStream = new MemoryStream();
        await pdfStream.CopyToAsync(memoryStream, cancellationToken);
        return await ExtractTextAsync(memoryStream.ToArray(), cancellationToken);
    }

    /// <summary>
    /// Gets metadata information about a PDF file.
    /// </summary>
    /// <param name="filePath">The full path to the PDF file.</param>
    /// <returns>A <see cref="PdfMetadata"/> object containing PDF metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the PDF file does not exist.</exception>
    /// <exception cref="PdfToolsException">Thrown when an error occurs reading the PDF metadata.</exception>
    public static PdfMetadata GetMetadata(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentNullException(nameof(filePath), "File path cannot be null or empty.");
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"PDF file not found: {filePath}", filePath);
        }

        try
        {
            // Call the CSnakes-generated Python wrapper
            using var module = PythonEnv.PdfExtractor();
            using var pythonDict = module.GetPdfInfo(filePath);

            // Convert Python dict to C# PdfMetadata object
            return new PdfMetadata
            {
                NumberOfPages = GetDictInt(pythonDict, "num_pages"),
                IsEncrypted = GetDictBool(pythonDict, "is_encrypted"),
                Title = GetDictString(pythonDict, "title"),
                Author = GetDictString(pythonDict, "author"),
                Subject = GetDictString(pythonDict, "subject"),
                Creator = GetDictString(pythonDict, "creator"),
                Producer = GetDictString(pythonDict, "producer")
            };
        }
        catch (FileNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new PdfToolsException($"Error reading PDF metadata: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously gets metadata information about a PDF file.
    /// </summary>
    /// <param name="filePath">The full path to the PDF file.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the PDF metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the PDF file does not exist.</exception>
    /// <exception cref="PdfToolsException">Thrown when an error occurs reading the PDF metadata.</exception>
    public static Task<PdfMetadata> GetMetadataAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => GetMetadata(filePath), cancellationToken);
    }

    /// <summary>
    /// Compares the text content of two PDF files.
    /// </summary>
    /// <param name="filePath1">The path to the first PDF file.</param>
    /// <param name="filePath2">The path to the second PDF file.</param>
    /// <returns>A <see cref="PdfComparisonResult"/> containing the comparison metrics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either file path is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when either PDF file does not exist.</exception>
    /// <exception cref="PdfToolsException">Thrown when an error occurs during comparison.</exception>
    public static PdfComparisonResult Compare(string filePath1, string filePath2)
    {
        if (string.IsNullOrWhiteSpace(filePath1))
        {
            throw new ArgumentNullException(nameof(filePath1), "First file path cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(filePath2))
        {
            throw new ArgumentNullException(nameof(filePath2), "Second file path cannot be null or empty.");
        }

        if (!File.Exists(filePath1))
        {
            throw new FileNotFoundException($"First PDF file not found: {filePath1}", filePath1);
        }

        if (!File.Exists(filePath2))
        {
            throw new FileNotFoundException($"Second PDF file not found: {filePath2}", filePath2);
        }

        try
        {
            // Call the CSnakes-generated Python wrapper
            using var module = PythonEnv.PdfExtractor();
            using var pythonDict = module.ComparePdfText(filePath1, filePath2);

            // Convert Python dict to C# PdfComparisonResult object
            return new PdfComparisonResult
            {
                Text1Length = GetDictInt(pythonDict, "text1_length"),
                Text2Length = GetDictInt(pythonDict, "text2_length"),
                Text1Lines = GetDictInt(pythonDict, "text1_lines"),
                Text2Lines = GetDictInt(pythonDict, "text2_lines"),
                IsIdentical = GetDictBool(pythonDict, "is_identical"),
                LengthDifference = GetDictInt(pythonDict, "length_difference"),
                SimilarityRatio = GetDictDouble(pythonDict, "similarity_ratio")
            };
        }
        catch (FileNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new PdfToolsException($"Error comparing PDFs: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously compares the text content of two PDF files.
    /// </summary>
    /// <param name="filePath1">The path to the first PDF file.</param>
    /// <param name="filePath2">The path to the second PDF file.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the comparison metrics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either file path is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when either PDF file does not exist.</exception>
    /// <exception cref="PdfToolsException">Thrown when an error occurs during comparison.</exception>
    public static Task<PdfComparisonResult> CompareAsync(string filePath1, string filePath2,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() => Compare(filePath1, filePath2), cancellationToken);
    }

    /// <summary>
    /// Handles exceptions from Python code and converts them to appropriate .NET exceptions.
    /// </summary>
    private static string HandlePythonException(Exception ex)
    {
        var message = ex.Message.ToLowerInvariant();

        if (message.Contains("encrypted"))
        {
            throw new PdfEncryptedException("PDF is encrypted and cannot be read without a password", ex);
        }

        if (message.Contains("corrupted") || message.Contains("pdf read error"))
        {
            throw new PdfCorruptedException("PDF file is corrupted or cannot be parsed", ex);
        }

        throw new PdfToolsException($"Error extracting text from PDF: {ex.Message}", ex);
    }

    /// <summary>
    /// Helper method to safely extract an integer value from a Python dictionary.
    /// </summary>
    private static int GetDictInt(PyObject dict, string key)
    {
        try
        {
            if (dict.HasAttr(key))
            {
                using var value = dict.GetAttr(key);
                return value.As<int>();
            }

            return 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Helper method to safely extract a boolean value from a Python dictionary.
    /// </summary>
    private static bool GetDictBool(PyObject dict, string key)
    {
        try
        {
            if (dict.HasAttr(key))
            {
                using var value = dict.GetAttr(key);
                return value.As<bool>();
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Helper method to safely extract a double value from a Python dictionary.
    /// </summary>
    private static double GetDictDouble(PyObject dict, string key)
    {
        try
        {
            if (dict.HasAttr(key))
            {
                using var value = dict.GetAttr(key);
                return value.As<double>();
            }

            return 0.0;
        }
        catch
        {
            return 0.0;
        }
    }

    /// <summary>
    /// Helper method to safely extract a string value from a Python dictionary.
    /// </summary>
    private static string? GetDictString(PyObject dict, string key)
    {
        try
        {
            if (dict.HasAttr(key))
            {
                using var value = dict.GetAttr(key);
                var str = value.As<string>();
                return string.IsNullOrEmpty(str) ? null : str;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
