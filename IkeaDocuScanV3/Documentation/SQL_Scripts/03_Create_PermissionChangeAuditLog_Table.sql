-- =============================================
-- Script: 03_Create_PermissionChangeAuditLog_Table.sql
-- Description: Creates the PermissionChangeAuditLog table for auditing permission changes
-- Date: 2025-01-17
-- Author: Role Extension Implementation
-- =============================================

USE [IkeaDocuScan]
GO

-- Drop table if exists (for development/testing only - UNCOMMENT CAREFULLY!)
-- IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PermissionChangeAuditLog]') AND type in (N'U'))
-- DROP TABLE [dbo].[PermissionChangeAuditLog]
-- GO

CREATE TABLE [dbo].[PermissionChangeAuditLog](
    [AuditId] INT IDENTITY(1,1) NOT NULL,
    [EndpointId] INT NOT NULL,
    [ChangedBy] VARCHAR(255) NOT NULL,
    [ChangeType] VARCHAR(50) NOT NULL,
    [OldValue] NVARCHAR(MAX) NULL,
    [NewValue] NVARCHAR(MAX) NULL,
    [ChangeReason] NVARCHAR(500) NULL,
    [ChangedOn] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_PermissionChangeAuditLog] PRIMARY KEY CLUSTERED ([AuditId] ASC),
    CONSTRAINT [FK_PermissionChangeAudit_Endpoint] FOREIGN KEY([EndpointId])
        REFERENCES [dbo].[EndpointRegistry] ([EndpointId]),
    CONSTRAINT [CHK_PermissionChangeAuditLog_ChangeType] CHECK ([ChangeType] IN ('RoleAdded', 'RoleRemoved', 'EndpointCreated', 'EndpointModified', 'EndpointDeactivated', 'EndpointReactivated'))
) ON [PRIMARY]
GO

-- Create indexes for performance
CREATE NONCLUSTERED INDEX [IX_PermissionChangeAuditLog_EndpointId]
ON [dbo].[PermissionChangeAuditLog] ([EndpointId] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_PermissionChangeAuditLog_ChangedOn]
ON [dbo].[PermissionChangeAuditLog] ([ChangedOn] DESC)
GO

CREATE NONCLUSTERED INDEX [IX_PermissionChangeAuditLog_ChangedBy]
ON [dbo].[PermissionChangeAuditLog] ([ChangedBy] ASC)
GO

PRINT 'PermissionChangeAuditLog table created successfully'
GO
