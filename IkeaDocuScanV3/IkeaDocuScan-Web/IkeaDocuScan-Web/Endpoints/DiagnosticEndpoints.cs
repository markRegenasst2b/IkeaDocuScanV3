#if DEBUG
using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan_Web.Endpoints;

/// <summary>
/// Diagnostic endpoints for verifying database access (DEBUG mode only)
/// Uses dynamic database-driven authorization
/// </summary>
public static class DiagnosticEndpoints
{
    public static void MapDiagnosticEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/diagnostic")
            .RequireAuthorization()  // Base authentication required
            .WithTags("Diagnostic");

        // Test database connection
        group.MapGet("/db-connection", async ([FromServices] AppDbContext dbContext) =>
        {
            try
            {
                var canConnect = await dbContext.Database.CanConnectAsync();
                var connectionString = dbContext.Database.GetConnectionString();

                // Mask password in connection string
                var maskedConnectionString = connectionString != null
                    ? System.Text.RegularExpressions.Regex.Replace(connectionString, @"Password=([^;]+)", "Password=***")
                    : "N/A";

                return Results.Ok(new
                {
                    success = canConnect,
                    message = canConnect ? "Database connection successful" : "Cannot connect to database",
                    connectionString = maskedConnectionString,
                    databaseName = dbContext.Database.GetDbConnection().Database,
                    providerName = dbContext.Database.ProviderName
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new
                {
                    success = false,
                    message = "Database connection failed",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        })
        .WithName("TestDatabaseConnection")
        .RequireAuthorization("Endpoint:GET:/api/diagnostic/db-connection")
        .WithDescription("Test database connectivity");

        // Test EndpointRegistry table access
        group.MapGet("/endpoint-registry", async ([FromServices] AppDbContext dbContext) =>
        {
            try
            {
                var count = await dbContext.EndpointRegistries.CountAsync();
                var sample = await dbContext.EndpointRegistries
                    .OrderBy(e => e.EndpointId)
                    .Take(5)
                    .Select(e => new
                    {
                        e.EndpointId,
                        e.HttpMethod,
                        e.Route,
                        e.EndpointName,
                        e.Category,
                        e.IsActive
                    })
                    .ToListAsync();

                return Results.Ok(new
                {
                    success = true,
                    tableName = "EndpointRegistry",
                    totalCount = count,
                    sampleRecords = sample,
                    message = $"Successfully accessed EndpointRegistry table with {count} records"
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new
                {
                    success = false,
                    tableName = "EndpointRegistry",
                    message = "Failed to access EndpointRegistry table",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        })
        .WithName("TestEndpointRegistryAccess")
        .RequireAuthorization("Endpoint:GET:/api/diagnostic/endpoint-registry")
        .WithDescription("Test EndpointRegistry table access via EF Core");

        // Test EndpointRolePermission table access
        group.MapGet("/endpoint-role-permission", async ([FromServices] AppDbContext dbContext) =>
        {
            try
            {
                var count = await dbContext.EndpointRolePermissions.CountAsync();

                var roleDistribution = await dbContext.EndpointRolePermissions
                    .GroupBy(p => p.RoleName)
                    .Select(g => new
                    {
                        roleName = g.Key,
                        count = g.Count()
                    })
                    .OrderByDescending(x => x.count)
                    .ToListAsync();

                var sample = await dbContext.EndpointRolePermissions
                    .Include(p => p.Endpoint)
                    .OrderBy(p => p.PermissionId)
                    .Take(10)
                    .Select(p => new
                    {
                        p.PermissionId,
                        p.EndpointId,
                        p.RoleName,
                        endpointRoute = p.Endpoint.Route,
                        endpointMethod = p.Endpoint.HttpMethod
                    })
                    .ToListAsync();

                return Results.Ok(new
                {
                    success = true,
                    tableName = "EndpointRolePermission",
                    totalCount = count,
                    roleDistribution,
                    sampleRecords = sample,
                    message = $"Successfully accessed EndpointRolePermission table with {count} records"
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new
                {
                    success = false,
                    tableName = "EndpointRolePermission",
                    message = "Failed to access EndpointRolePermission table",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        })
        .WithName("TestEndpointRolePermissionAccess")
        .RequireAuthorization("Endpoint:GET:/api/diagnostic/endpoint-role-permission")
        .WithDescription("Test EndpointRolePermission table access via EF Core");

        // Test PermissionChangeAuditLog table access
        group.MapGet("/permission-audit-log", async ([FromServices] AppDbContext dbContext) =>
        {
            try
            {
                var count = await dbContext.PermissionChangeAuditLogs.CountAsync();
                var sample = await dbContext.PermissionChangeAuditLogs
                    .Include(a => a.Endpoint)
                    .OrderByDescending(a => a.ChangedOn)
                    .Take(5)
                    .Select(a => new
                    {
                        a.AuditId,
                        a.EndpointId,
                        endpointRoute = a.Endpoint.Route,
                        a.ChangedBy,
                        a.ChangeType,
                        a.ChangedOn
                    })
                    .ToListAsync();

                return Results.Ok(new
                {
                    success = true,
                    tableName = "PermissionChangeAuditLog",
                    totalCount = count,
                    sampleRecords = sample,
                    message = $"Successfully accessed PermissionChangeAuditLog table with {count} records"
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new
                {
                    success = false,
                    tableName = "PermissionChangeAuditLog",
                    message = "Failed to access PermissionChangeAuditLog table",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        })
        .WithName("TestPermissionAuditLogAccess")
        .RequireAuthorization("Endpoint:GET:/api/diagnostic/permission-audit-log")
        .WithDescription("Test PermissionChangeAuditLog table access via EF Core");

        // Test all authorization tables in one call
        group.MapGet("/all-tables", async ([FromServices] AppDbContext dbContext) =>
        {
            var results = new Dictionary<string, object>();

            // Test EndpointRegistry
            try
            {
                var endpointCount = await dbContext.EndpointRegistries.CountAsync();
                var categories = await dbContext.EndpointRegistries
                    .GroupBy(e => e.Category)
                    .Select(g => new { category = g.Key, count = g.Count() })
                    .OrderBy(x => x.category)
                    .ToListAsync();

                results["EndpointRegistry"] = new
                {
                    success = true,
                    totalCount = endpointCount,
                    categories
                };
            }
            catch (Exception ex)
            {
                results["EndpointRegistry"] = new
                {
                    success = false,
                    error = ex.Message
                };
            }

            // Test EndpointRolePermission
            try
            {
                var permissionCount = await dbContext.EndpointRolePermissions.CountAsync();
                var roles = await dbContext.EndpointRolePermissions
                    .GroupBy(p => p.RoleName)
                    .Select(g => new { roleName = g.Key, count = g.Count() })
                    .OrderBy(x => x.roleName)
                    .ToListAsync();

                results["EndpointRolePermission"] = new
                {
                    success = true,
                    totalCount = permissionCount,
                    roles
                };
            }
            catch (Exception ex)
            {
                results["EndpointRolePermission"] = new
                {
                    success = false,
                    error = ex.Message
                };
            }

            // Test PermissionChangeAuditLog
            try
            {
                var auditCount = await dbContext.PermissionChangeAuditLogs.CountAsync();
                results["PermissionChangeAuditLog"] = new
                {
                    success = true,
                    totalCount = auditCount
                };
            }
            catch (Exception ex)
            {
                results["PermissionChangeAuditLog"] = new
                {
                    success = false,
                    error = ex.Message
                };
            }

            var allSuccess = results.Values.All(v =>
            {
                var dict = v as Dictionary<string, object>;
                if (dict != null && dict.ContainsKey("success"))
                    return (bool)dict["success"];

                var type = v.GetType();
                var prop = type.GetProperty("success");
                return prop != null && (bool)prop.GetValue(v)!;
            });

            return Results.Ok(new
            {
                success = allSuccess,
                message = allSuccess
                    ? "All authorization tables are accessible via EF Core"
                    : "Some tables failed to access - see details",
                tables = results
            });
        })
        .WithName("TestAllAuthorizationTables")
        .RequireAuthorization("Endpoint:GET:/api/diagnostic/all-tables")
        .WithDescription("Test all authorization tables access via EF Core");

        // Test endpoint authorization service
        group.MapGet("/test-authorization-service", async (
            [FromServices] IEndpointAuthorizationService authService) =>
        {
            try
            {
                // Test GetAllowedRolesAsync
                var roles = await authService.GetAllowedRolesAsync("GET", "/api/documents/");

                // Test CheckAccessAsync
                var readerAccess = await authService.CheckAccessAsync("GET", "/api/documents/", new[] { "Reader" });
                var publisherAccess = await authService.CheckAccessAsync("POST", "/api/documents/", new[] { "Publisher" });
                var superUserAccess = await authService.CheckAccessAsync("DELETE", "/api/documents/{id}", new[] { "SuperUser" });

                // Test GetAllEndpointsAsync
                var allEndpoints = await authService.GetAllEndpointsAsync();

                return Results.Ok(new
                {
                    success = true,
                    message = "EndpointAuthorizationService is working correctly",
                    tests = new
                    {
                        getAllowedRoles = new
                        {
                            endpoint = "GET /api/documents/",
                            roles
                        },
                        checkAccess = new
                        {
                            readerCanGetDocuments = readerAccess,
                            publisherCanPostDocuments = publisherAccess,
                            superUserCanDeleteDocuments = superUserAccess
                        },
                        allEndpoints = new
                        {
                            totalCount = allEndpoints.Count,
                            categories = allEndpoints.GroupBy(e => e.Category)
                                .Select(g => new { category = g.Key, count = g.Count() })
                                .OrderBy(x => x.category)
                                .ToList()
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new
                {
                    success = false,
                    message = "EndpointAuthorizationService test failed",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        })
        .WithName("TestEndpointAuthorizationService")
        .RequireAuthorization("Endpoint:GET:/api/diagnostic/test-authorization-service")
        .WithDescription("Test EndpointAuthorizationService functionality");
    }
}
#endif
