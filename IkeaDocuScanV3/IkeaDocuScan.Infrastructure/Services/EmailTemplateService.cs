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
            _logger.LogDebug("RenderTemplate called with template length: {Length}, data keys: {Keys}",
                template.Length, string.Join(", ", data.Keys));

            var result = template;
            var replacementCount = 0;

#if DEBUG
            data["TestEnvironmentIndicator"]= "<span style=\"color:red; font-weight:bold;\">[TEST ENVIRONMENT]</span></br>";
#else
            data["TestEnvironmentIndicator"] = string.Empty;
#endif


            // Replace all placeholders
            result = PlaceholderRegex.Replace(result, match =>
            {
                var placeholder = match.Groups[1].Value.Trim();
                _logger.LogDebug("Found placeholder: {{{{{Placeholder}}}}}", placeholder);

                if (data.TryGetValue(placeholder, out var value))
                {
                    var formattedValue = FormatValue(value);
                    _logger.LogDebug("Replacing {{{{{Placeholder}}}}} with: {Value}", placeholder, formattedValue);
                    replacementCount++;
                    return formattedValue;
                }

                _logger.LogWarning("Placeholder {{{{{Placeholder}}}}} not found in data dictionary. Available keys: {Keys}",
                    placeholder, string.Join(", ", data.Keys));
                return match.Value; // Keep original placeholder if not found
            });

            _logger.LogInformation("Template rendering complete. Replaced {Count} placeholder(s)", replacementCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template. Template length: {Length}", template.Length);
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
            _logger.LogDebug("RenderTemplateWithLoops called. Template length: {Length}, Loop count: {LoopCount}, Data keys: {DataKeys}",
                template.Length, loops.Count, string.Join(", ", data.Keys));

            var result = template;

            // Process loops first
            foreach (var loop in loops)
            {
                _logger.LogDebug("Processing loop: {LoopName} with {ItemCount} items", loop.Key, loop.Value.Count);
                result = RenderLoop(result, loop.Key, loop.Value);
            }

            _logger.LogDebug("All loops processed. Template length after loops: {Length}", result.Length);

            // Then replace simple placeholders
            result = RenderTemplate(result, data);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template with loops. Template length: {Length}, Loop names: {LoopNames}",
                template.Length, string.Join(", ", loops.Keys));
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
            _logger.LogDebug("RenderLoop called for: {LoopName} with {ItemCount} items", loopName, items.Count);

            // Find loop boundaries - FIXED: Use actual loopName variable, not literal "(loopName)"
            var startPattern = $@"\{{\{{#{loopName}\}}\}}";
            var endPattern = $@"\{{\{{/{loopName}\}}\}}";

            _logger.LogDebug("Looking for loop markers: Start={{{{#{LoopName}}}}}, End={{{{/{LoopName}}}}}", loopName, loopName);

            var startMatch = Regex.Match(template, startPattern);
            var endMatch = Regex.Match(template, endPattern);

            if (!startMatch.Success || !endMatch.Success)
            {
                _logger.LogWarning("Loop markers not found for: {LoopName}. StartMatch: {StartFound}, EndMatch: {EndFound}",
                    loopName, startMatch.Success, endMatch.Success);

                // Check if the markers exist at all in the template
                if (template.Contains($"{{{{#{loopName}}}}}"))
                {
                    _logger.LogWarning("Start marker {{{{#{LoopName}}}}}} exists in template but regex didn't match!", loopName);
                }
                if (template.Contains($"{{{{/{loopName}}}}}"))
                {
                    _logger.LogWarning("End marker {{{{/{LoopName}}}}}} exists in template but regex didn't match!", loopName);
                }

                return template;
            }

            _logger.LogDebug("Loop markers found. Start index: {StartIndex}, End index: {EndIndex}",
                startMatch.Index, endMatch.Index);

            var startIndex = startMatch.Index + startMatch.Length;
            var endIndex = endMatch.Index;
            var loopTemplate = template.Substring(startIndex, endIndex - startIndex);

            _logger.LogDebug("Loop template extracted. Length: {Length}, Content preview: {Preview}",
                loopTemplate.Length, loopTemplate.Length > 100 ? loopTemplate.Substring(0, 100) + "..." : loopTemplate);

            var renderedRows = new StringBuilder();
            var rowCount = 0;

            // Render each item
            foreach (var item in items)
            {
                var row = loopTemplate;
                var replacementCount = 0;

                foreach (var kvp in item)
                {
                    var placeholder = $"{{{{{kvp.Key}}}}}";
                    if (row.Contains(placeholder))
                    {
                        row = row.Replace(placeholder, FormatValue(kvp.Value));
                        replacementCount++;
                    }
                }

                renderedRows.Append(row);
                rowCount++;

                if (rowCount == 1)
                {
                    _logger.LogDebug("First row rendered with {ReplacementCount} placeholder replacements", replacementCount);
                }
            }

            _logger.LogInformation("Rendered {RowCount} rows for loop {LoopName}. Total output length: {Length}",
                rowCount, loopName, renderedRows.Length);

            // Replace loop section with rendered content
            var before = template.Substring(0, startMatch.Index);
            var after = template.Substring(endMatch.Index + endMatch.Length);
            var result = before + renderedRows.ToString() + after;

            _logger.LogDebug("Loop rendering complete. Result length: {Length} (was {OriginalLength})",
                result.Length, template.Length);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering loop: {LoopName}. Template length: {Length}, Items count: {Count}",
                loopName, template.Length, items.Count);
            return template;
        }
    }

    private string FormatValue(object value)
    {
        try
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
            var result = value.ToString() ?? string.Empty;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error formatting value of type {Type}", value?.GetType().Name ?? "null");
            return string.Empty;
        }
    }

    #endregion
}
