-- =============================================
-- Script: Create DocuScanUser Table
-- Purpose: Create a separate user table to store user information
--          including account name, user ID, last logon timestamp,
--          and superuser flag
-- =============================================

USE PPDOCUSCAN;
GO

-- Create DocuScanUser table
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE ID = object_id(N'[dbo].[DocuScanUser]')
	AND OBJECTPROPERTY(ID, N'IsTable') = 1)
BEGIN
    CREATE TABLE [dbo].[DocuScanUser]
    (
        [UserId] INT IDENTITY NOT NULL CONSTRAINT DOCUSCANUSER_PK PRIMARY KEY,
        [AccountName] VARCHAR(255) NOT NULL,
        [LastLogon] DATETIME NULL,
        [IsSuperUser] BIT NOT NULL CONSTRAINT DF_DocuScanUser_IsSuperUser DEFAULT(0),
        [CreatedOn] DATETIME NOT NULL CONSTRAINT DF_DocuScanUser_CreatedOn DEFAULT(GETDATE()),
        [ModifiedOn] DATETIME NULL,

        CONSTRAINT UK_DocuScanUser_AccountName UNIQUE ([AccountName])
    );

    PRINT 'DocuScanUser table created successfully.';
END
ELSE
BEGIN
    PRINT 'DocuScanUser table already exists.';
END
GO

-- Create index on LastLogon for performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_DocuScanUser_LastLogon')
BEGIN
    CREATE INDEX IX_DocuScanUser_LastLogon
    ON [dbo].[DocuScanUser] ([LastLogon]);

    PRINT 'Index on LastLogon created successfully.';
END
GO

-- Create index on IsSuperUser for filtering
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_DocuScanUser_IsSuperUser')
BEGIN
    CREATE INDEX IX_DocuScanUser_IsSuperUser
    ON [dbo].[DocuScanUser] ([IsSuperUser]);

    PRINT 'Index on IsSuperUser created successfully.';
END
GO

PRINT 'Script 04_Create_DocuScanUser_Table.sql completed successfully.';
GO
