using IkeaDocuScan.Shared.DTOs.Email;
using System.Net.Http.Json;

namespace IkeaDocuScan_Web.Client.Services;

/// <summary>
/// HTTP client service for sending emails
/// </summary>
public class EmailHttpService
{
    private readonly HttpClient _http;
    private readonly ILogger<EmailHttpService> _logger;

    public EmailHttpService(HttpClient http, ILogger<EmailHttpService> logger)
    {
        _http = http;
        _logger = logger;
    }

    /// <summary>
    /// Send a custom email with HTML body
    /// </summary>
    public async Task<bool> SendEmailAsync(SendEmailRequest request)
    {
        try
        {
            _logger.LogInformation("Sending email to {ToEmail} via API", request.ToEmail);

            var response = await _http.PostAsJsonAsync("/api/email/send", request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully to {ToEmail}", request.ToEmail);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send email. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {ToEmail}", request.ToEmail);
            throw;
        }
    }

    /// <summary>
    /// Send email with document attachments
    /// </summary>
    public async Task<bool> SendEmailWithAttachmentsAsync(SendEmailWithAttachmentsRequest request)
    {
        try
        {
            _logger.LogInformation("Sending email with {Count} attachments to {ToEmail} via API",
                request.DocumentIds.Count, request.ToEmail);

            var response = await _http.PostAsJsonAsync("/api/email/send-with-attachments", request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email with attachments sent successfully to {ToEmail}", request.ToEmail);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send email with attachments. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email with attachments to {ToEmail}", request.ToEmail);
            throw;
        }
    }
}
