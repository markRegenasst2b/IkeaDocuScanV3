using Microsoft.AspNetCore.Authorization;

namespace IkeaDocuScan_Web.Authorization;

/// <summary>
/// Authorization requirement for user access
/// </summary>
public class UserAccessRequirement : IAuthorizationRequirement
{
    // Empty requirement - handler will check HasAccess claim
}

/// <summary>
/// Authorization requirement for super user access
/// </summary>
public class SuperUserRequirement : IAuthorizationRequirement
{
    // Empty requirement - handler will check IsSuperUser claim
}
