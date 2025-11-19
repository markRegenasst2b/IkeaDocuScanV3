-- =============================================
-- Migration: AddConfigurationTables
-- Description: Creates configuration management tables
-- Generated: 2025-11-04
-- =============================================

-- Create SystemConfiguration table
CREATE TABLE [dbo].[SystemConfiguration] (
    [ConfigurationId] INT IDENTITY(1,1) NOT NULL,
    [ConfigKey] NVARCHAR(200) NOT NULL,
    [ConfigSection] NVARCHAR(100) NOT NULL,
    [ConfigValue] NVARCHAR(MAX) NOT NULL,
    [ValueType] NVARCHAR(50) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [IsActive] BIT NOT NULL,
    [IsOverride] BIT NOT NULL,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL CONSTRAINT DF_SystemConfiguration_CreatedDate DEFAULT GETUTCDATE(),
    [ModifiedBy] NVARCHAR(100) NULL,
    [ModifiedDate] DATETIME2 NULL,
    CONSTRAINT [PK_SystemConfiguration] PRIMARY KEY CLUSTERED ([ConfigurationId] ASC),
    CONSTRAINT [CK_SystemConfiguration_Section] CHECK (ConfigSection IN ('Email', 'ActionReminderService', 'General', 'System'))
);
GO

-- Create indexes for SystemConfiguration
CREATE UNIQUE NONCLUSTERED INDEX [IX_SystemConfiguration_ConfigKey]
ON [dbo].[SystemConfiguration]([ConfigKey] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_SystemConfiguration_Section_Active]
ON [dbo].[SystemConfiguration]([ConfigSection] ASC, [IsActive] ASC);
GO

-- Create SystemConfigurationAudit table
CREATE TABLE [dbo].[SystemConfigurationAudit] (
    [AuditId] INT IDENTITY(1,1) NOT NULL,
    [ConfigurationId] INT NOT NULL,
    [ConfigKey] NVARCHAR(200) NOT NULL,
    [OldValue] NVARCHAR(MAX) NULL,
    [NewValue] NVARCHAR(MAX) NULL,
    [ChangedBy] NVARCHAR(100) NOT NULL,
    [ChangedDate] DATETIME2 NOT NULL CONSTRAINT DF_SystemConfigurationAudit_ChangedDate DEFAULT GETUTCDATE(),
    [ChangeReason] NVARCHAR(500) NULL,
    CONSTRAINT [PK_SystemConfigurationAudit] PRIMARY KEY CLUSTERED ([AuditId] ASC),
    CONSTRAINT [FK_ConfigAudit_Config] FOREIGN KEY ([ConfigurationId])
        REFERENCES [dbo].[SystemConfiguration] ([ConfigurationId])
);
GO

-- Create indexes for SystemConfigurationAudit
CREATE NONCLUSTERED INDEX [IX_SystemConfigurationAudit_ConfigId]
ON [dbo].[SystemConfigurationAudit]([ConfigurationId] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_SystemConfigurationAudit_ChangedDate]
ON [dbo].[SystemConfigurationAudit]([ChangedDate] ASC);
GO

-- Create EmailTemplate table
CREATE TABLE [dbo].[EmailTemplate] (
    [TemplateId] INT IDENTITY(1,1) NOT NULL,
    [TemplateName] NVARCHAR(100) NOT NULL,
    [TemplateKey] NVARCHAR(100) NOT NULL,
    [Subject] NVARCHAR(500) NOT NULL,
    [HtmlBody] NVARCHAR(MAX) NOT NULL,
    [PlainTextBody] NVARCHAR(MAX) NULL,
    [PlaceholderDefinitions] NVARCHAR(MAX) NULL,
    [Category] NVARCHAR(50) NULL,
    [IsActive] BIT NOT NULL,
    [IsDefault] BIT NOT NULL,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL CONSTRAINT DF_EmailTemplate_CreatedDate DEFAULT GETUTCDATE(),
    [ModifiedBy] NVARCHAR(100) NULL,
    [ModifiedDate] DATETIME2 NULL,
    CONSTRAINT [PK_EmailTemplate] PRIMARY KEY CLUSTERED ([TemplateId] ASC)
);
GO

-- Create indexes for EmailTemplate
CREATE UNIQUE NONCLUSTERED INDEX [IX_EmailTemplate_Name]
ON [dbo].[EmailTemplate]([TemplateName] ASC);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_EmailTemplate_Key]
ON [dbo].[EmailTemplate]([TemplateKey] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_EmailTemplate_Key_Active]
ON [dbo].[EmailTemplate]([TemplateKey] ASC, [IsActive] ASC);
GO

-- Create EmailRecipientGroup table
CREATE TABLE [dbo].[EmailRecipientGroup] (
    [GroupId] INT IDENTITY(1,1) NOT NULL,
    [GroupName] NVARCHAR(100) NOT NULL,
    [GroupKey] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [IsActive] BIT NOT NULL,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL CONSTRAINT DF_EmailRecipientGroup_CreatedDate DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_EmailRecipientGroup] PRIMARY KEY CLUSTERED ([GroupId] ASC)
);
GO

-- Create indexes for EmailRecipientGroup
CREATE UNIQUE NONCLUSTERED INDEX [IX_EmailRecipientGroup_Name]
ON [dbo].[EmailRecipientGroup]([GroupName] ASC);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_EmailRecipientGroup_Key]
ON [dbo].[EmailRecipientGroup]([GroupKey] ASC);
GO

-- Create EmailRecipient table
CREATE TABLE [dbo].[EmailRecipient] (
    [RecipientId] INT IDENTITY(1,1) NOT NULL,
    [GroupId] INT NOT NULL,
    [EmailAddress] NVARCHAR(255) NOT NULL,
    [DisplayName] NVARCHAR(200) NULL,
    [IsActive] BIT NOT NULL,
    [SortOrder] INT NOT NULL,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL CONSTRAINT DF_EmailRecipient_CreatedDate DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_EmailRecipient] PRIMARY KEY CLUSTERED ([RecipientId] ASC),
    CONSTRAINT [FK_EmailRecipient_Group] FOREIGN KEY ([GroupId])
        REFERENCES [dbo].[EmailRecipientGroup] ([GroupId])
        ON DELETE CASCADE
);
GO

-- Create indexes for EmailRecipient
CREATE UNIQUE NONCLUSTERED INDEX [IX_EmailRecipient_Group_Email]
ON [dbo].[EmailRecipient]([GroupId] ASC, [EmailAddress] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_EmailRecipient_Group_Active]
ON [dbo].[EmailRecipient]([GroupId] ASC, [IsActive] ASC);
GO

-- Insert into __EFMigrationsHistory to mark migration as applied
-- Note: Only run this if you've manually applied the above DDL
-- INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
-- VALUES (N'20251104220432_AddConfigurationManagement', N'9.0.10');
-- GO

PRINT 'Configuration tables created successfully';
GO
