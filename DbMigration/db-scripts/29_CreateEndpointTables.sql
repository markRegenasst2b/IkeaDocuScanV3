USE IkeaDocuScan
GO


	CREATE TABLE [dbo].[EndpointRegistry](
		[EndpointId] [int] IDENTITY(1,1) NOT NULL,
		[HttpMethod] [varchar](10) NOT NULL,
		[Route] [varchar](500) NOT NULL,
		[EndpointName] [varchar](200) NOT NULL,
		[Description] [nvarchar](500) NULL,
		[Category] [varchar](100) NULL,
		[IsActive] [bit] NOT NULL,
		[CreatedOn] [datetime2](7) NOT NULL,
		[ModifiedOn] [datetime2](7) NULL,
	 CONSTRAINT [PK_EndpointRegistry] PRIMARY KEY CLUSTERED 
	(
		[EndpointId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	 CONSTRAINT [UK_EndpointRegistry_Method_Route] UNIQUE NONCLUSTERED 
	(
		[HttpMethod] ASC,
		[Route] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]
	GO

	ALTER TABLE [dbo].[EndpointRegistry] ADD  DEFAULT ((1)) FOR [IsActive]
	GO

	ALTER TABLE [dbo].[EndpointRegistry] ADD  DEFAULT (getdate()) FOR [CreatedOn]
	GO


	CREATE TABLE [dbo].[EndpointRolePermission](
		[PermissionId] [int] IDENTITY(1,1) NOT NULL,
		[EndpointId] [int] NOT NULL,
		[RoleName] [varchar](50) NOT NULL,
		[CreatedOn] [datetime2](7) NOT NULL,
		[CreatedBy] [varchar](255) NULL,
	 CONSTRAINT [PK_EndpointRolePermission] PRIMARY KEY CLUSTERED 
	(
		[PermissionId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
	 CONSTRAINT [UK_EndpointRolePermission_EndpointRole] UNIQUE NONCLUSTERED 
	(
		[EndpointId] ASC,
		[RoleName] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]
	GO

	ALTER TABLE [dbo].[EndpointRolePermission] ADD  DEFAULT (getdate()) FOR [CreatedOn]
	GO

	ALTER TABLE [dbo].[EndpointRolePermission]  WITH CHECK ADD  CONSTRAINT [FK_EndpointRolePermission_Endpoint] FOREIGN KEY([EndpointId])
	REFERENCES [dbo].[EndpointRegistry] ([EndpointId])
	ON DELETE CASCADE
	GO

	ALTER TABLE [dbo].[EndpointRolePermission] CHECK CONSTRAINT [FK_EndpointRolePermission_Endpoint]
	GO

	ALTER TABLE [dbo].[EndpointRolePermission]  WITH CHECK ADD  CONSTRAINT [CHK_EndpointRolePermission_RoleName] CHECK  (([RoleName]='SuperUser' OR [RoleName]='ADAdmin' OR [RoleName]='Publisher' OR [RoleName]='Reader'))
	GO

	ALTER TABLE [dbo].[EndpointRolePermission] CHECK CONSTRAINT [CHK_EndpointRolePermission_RoleName]
	GO



	CREATE TABLE [dbo].[PermissionChangeAuditLog](
		[AuditId] [int] IDENTITY(1,1) NOT NULL,
		[EndpointId] [int] NOT NULL,
		[ChangedBy] [varchar](255) NOT NULL,
		[ChangeType] [varchar](50) NOT NULL,
		[OldValue] [nvarchar](max) NULL,
		[NewValue] [nvarchar](max) NULL,
		[ChangeReason] [nvarchar](500) NULL,
		[ChangedOn] [datetime2](7) NOT NULL,
	 CONSTRAINT [PK_PermissionChangeAuditLog] PRIMARY KEY CLUSTERED 
	(
		[AuditId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
	GO

	ALTER TABLE [dbo].[PermissionChangeAuditLog] ADD  DEFAULT (getdate()) FOR [ChangedOn]
	GO

	ALTER TABLE [dbo].[PermissionChangeAuditLog]  WITH CHECK ADD  CONSTRAINT [FK_PermissionChangeAudit_Endpoint] FOREIGN KEY([EndpointId])
	REFERENCES [dbo].[EndpointRegistry] ([EndpointId])
	GO

	ALTER TABLE [dbo].[PermissionChangeAuditLog] CHECK CONSTRAINT [FK_PermissionChangeAudit_Endpoint]
	GO

	ALTER TABLE [dbo].[PermissionChangeAuditLog]  WITH CHECK ADD  CONSTRAINT [CHK_PermissionChangeAuditLog_ChangeType] CHECK  (([ChangeType]='EndpointReactivated' OR [ChangeType]='EndpointDeactivated' OR [ChangeType]='EndpointModified' OR [ChangeType]='EndpointCreated' OR [ChangeType]='RoleRemoved' OR [ChangeType]='RoleAdded'))
	GO

	ALTER TABLE [dbo].[PermissionChangeAuditLog] CHECK CONSTRAINT [CHK_PermissionChangeAuditLog_ChangeType]
	GO


