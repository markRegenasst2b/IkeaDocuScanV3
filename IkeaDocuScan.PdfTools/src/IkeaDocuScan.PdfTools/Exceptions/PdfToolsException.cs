namespace IkeaDocuScan.PdfTools.Exceptions;

/// <summary>
/// Exception thrown when an error occurs during PDF processing operations.
/// </summary>
public class PdfToolsException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PdfToolsException"/> class.
    /// </summary>
    public PdfToolsException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfToolsException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public PdfToolsException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfToolsException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public PdfToolsException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a PDF file is encrypted and cannot be read without a password.
/// </summary>
public class PdfEncryptedException : PdfToolsException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PdfEncryptedException"/> class.
    /// </summary>
    public PdfEncryptedException() : base("PDF is encrypted and cannot be read without a password")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfEncryptedException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public PdfEncryptedException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfEncryptedException"/> class with a specified error message
    /// and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public PdfEncryptedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a PDF file is corrupted or cannot be read.
/// </summary>
public class PdfCorruptedException : PdfToolsException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PdfCorruptedException"/> class.
    /// </summary>
    public PdfCorruptedException() : base("PDF file is corrupted or cannot be read")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfCorruptedException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public PdfCorruptedException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfCorruptedException"/> class with a specified error message
    /// and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public PdfCorruptedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
