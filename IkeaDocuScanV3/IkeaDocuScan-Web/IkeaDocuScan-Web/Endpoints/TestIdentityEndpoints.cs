#if DEBUG
using IkeaDocuScan.Shared.DTOs.Testing;
using IkeaDocuScan_Web.Services;

namespace IkeaDocuScan_Web.Endpoints;

/// <summary>
/// API endpoints for test identity management (DEVELOPMENT ONLY)
/// Uses dynamic database-driven authorization
/// ⚠️ WARNING: THESE ENDPOINTS ONLY EXIST IN DEBUG MODE ⚠️
/// </summary>
public static class TestIdentityEndpoints
{
    public static void MapTestIdentityEndpoints(this IEndpointRouteBuilder routes)
    {
#if DEBUG
        var group = routes.MapGroup("/api/test-identity")
            .RequireAuthorization()  // Base authentication required
            .WithTags("TestIdentity (DEBUG ONLY)");

        // Get all available test identity profiles
        group.MapGet("/profiles", (TestIdentityService service) =>
        {
            var profiles = service.GetAvailableProfiles();
            return Results.Ok(profiles);
        })
        .WithName("GetTestIdentityProfiles")
        .RequireAuthorization("Endpoint:GET:/api/test-identity/profiles")
        .Produces<List<TestIdentityProfile>>(200);

        // Get current test identity status
        group.MapGet("/status", (TestIdentityService service) =>
        {
            var status = service.GetStatus();
            return Results.Ok(status);
        })
        .WithName("GetTestIdentityStatus")
        .RequireAuthorization("Endpoint:GET:/api/test-identity/status")
        .Produces<TestIdentityStatus>(200);

        // Set active test identity
        group.MapPost("/activate/{profileId}", (string profileId, TestIdentityService service) =>
        {
            try
            {
                service.SetTestIdentity(profileId);
                return Results.Ok(new { success = true, message = $"Test identity '{profileId}' activated" });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: 500,
                    title: "Error activating test identity");
            }
        })
        .WithName("ActivateTestIdentity")
        .RequireAuthorization("Endpoint:POST:/api/test-identity/activate/{profileId}")
        .Produces(200)
        .Produces(400)
        .Produces(500);

        // Reset to real identity
        group.MapPost("/reset", (TestIdentityService service) =>
        {
            service.SetTestIdentity("reset");
            return Results.Ok(new { success = true, message = "Test identity removed" });
        })
        .WithName("ResetTestIdentity")
        .RequireAuthorization("Endpoint:POST:/api/test-identity/reset")
        .Produces(200);
#endif
    }
}
#endif
