-- =============================================
-- Script: 01_Create_EndpointRegistry_Table.sql
-- Description: Creates the EndpointRegistry table for cataloging all API endpoints
-- Date: 2025-01-17
-- Author: Role Extension Implementation
-- =============================================

USE [IkeaDocuScan]
GO

-- Drop table if exists (for development/testing only - UNCOMMENT CAREFULLY!)
-- IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EndpointRegistry]') AND type in (N'U'))
-- DROP TABLE [dbo].[EndpointRegistry]
-- GO

CREATE TABLE [dbo].[EndpointRegistry](
    [EndpointId] INT IDENTITY(1,1) NOT NULL,
    [HttpMethod] VARCHAR(10) NOT NULL,
    [Route] VARCHAR(500) NOT NULL,
    [EndpointName] VARCHAR(200) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [Category] VARCHAR(100) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedOn] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [ModifiedOn] DATETIME2(7) NULL,
    CONSTRAINT [PK_EndpointRegistry] PRIMARY KEY CLUSTERED ([EndpointId] ASC),
    CONSTRAINT [UK_EndpointRegistry_Method_Route] UNIQUE NONCLUSTERED ([HttpMethod] ASC, [Route] ASC)
) ON [PRIMARY]
GO

-- Create indexes for performance
CREATE NONCLUSTERED INDEX [IX_EndpointRegistry_Category]
ON [dbo].[EndpointRegistry] ([Category] ASC)
WHERE [IsActive] = 1
GO

CREATE NONCLUSTERED INDEX [IX_EndpointRegistry_IsActive]
ON [dbo].[EndpointRegistry] ([IsActive] ASC)
GO

PRINT 'EndpointRegistry table created successfully'
GO
