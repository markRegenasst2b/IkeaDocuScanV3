using IkeaDocuScan.Shared.DTOs;
using System.Net.Http.Json;

namespace IkeaDocuScan_Web.Client.Services;

/// <summary>
/// HTTP client service for managing endpoint authorization permissions
/// Used by SuperUser for administering role-based endpoint access
/// </summary>
public class EndpointManagementHttpService
{
    private readonly HttpClient _http;
    private readonly ILogger<EndpointManagementHttpService> _logger;

    public EndpointManagementHttpService(
        HttpClient http,
        ILogger<EndpointManagementHttpService> logger)
    {
        _http = http;
        _logger = logger;
    }

    /// <summary>
    /// Get all endpoints with their role permissions
    /// </summary>
    public async Task<List<EndpointRegistryDto>> GetAllEndpointsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all endpoints from API");

            // Add timeout to prevent hanging
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            var result = await _http.GetFromJsonAsync<List<EndpointRegistryDto>>(
                "/api/endpoint-authorization/endpoints",
                cts.Token);

            return result ?? new List<EndpointRegistryDto>();
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timed out fetching endpoints");
            throw new TimeoutException("Request timed out while fetching endpoints. Please check your connection.", ex);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            _logger.LogError(ex, "Permission denied fetching endpoints");
            throw new UnauthorizedAccessException("You do not have permission to view endpoints. SuperUser role required.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching endpoints");
            throw;
        }
    }

    /// <summary>
    /// Update the roles that have access to a specific endpoint
    /// </summary>
    /// <param name="endpointId">Endpoint ID</param>
    /// <param name="dto">Update DTO containing new role list, changed by, and reason</param>
    public async Task UpdateEndpointRolesAsync(int endpointId, UpdateEndpointRolesDto dto)
    {
        try
        {
            _logger.LogInformation(
                "Updating roles for endpoint {EndpointId}. New roles: {Roles}",
                endpointId,
                string.Join(", ", dto.RoleNames));

            // Add timeout to prevent hanging
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            var response = await _http.PostAsJsonAsync(
                $"/api/endpoint-authorization/endpoints/{endpointId}/roles",
                dto,
                cts.Token);

            // Check for specific error status codes and read error message
            if (!response.IsSuccessStatusCode)
            {
                string? errorMessage = null;
                try
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cts.Token);
                    _logger.LogWarning("Error response from server: {ErrorContent}", errorContent);

                    // Try to parse as JSON error object
                    if (!string.IsNullOrWhiteSpace(errorContent))
                    {
                        try
                        {
                            var errorObj = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(errorContent);
                            if (errorObj != null && errorObj.ContainsKey("error"))
                            {
                                errorMessage = errorObj["error"]?.ToString();
                            }
                        }
                        catch
                        {
                            // If not JSON, use raw content
                            errorMessage = errorContent;
                        }
                    }
                }
                catch
                {
                    // Ignore error reading content
                }

                // Throw specific exceptions based on status code
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.NotFound:
                        throw new InvalidOperationException($"Endpoint with ID {endpointId} not found");

                    case System.Net.HttpStatusCode.BadRequest:
                        throw new InvalidOperationException(errorMessage ?? "Invalid request - validation failed");

                    case System.Net.HttpStatusCode.Forbidden:
                        throw new UnauthorizedAccessException(errorMessage ?? "You do not have permission to update endpoint roles. SuperUser role required.");

                    case System.Net.HttpStatusCode.Unauthorized:
                        throw new UnauthorizedAccessException("Not authenticated. Please log in.");

                    default:
                        throw new HttpRequestException($"Server returned error: {response.StatusCode} - {errorMessage ?? response.ReasonPhrase}");
                }
            }

            _logger.LogInformation("Successfully updated roles for endpoint {EndpointId}", endpointId);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timed out updating endpoint {EndpointId}", endpointId);
            throw new TimeoutException($"Request timed out while updating endpoint {endpointId}. Please try again.", ex);
        }
        catch (UnauthorizedAccessException)
        {
            // Re-throw authorization exceptions as-is
            throw;
        }
        catch (InvalidOperationException)
        {
            // Re-throw validation exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating roles for endpoint {EndpointId}", endpointId);
            throw new InvalidOperationException($"Unexpected error updating endpoint: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Validate a permission change before applying it
    /// </summary>
    /// <param name="dto">Validation request containing endpoint ID and new role list</param>
    /// <returns>Validation result with any errors</returns>
    public async Task<ValidatePermissionChangeResult> ValidatePermissionChangeAsync(
        ValidatePermissionChangeDto dto)
    {
        try
        {
            _logger.LogInformation(
                "Validating permission change for endpoint {EndpointId}",
                dto.EndpointId);

            // Create a cancellation token with timeout to prevent hanging
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            var response = await _http.PostAsJsonAsync(
                "/api/endpoint-authorization/validate",
                dto,
                cts.Token);

            // Check for authorization failures
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                _logger.LogWarning("Permission denied for validation endpoint");
                return new ValidatePermissionChangeResult
                {
                    IsValid = false,
                    ValidationErrors = new List<string> { "Permission denied: You do not have access to validate permissions" }
                };
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("User not authenticated for validation endpoint");
                return new ValidatePermissionChangeResult
                {
                    IsValid = false,
                    ValidationErrors = new List<string> { "Not authenticated: Please log in" }
                };
            }

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ValidatePermissionChangeResult>(
                cancellationToken: cts.Token);

            return result ?? new ValidatePermissionChangeResult
            {
                IsValid = false,
                ValidationErrors = new List<string> { "Invalid response from server" }
            };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Validation request timed out");
            return new ValidatePermissionChangeResult
            {
                IsValid = false,
                ValidationErrors = new List<string> { "Request timed out. Please check your connection and try again." }
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error validating permission change: {StatusCode}", ex.StatusCode);
            return new ValidatePermissionChangeResult
            {
                IsValid = false,
                ValidationErrors = new List<string> { $"Connection error: {ex.Message}" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating permission change");
            return new ValidatePermissionChangeResult
            {
                IsValid = false,
                ValidationErrors = new List<string> { $"Validation failed: {ex.Message}" }
            };
        }
    }

    /// <summary>
    /// Get audit log for permission changes
    /// </summary>
    /// <param name="endpointId">Optional filter by endpoint ID</param>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <returns>List of audit log entries</returns>
    public async Task<List<PermissionChangeAuditLogDto>> GetAuditLogAsync(
        int? endpointId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        try
        {
            var queryParams = new List<string>();

            if (endpointId.HasValue)
                queryParams.Add($"endpointId={endpointId.Value}");

            if (fromDate.HasValue)
                queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-ddTHH:mm:ss}");

            if (toDate.HasValue)
                queryParams.Add($"toDate={toDate.Value:yyyy-MM-ddTHH:mm:ss}");

            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : string.Empty;
            var url = $"/api/endpoint-authorization/audit{queryString}";

            _logger.LogInformation("Fetching audit log from API: {Url}", url);

            var result = await _http.GetFromJsonAsync<List<PermissionChangeAuditLogDto>>(url);
            return result ?? new List<PermissionChangeAuditLogDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching audit log");
            throw;
        }
    }

    /// <summary>
    /// Invalidate the authorization cache after making permission changes
    /// </summary>
    public async Task InvalidateCacheAsync()
    {
        try
        {
            _logger.LogInformation("Invalidating authorization cache");

            var response = await _http.PostAsync(
                "/api/endpoint-authorization/cache/invalidate",
                null);

            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Successfully invalidated authorization cache");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error invalidating cache (non-critical)");
            // Don't throw - cache invalidation failure is not critical
        }
    }
}

/// <summary>
/// Extension methods for HttpRequestException
/// </summary>
internal static class HttpRequestExceptionExtensions
{
    /// <summary>
    /// Extract error message from HTTP response content
    /// </summary>
    public static async Task<string?> GetErrorMessageAsync(this HttpRequestException ex)
    {
        try
        {
            // Try to read error message from response content if available
            // This is a simplified version - actual implementation may vary
            return ex.Message;
        }
        catch
        {
            return null;
        }
    }
}
