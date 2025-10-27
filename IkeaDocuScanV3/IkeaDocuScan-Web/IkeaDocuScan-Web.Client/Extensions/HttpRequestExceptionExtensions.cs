using System.Net.Http.Json;

namespace IkeaDocuScan_Web.Client.Extensions;

/// <summary>
/// Extension methods for HttpRequestException to extract error messages
/// </summary>
public static class HttpRequestExceptionExtensions
{
    /// <summary>
    /// Extract error message from HTTP response
    /// </summary>
    public static async Task<string> GetErrorMessageAsync(this HttpRequestException ex)
    {
        if (ex.Data.Contains("ResponseContent"))
        {
            var content = ex.Data["ResponseContent"]?.ToString();
            if (!string.IsNullOrEmpty(content))
            {
                try
                {
                    var errorResponse = System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(content);
                    if (errorResponse?.Error != null)
                    {
                        return errorResponse.Error;
                    }
                }
                catch
                {
                    // If deserialization fails, return raw content
                    return content;
                }
            }
        }

        return ex.Message;
    }

    private class ErrorResponse
    {
        public string? Error { get; set; }
    }
}
