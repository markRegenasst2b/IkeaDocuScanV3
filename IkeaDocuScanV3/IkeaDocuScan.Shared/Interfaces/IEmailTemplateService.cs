namespace IkeaDocuScan.Shared.Interfaces;

/// <summary>
/// Service for rendering email templates with placeholder support
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Render email template with provided data
    /// </summary>
    /// <param name="template">HTML template with placeholders</param>
    /// <param name="data">Dictionary of placeholder values</param>
    /// <returns>Rendered HTML string</returns>
    string RenderTemplate(string template, Dictionary<string, object> data);

    /// <summary>
    /// Render email template with loop support (for tables)
    /// </summary>
    /// <param name="template">HTML template with placeholders and loops</param>
    /// <param name="data">Dictionary of placeholder values</param>
    /// <param name="loops">Dictionary of loop data (key = loop name, value = list of items)</param>
    /// <returns>Rendered HTML string</returns>
    string RenderTemplateWithLoops(string template, Dictionary<string, object> data, Dictionary<string, List<Dictionary<string, object>>> loops);

    /// <summary>
    /// Get list of placeholders in a template
    /// </summary>
    /// <param name="template">Template string</param>
    /// <returns>List of placeholder names</returns>
    List<string> ExtractPlaceholders(string template);

    /// <summary>
    /// Validate that all required placeholders are provided
    /// </summary>
    /// <param name="template">Template string</param>
    /// <param name="data">Data dictionary</param>
    /// <returns>True if valid, false otherwise</returns>
    bool ValidateTemplate(string template, Dictionary<string, object> data);
}
