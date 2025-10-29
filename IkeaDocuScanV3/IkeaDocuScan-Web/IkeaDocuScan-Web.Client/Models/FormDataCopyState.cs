using System.Text.Json;

namespace IkeaDocuScan_Web.Client.Models;

/// <summary>
/// Represents copied form data for Copy/Paste functionality.
/// Stored in browser localStorage with expiration.
/// </summary>
public class FormDataCopyState
{
    /// <summary>
    /// Storage key for localStorage
    /// Version 2: Excludes FileBytes, SourceFilePath, Mode, PropertySetNumber, FieldConfig, Id, and audit fields
    /// </summary>
    public const string LocalStorageKey = "documentFormCopy_v2";

    /// <summary>
    /// Expiration time in days
    /// </summary>
    public const int ExpirationDays = 10;

    /// <summary>
    /// Timestamp when data was copied
    /// </summary>
    public DateTime CopiedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Serialized DocumentPropertiesViewModel JSON
    /// </summary>
    public string ViewModelJson { get; set; } = string.Empty;

    /// <summary>
    /// User who copied the data (for audit/display)
    /// </summary>
    public string? CopiedBy { get; set; }

    /// <summary>
    /// Checks if the copied data has expired
    /// </summary>
    public bool IsExpired => DateTime.Now.Subtract(CopiedAt).TotalDays > ExpirationDays;

    /// <summary>
    /// Serializes this state to JSON for localStorage
    /// </summary>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = false
        });
    }

    /// <summary>
    /// Deserializes from JSON stored in localStorage
    /// </summary>
    public static FormDataCopyState? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<FormDataCopyState>(json);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Creates a new copy state from a ViewModel
    /// </summary>
    public static FormDataCopyState Create(DocumentPropertiesViewModel viewModel, string copiedBy)
    {
        return new FormDataCopyState
        {
            CopiedAt = DateTime.Now,
            CopiedBy = copiedBy,
            ViewModelJson = JsonSerializer.Serialize(viewModel, new JsonSerializerOptions
            {
                WriteIndented = false
            })
        };
    }

    /// <summary>
    /// Extracts the ViewModel from this copy state
    /// </summary>
    public DocumentPropertiesViewModel? GetViewModel()
    {
        if (string.IsNullOrWhiteSpace(ViewModelJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<DocumentPropertiesViewModel>(ViewModelJson);
        }
        catch
        {
            return null;
        }
    }
}
