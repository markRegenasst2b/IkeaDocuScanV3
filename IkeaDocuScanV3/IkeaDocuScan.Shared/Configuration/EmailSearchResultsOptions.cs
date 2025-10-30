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
    /// Subject line template for emails with attachments
    /// Placeholders: {DocumentCount}
    /// </summary>
    public string AttachSubjectTemplate { get; set; } = "IKEA Document(s): {DocumentCount} file(s)";

    /// <summary>
    /// Email body template for emails with attachments
    /// Placeholders: {DocumentCount}, {Barcodes}
    /// </summary>
    public string AttachBodyTemplate { get; set; } =
        "Please find attached {DocumentCount} document(s) with the following barcodes:\n\n{Barcodes}\n\nBest regards,\nIKEA DocuScan System";

    /// <summary>
    /// Subject line template for emails with download links
    /// Placeholders: {DocumentCount}
    /// </summary>
    public string LinkSubjectTemplate { get; set; } = "IKEA Document Links: {DocumentCount} file(s)";

    /// <summary>
    /// Email body template for emails with download links
    /// Placeholders: {DocumentCount}, {Links}
    /// </summary>
    public string LinkBodyTemplate { get; set; } =
        "You can access the following {DocumentCount} document(s):\n\n{Links}\n\nBest regards,\nIKEA DocuScan System";

    /// <summary>
    /// Validate configuration options
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(DefaultRecipient))
        {
            throw new InvalidOperationException("DefaultRecipient email address is required");
        }

        if (string.IsNullOrWhiteSpace(AttachSubjectTemplate))
        {
            throw new InvalidOperationException("AttachSubjectTemplate is required");
        }

        if (string.IsNullOrWhiteSpace(AttachBodyTemplate))
        {
            throw new InvalidOperationException("AttachBodyTemplate is required");
        }

        if (string.IsNullOrWhiteSpace(LinkSubjectTemplate))
        {
            throw new InvalidOperationException("LinkSubjectTemplate is required");
        }

        if (string.IsNullOrWhiteSpace(LinkBodyTemplate))
        {
            throw new InvalidOperationException("LinkBodyTemplate is required");
        }
    }

    /// <summary>
    /// Formats the subject line for attachment emails
    /// </summary>
    public string FormatAttachSubject(int documentCount)
    {
        return AttachSubjectTemplate.Replace("{DocumentCount}", documentCount.ToString());
    }

    /// <summary>
    /// Formats the body for attachment emails
    /// </summary>
    public string FormatAttachBody(int documentCount, string barcodes)
    {
        return AttachBodyTemplate
            .Replace("{DocumentCount}", documentCount.ToString())
            .Replace("{Barcodes}", barcodes);
    }

    /// <summary>
    /// Formats the subject line for link emails
    /// </summary>
    public string FormatLinkSubject(int documentCount)
    {
        return LinkSubjectTemplate.Replace("{DocumentCount}", documentCount.ToString());
    }

    /// <summary>
    /// Formats the body for link emails
    /// </summary>
    public string FormatLinkBody(int documentCount, string links)
    {
        return LinkBodyTemplate
            .Replace("{DocumentCount}", documentCount.ToString())
            .Replace("{Links}", links);
    }
}
