using System.Text;
using System.Text.RegularExpressions;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace IkeaDocuScan.Infrastructure.Services;

/// <summary>
/// Service for rendering email templates with placeholder and loop support
/// Supports placeholders like {{Username}}, {{Count}}, {{Date}}
/// Supports loops like {{#ActionRows}}...{{/ActionRows}}
/// </summary>
public class EmailTemplateService : IEmailTemplateService
{
    private readonly ILogger<EmailTemplateService> _logger;

    // Regex patterns for placeholders and loops
    private static readonly Regex PlaceholderRegex = new(@"\{\{([^#/][^\}]*)\}\}", RegexOptions.Compiled);
    private static readonly Regex LoopStartRegex = new(@"\{\{#(\w+)\}\}", RegexOptions.Compiled);
    private static readonly Regex LoopEndRegex = new(@"\{\{/(\w+)\}\}", RegexOptions.Compiled);

    public EmailTemplateService(ILogger<EmailTemplateService> logger)
    {
        _logger = logger;
    }

    public string RenderTemplate(string template, Dictionary<string, object> data)
    {
        if (string.IsNullOrEmpty(template))
        {
            _logger.LogWarning("Attempted to render empty template");
            return string.Empty;
        }

        try
        {
            var result = template;

            // Replace all placeholders
            result = PlaceholderRegex.Replace(result, match =>
            {
                var placeholder = match.Groups[1].Value.Trim();

                if (data.TryGetValue(placeholder, out var value))
                {
                    return FormatValue(value);
                }

                _logger.LogWarning("Placeholder {{{{{{{{0}}}}}}}} not found in data dictionary", placeholder);
                return match.Value; // Keep original placeholder if not found
            });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template");
            return template; // Return original template on error
        }
    }

    public string RenderTemplateWithLoops(string template, Dictionary<string, object> data, Dictionary<string, List<Dictionary<string, object>>> loops)
    {
        if (string.IsNullOrEmpty(template))
        {
            _logger.LogWarning("Attempted to render empty template");
            return string.Empty;
        }

        try
        {
            var result = template;

            // Process loops first
            foreach (var loop in loops)
            {
                result = RenderLoop(result, loop.Key, loop.Value);
            }

            // Then replace simple placeholders
            result = RenderTemplate(result, data);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template with loops");
            return template;
        }
    }

    public List<string> ExtractPlaceholders(string template)
    {
        if (string.IsNullOrEmpty(template))
        {
            return new List<string>();
        }

        var placeholders = new List<string>();

        var matches = PlaceholderRegex.Matches(template);
        foreach (Match match in matches)
        {
            var placeholder = match.Groups[1].Value.Trim();
            if (!placeholders.Contains(placeholder))
            {
                placeholders.Add(placeholder);
            }
        }

        // Also extract loop names
        var loopMatches = LoopStartRegex.Matches(template);
        foreach (Match match in loopMatches)
        {
            var loopName = match.Groups[1].Value;
            if (!placeholders.Contains(loopName))
            {
                placeholders.Add(loopName);
            }
        }

        return placeholders;
    }

    public bool ValidateTemplate(string template, Dictionary<string, object> data)
    {
        if (string.IsNullOrEmpty(template))
        {
            _logger.LogWarning("Template is empty");
            return false;
        }

        try
        {
            // Check for balanced braces
            var openCount = template.Count(c => c == '{');
            var closeCount = template.Count(c => c == '}');
            if (openCount != closeCount)
            {
                _logger.LogWarning("Template has mismatched braces. Open: {Open}, Close: {Close}", openCount, closeCount);
                return false;
            }

            // Check for balanced loops
            var loopStarts = LoopStartRegex.Matches(template);
            var loopEnds = LoopEndRegex.Matches(template);

            if (loopStarts.Count != loopEnds.Count)
            {
                _logger.LogWarning("Template has mismatched loop tags. Starts: {Starts}, Ends: {Ends}", loopStarts.Count, loopEnds.Count);
                return false;
            }

            // Check that loop names match
            for (int i = 0; i < loopStarts.Count; i++)
            {
                var startName = loopStarts[i].Groups[1].Value;
                var endName = loopEnds[i].Groups[1].Value;
                if (startName != endName)
                {
                    _logger.LogWarning("Loop names don't match: {Start} vs {End}", startName, endName);
                    return false;
                }
            }

            // Extract placeholders and check if critical ones are provided
            var placeholders = ExtractPlaceholders(template);
            var missingPlaceholders = placeholders.Where(p => !data.ContainsKey(p)).ToList();

            if (missingPlaceholders.Any())
            {
                _logger.LogInformation("Template has placeholders without data: {Placeholders}", string.Join(", ", missingPlaceholders));
                // This is just a warning, not a validation failure
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating template");
            return false;
        }
    }

    #region Private Helper Methods

    private string RenderLoop(string template, string loopName, List<Dictionary<string, object>> items)
    {
        try
        {
            // Find loop boundaries
            var startPattern = $@"\{{\{{#(loopName)\}}}}\";
            var endPattern = $@"\{{\{{/(loopName)\}}}}\";

            var startMatch = Regex.Match(template, startPattern);
            var endMatch = Regex.Match(template, endPattern);

            if (!startMatch.Success || !endMatch.Success)
            {
                _logger.LogWarning("Loop markers not found for: {LoopName}", loopName);
                return template;
            }

            var startIndex = startMatch.Index + startMatch.Length;
            var endIndex = endMatch.Index;
            var loopTemplate = template.Substring(startIndex, endIndex - startIndex);

            var renderedRows = new StringBuilder();

            // Render each item
            foreach (var item in items)
            {
                var row = loopTemplate;
                foreach (var kvp in item)
                {
                    var placeholder = $"{{{{{{{kvp.Key}}}}}}}";
                    row = row.Replace(placeholder, FormatValue(kvp.Value));
                }
                renderedRows.Append(row);
            }

            // Replace loop section with rendered content
            var before = template.Substring(0, startMatch.Index);
            var after = template.Substring(endMatch.Index + endMatch.Length);
            var result = before + renderedRows.ToString() + after;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering loop: {LoopName}", loopName);
            return template;
        }
    }

    private string FormatValue(object value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        // Format dates
        if (value is DateTime dateTime)
        {
            return dateTime.ToString("dd/MM/yyyy HH:mm");
        }

        // Format dates (date only)
        if (value is DateOnly dateOnly)
        {
            return dateOnly.ToString("dd/MM/yyyy");
        }

        // Format numbers with thousand separators
        if (value is int || value is long)
        {
            return string.Format("{0:N0}", value);
        }

        // Format decimals
        if (value is decimal || value is double || value is float)
        {
            return string.Format("{0:N2}", value);
        }

        // Format booleans
        if (value is bool boolValue)
        {
            return boolValue ? "Yes" : "No";
        }

        // Default string conversion
        return value.ToString() ?? string.Empty;
    }

    #endregion
}
