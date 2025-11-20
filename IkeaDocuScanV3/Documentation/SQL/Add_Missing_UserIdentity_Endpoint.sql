-- Add missing /api/user/identity endpoint to EndpointRegistry
-- This endpoint was migrated but not yet in the database

-- First, check if the endpoint already exists
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE Route = '/api/user/identity' AND HttpMethod = 'GET')
BEGIN
    -- Insert into EndpointRegistry
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Category, IsActive, Description)
    VALUES ('GET', '/api/user/identity', 'GetUserIdentity', 'UserIdentity', 1, 'Get current user identity and claims');

    -- Get the EndpointId that was just inserted
    DECLARE @EndpointId INT = SCOPE_IDENTITY();

    -- Add permissions for all roles (this is a basic endpoint that all authenticated users should access)
    INSERT INTO EndpointRolePermission (EndpointId, RoleName)
    VALUES
        (@EndpointId, 'Reader'),
        (@EndpointId, 'Publisher'),
        (@EndpointId, 'ADAdmin'),
        (@EndpointId, 'SuperUser');

    PRINT 'Successfully added /api/user/identity endpoint with permissions for all roles';
END
ELSE
BEGIN
    PRINT 'Endpoint /api/user/identity already exists';
END
GO
