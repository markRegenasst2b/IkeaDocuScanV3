namespace IkeaDocuScan.Shared.Configuration;

/// <summary>
/// Configuration options for email notifications related to search results
/// Bound from appsettings.json "Email:SearchResults" section
/// </summary>
public class EmailSearchResultsOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "Email:SearchResults";

    /// <summary>
    /// Default recipient email address for search result emails
    /// </summary>
    public string DefaultRecipient { get; set; } = "legal@ikea.com";

    /// <summary>
    /// HTML email template for emails with attachments
    /// Placeholders: {DocumentCount}, {Barcodes}
    /// </summary>
    public string AttachEmailTemplate { get; set; } =
        "<!DOCTYPE html><html><body><p>Please find attached {DocumentCount} document(s):</p>{Barcodes}</body></html>";

    /// <summary>
    /// HTML template for individual barcode items in attachment emails
    /// Placeholders: {Barcode}, {DocumentName}, {DocumentType}
    /// </summary>
    public string AttachBarcodeItemTemplate { get; set; } =
        "<div>Barcode: {Barcode} - {DocumentName} ({DocumentType})</div>";

    /// <summary>
    /// HTML email template for emails with download links
    /// Placeholders: {DocumentCount}, {Links}
    /// </summary>
    public string LinkEmailTemplate { get; set; } =
        "<!DOCTYPE html><html><body><p>Access {DocumentCount} document(s):</p>{Links}</body></html>";

    /// <summary>
    /// HTML template for individual link items in link emails
    /// Placeholders: {Barcode}, {DocumentName}, {DocumentType}, {DownloadUrl}
    /// </summary>
    public string LinkItemTemplate { get; set; } =
        "<div>Barcode: {Barcode} - {DocumentName} ({DocumentType})<br><a href='{DownloadUrl}'>Download</a></div>";

    /// <summary>
    /// Validate configuration options
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(DefaultRecipient))
        {
            throw new InvalidOperationException("DefaultRecipient email address is required");
        }

        if (string.IsNullOrWhiteSpace(AttachEmailTemplate))
        {
            throw new InvalidOperationException("AttachEmailTemplate is required");
        }

        if (string.IsNullOrWhiteSpace(AttachBarcodeItemTemplate))
        {
            throw new InvalidOperationException("AttachBarcodeItemTemplate is required");
        }

        if (string.IsNullOrWhiteSpace(LinkEmailTemplate))
        {
            throw new InvalidOperationException("LinkEmailTemplate is required");
        }

        if (string.IsNullOrWhiteSpace(LinkItemTemplate))
        {
            throw new InvalidOperationException("LinkItemTemplate is required");
        }
    }

    /// <summary>
    /// Formats a single barcode item for attachment emails
    /// </summary>
    public string FormatBarcodeItem(int barcode, string documentName, string documentType)
    {
        return AttachBarcodeItemTemplate
            .Replace("{Barcode}", barcode.ToString())
            .Replace("{DocumentName}", documentName ?? "N/A")
            .Replace("{DocumentType}", documentType ?? "N/A");
    }

    /// <summary>
    /// Formats the complete HTML body for attachment emails
    /// </summary>
    public string FormatAttachEmail(int documentCount, string barcodeItems)
    {
        return AttachEmailTemplate
            .Replace("{DocumentCount}", documentCount.ToString())
            .Replace("{Barcodes}", barcodeItems);
    }

    /// <summary>
    /// Formats a single link item for link emails
    /// </summary>
    public string FormatLinkItem(int barcode, string documentName, string documentType, string downloadUrl)
    {
        return LinkItemTemplate
            .Replace("{Barcode}", barcode.ToString())
            .Replace("{DocumentName}", documentName ?? "N/A")
            .Replace("{DocumentType}", documentType ?? "N/A")
            .Replace("{DownloadUrl}", downloadUrl);
    }

    /// <summary>
    /// Formats the complete HTML body for link emails
    /// </summary>
    public string FormatLinkEmail(int documentCount, string linkItems)
    {
        return LinkEmailTemplate
            .Replace("{DocumentCount}", documentCount.ToString())
            .Replace("{Links}", linkItems);
    }
}
