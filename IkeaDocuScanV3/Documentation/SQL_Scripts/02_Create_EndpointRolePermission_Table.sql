-- =============================================
-- Script: 02_Create_EndpointRolePermission_Table.sql
-- Description: Creates the EndpointRolePermission table for role-to-endpoint mappings
-- Date: 2025-01-17
-- Author: Role Extension Implementation
-- =============================================

USE [IkeaDocuScan]
GO

-- Drop table if exists (for development/testing only - UNCOMMENT CAREFULLY!)
-- IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EndpointRolePermission]') AND type in (N'U'))
-- DROP TABLE [dbo].[EndpointRolePermission]
-- GO

CREATE TABLE [dbo].[EndpointRolePermission](
    [PermissionId] INT IDENTITY(1,1) NOT NULL,
    [EndpointId] INT NOT NULL,
    [RoleName] VARCHAR(50) NOT NULL,
    [CreatedOn] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [CreatedBy] VARCHAR(255) NULL,
    CONSTRAINT [PK_EndpointRolePermission] PRIMARY KEY CLUSTERED ([PermissionId] ASC),
    CONSTRAINT [UK_EndpointRolePermission_EndpointRole] UNIQUE NONCLUSTERED ([EndpointId] ASC, [RoleName] ASC),
    CONSTRAINT [FK_EndpointRolePermission_Endpoint] FOREIGN KEY([EndpointId])
        REFERENCES [dbo].[EndpointRegistry] ([EndpointId])
        ON DELETE CASCADE,
    CONSTRAINT [CHK_EndpointRolePermission_RoleName] CHECK ([RoleName] IN ('Reader', 'Publisher', 'ADAdmin', 'SuperUser'))
) ON [PRIMARY]
GO

-- Create indexes for performance
CREATE NONCLUSTERED INDEX [IX_EndpointRolePermission_EndpointId]
ON [dbo].[EndpointRolePermission] ([EndpointId] ASC)
INCLUDE ([RoleName])
GO

CREATE NONCLUSTERED INDEX [IX_EndpointRolePermission_RoleName]
ON [dbo].[EndpointRolePermission] ([RoleName] ASC)
GO

PRINT 'EndpointRolePermission table created successfully'
GO
