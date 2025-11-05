using IkeaDocuScan.Shared.DTOs.UserPermissions;
using IkeaDocuScan.Shared.Interfaces;
using System.Net.Http.Json;

namespace IkeaDocuScan_Web.Client.Services;

/// <summary>
/// HTTP client service for UserPermission operations
/// Implements IUserPermissionService interface to call server APIs
/// </summary>
public class UserPermissionHttpService : IUserPermissionService
{
    private readonly HttpClient _http;
    private readonly ILogger<UserPermissionHttpService> _logger;

    public UserPermissionHttpService(HttpClient http, ILogger<UserPermissionHttpService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<DocuScanUserDto>> GetAllUsersAsync(string? accountNameFilter = null)
    {
        try
        {
            _logger.LogInformation("Fetching all DocuScan users from API");
            var url = string.IsNullOrWhiteSpace(accountNameFilter)
                ? "/api/userpermissions/users"
                : $"/api/userpermissions/users?accountNameFilter={Uri.EscapeDataString(accountNameFilter)}";

            var result = await _http.GetFromJsonAsync<List<DocuScanUserDto>>(url);
            return result ?? new List<DocuScanUserDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching DocuScan users");
            throw;
        }
    }

    public async Task<List<UserPermissionDto>> GetAllAsync(string? accountNameFilter = null)
    {
        try
        {
            _logger.LogInformation("Fetching all user permissions from API");
            var url = string.IsNullOrWhiteSpace(accountNameFilter)
                ? "/api/userpermissions"
                : $"/api/userpermissions?accountNameFilter={Uri.EscapeDataString(accountNameFilter)}";

            var result = await _http.GetFromJsonAsync<List<UserPermissionDto>>(url);
            return result ?? new List<UserPermissionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user permissions");
            throw;
        }
    }

    public async Task<UserPermissionDto?> GetByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Fetching user permission {Id} from API", id);
            return await _http.GetFromJsonAsync<UserPermissionDto>($"/api/userpermissions/{id}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("User permission {Id} not found", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user permission {Id}", id);
            throw;
        }
    }

    public async Task<List<UserPermissionDto>> GetByUserIdAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Fetching user permissions for user {UserId} from API", userId);
            var result = await _http.GetFromJsonAsync<List<UserPermissionDto>>($"/api/userpermissions/user/{userId}");
            return result ?? new List<UserPermissionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user permissions for user {UserId}", userId);
            throw;
        }
    }

    public async Task<UserPermissionDto> CreateAsync(CreateUserPermissionDto dto)
    {
        try
        {
            _logger.LogInformation("Creating new user permission via API");
            var response = await _http.PostAsJsonAsync("/api/userpermissions", dto);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<UserPermissionDto>();
            return result!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user permission");
            throw;
        }
    }

    public async Task<UserPermissionDto> UpdateAsync(UpdateUserPermissionDto dto)
    {
        try
        {
            _logger.LogInformation("Updating user permission {Id} via API", dto.Id);
            var response = await _http.PutAsJsonAsync($"/api/userpermissions/{dto.Id}", dto);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<UserPermissionDto>();
            return result!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user permission {Id}", dto.Id);
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting user permission {Id} via API", id);
            var response = await _http.DeleteAsync($"/api/userpermissions/{id}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user permission {Id}", id);
            throw;
        }
    }

    public async Task DeleteUserAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Deleting user {UserId} and all their permissions via API", userId);
            var response = await _http.DeleteAsync($"/api/userpermissions/user/{userId}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            throw;
        }
    }

    public async Task<DocuScanUserDto> CreateUserAsync(CreateDocuScanUserDto dto)
    {
        try
        {
            _logger.LogInformation("Creating new DocuScan user via API");
            var response = await _http.PostAsJsonAsync("/api/userpermissions/user", dto);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<DocuScanUserDto>();
            return result!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating DocuScan user");
            throw;
        }
    }

    public async Task<DocuScanUserDto> UpdateUserAsync(UpdateDocuScanUserDto dto)
    {
        try
        {
            _logger.LogInformation("Updating DocuScan user {UserId} via API", dto.UserId);
            var response = await _http.PutAsJsonAsync($"/api/userpermissions/user/{dto.UserId}", dto);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<DocuScanUserDto>();
            return result!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating DocuScan user {UserId}", dto.UserId);
            throw;
        }
    }
}
