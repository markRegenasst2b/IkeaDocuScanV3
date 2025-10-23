USE [IkeaDocumentScanningCH]
GO
/****** Object:  Table [dbo].[AuditTrail]    Script Date: 23.10.2025 12:54:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AuditTrail](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Timestamp] [datetime] NOT NULL,
	[User] [varchar](128) NOT NULL,
	[Action] [varchar](128) NOT NULL,
	[Details] [varchar](2500) NULL,
	[BarCode] [varchar](10) NOT NULL,
 CONSTRAINT [PK__AuditTrail__4AB81AF0] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CounterParty]    Script Date: 23.10.2025 12:54:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CounterParty](
	[CounterPartyId] [int] IDENTITY(1,1) NOT NULL,
	[CounterPartyNo] [int] NOT NULL,
	[Name] [varchar](128) NULL,
	[Since] [datetime] NOT NULL,
	[Address] [varchar](255) NULL,
	[DisplayAtCheckIn] [bit] NOT NULL,
	[Comments] [varchar](255) NULL,
	[Country] [char](2) NOT NULL,
	[City] [varchar](128) NOT NULL,
	[AffiliatedTo] [varchar](128) NULL,
	[CounterPartyNoAlpha] [varchar](32) NULL,
 CONSTRAINT [PK__CounterParty__3B75D760] PRIMARY KEY CLUSTERED 
(
	[CounterPartyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CounterParty20150928]    Script Date: 23.10.2025 12:54:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CounterParty20150928](
	[CounterPartyId] [int] IDENTITY(1,1) NOT NULL,
	[CounterPartyNo] [int] NOT NULL,
	[Name] [varchar](128) NULL,
	[Since] [datetime] NOT NULL,
	[Address] [varchar](255) NULL,
	[DisplayAtCheckIn] [bit] NOT NULL,
	[Comments] [varchar](255) NULL,
	[Country] [char](2) NOT NULL,
	[City] [varchar](128) NOT NULL,
	[AffiliatedTo] [varchar](128) NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CounterPartyRelation]    Script Date: 23.10.2025 12:54:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CounterPartyRelation](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ParentCounterPartyNo] [int] NULL,
	[ChildCounterPartyNo] [int] NULL,
	[RelationType] [int] NULL,
	[Comment] [varchar](255) NULL,
	[ParentCounterPartyNoAlpha] [varchar](32) NULL,
	[ChildCounterPartyNoAlpha] [varchar](32) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CounterPartyRelationType]    Script Date: 23.10.2025 12:54:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CounterPartyRelationType](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Country]    Script Date: 23.10.2025 12:54:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Country](
	[CountryCode] [char](2) NOT NULL,
	[Name] [varchar](128) NULL,
PRIMARY KEY CLUSTERED 
(
	[CountryCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Currency]    Script Date: 23.10.2025 12:54:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Currency](
	[CurrencyCode] [char](3) NOT NULL,
	[Name] [varchar](128) NULL,
	[DecimalPlaces] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[CurrencyCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Document]    Script Date: 23.10.2025 12:54:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Document](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[BarCode] [int] NOT NULL,
	[DT_ID] [int] NULL,
	[CounterPartyId] [int] NULL,
	[DocumentNameId] [int] NULL,
	[FileId] [int] NULL,
	[DateOfContract] [datetime] NULL,
	[Comment] [varchar](255) NULL,
	[ReceivingDate] [datetime] NULL,
	[DispatchDate] [datetime] NULL,
	[Fax] [bit] NULL,
	[OriginalReceived] [bit] NULL,
	[ActionDate] [datetime] NULL,
	[ActionDescription] [varchar](255) NULL,
	[ReminderGroup] [varchar](255) NULL,
	[DocumentNo] [varchar](255) NULL,
	[AssociatedToPUA] [varchar](255) NULL,
	[VersionNo] [varchar](255) NULL,
	[AssociatedToAppendix] [varchar](255) NULL,
	[ValidUntil] [datetime] NULL,
	[CurrencyCode] [char](3) NULL,
	[Amount] [decimal](18, 0) NULL,
	[Authorisation] [varchar](255) NULL,
	[BankConfirmation] [bit] NULL,
	[TranslatedVersionReceived] [bit] NULL,
	[Confidential] [bit] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [varchar](255) NOT NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](255) NULL,
	[ThirdParty] [varchar](255) NULL,
	[ThirdPartyId] [varchar](255) NULL,
	[SendingOutDate] [datetime] NULL,
	[ForwardedToSignatoriesDate] [datetime] NULL,
 CONSTRAINT [DOCUMENT_PK] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [uk_document_barcode] UNIQUE NONCLUSTERED 
(
	[BarCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DocumentFile]    Script Date: 23.10.2025 12:54:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DocumentFile](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FileName] [varchar](900) NOT NULL,
	[FileType] [varchar](20) NOT NULL,
	[Bytes] [varbinary](max) NULL,
 CONSTRAINT [DOCUMENTFILE_PK] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [uk_file_filename] UNIQUE NONCLUSTERED 
(
	[FileName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DocumentName]    Script Date: 23.10.2025 12:54:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DocumentName](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NULL,
	[DocumentTypeId] [int] NULL,
 CONSTRAINT [PK__DocumentName__47DBAE45] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DocumentType]    Script Date: 23.10.2025 12:54:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DocumentType](
	[DT_ID] [int] NOT NULL,
	[DT_Name] [varchar](255) NOT NULL,
	[BarCode] [char](1) NOT NULL,
	[CounterParty] [char](1) NOT NULL,
	[DateOfContract] [char](1) NOT NULL,
	[Comment] [char](1) NOT NULL,
	[ReceivingDate] [char](1) NOT NULL,
	[DispatchDate] [char](1) NOT NULL,
	[Fax] [char](1) NOT NULL,
	[OriginalReceived] [char](1) NOT NULL,
	[DocumentNo] [char](1) NOT NULL,
	[AssociatedToPUA] [char](1) NOT NULL,
	[VersionNo] [char](1) NOT NULL,
	[AssociatedToAppendix] [char](1) NOT NULL,
	[ValidUntil] [char](1) NOT NULL,
	[Currency] [char](1) NOT NULL,
	[Amount] [char](1) NOT NULL,
	[Authorisation] [char](1) NOT NULL,
	[BankConfirmation] [char](1) NOT NULL,
	[TranslatedVersionReceived] [char](1) NOT NULL,
	[ActionDate] [char](1) NOT NULL,
	[ActionDescription] [char](1) NOT NULL,
	[ReminderGroup] [char](1) NOT NULL,
	[TemplatePath] [varchar](500) NULL,
	[Confidential] [char](1) NOT NULL,
	[IsAppendix] [bit] NOT NULL,
	[IsEnabled] [bit] NOT NULL,
	[CounterPartyAlpha] [char](1) NOT NULL,
	[SendingOutDate] [char](1) NOT NULL,
	[ForwardedToSignatoriesDate] [char](1) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[DT_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[OldCounterParty]    Script Date: 23.10.2025 12:54:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OldCounterParty](
	[CounterPartyNo] [int] NOT NULL,
	[Name] [varchar](128) NULL,
	[Country] [char](2) NOT NULL,
	[City] [varchar](128) NOT NULL,
	[AffiliatedTo] [varchar](128) NULL,
PRIMARY KEY CLUSTERED 
(
	[CounterPartyNo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Tabelle1]    Script Date: 23.10.2025 12:54:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tabelle1](
	[ID] [int] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [aaaaaTabelle1_PK] PRIMARY KEY NONCLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Tabelle1_remote]    Script Date: 23.10.2025 12:54:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tabelle1_remote](
	[ID] [int] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [aaaaaTabelle1_remote_PK] PRIMARY KEY NONCLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[UserPermissions]    Script Date: 23.10.2025 12:54:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserPermissions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AccountName] [varchar](255) NOT NULL,
	[DocumentTypeId] [int] NULL,
	[CounterPartyId] [int] NULL,
	[CountryCode] [char](2) NULL,
 CONSTRAINT [USERACCOUNT_PK] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[CounterParty] ADD  CONSTRAINT [DF_CounterParty_CounterPartyNo]  DEFAULT ((-1)) FOR [CounterPartyNo]
GO
ALTER TABLE [dbo].[CounterParty] ADD  CONSTRAINT [DF_CounterParty_DisplayAtCheckIn]  DEFAULT ((1)) FOR [DisplayAtCheckIn]
GO
ALTER TABLE [dbo].[CounterParty] ADD  CONSTRAINT [DF_CounterParty_CounterPartyNoAlpha]  DEFAULT ('') FOR [CounterPartyNoAlpha]
GO
ALTER TABLE [dbo].[Currency] ADD  CONSTRAINT [DF_Currency_DecimalPlaces]  DEFAULT (2) FOR [DecimalPlaces]
GO
ALTER TABLE [dbo].[DocumentFile] ADD  DEFAULT ('.pdf') FOR [FileType]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_BarCode]  DEFAULT ('M') FOR [BarCode]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_CounterParty]  DEFAULT ('M') FOR [CounterParty]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_DateOfContract]  DEFAULT ('M') FOR [DateOfContract]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_Comment]  DEFAULT ('O') FOR [Comment]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_ReceivingDate]  DEFAULT ('M') FOR [ReceivingDate]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_DispatchDate]  DEFAULT ('M') FOR [DispatchDate]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_Fax]  DEFAULT ('M') FOR [Fax]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_OriginalReceived]  DEFAULT ('M') FOR [OriginalReceived]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_DocumentNo]  DEFAULT ('N') FOR [DocumentNo]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_AssociatedToPUA]  DEFAULT ('N') FOR [AssociatedToPUA]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_VersionNo]  DEFAULT ('N') FOR [VersionNo]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_AssociatedToAppendix]  DEFAULT ('N') FOR [AssociatedToAppendix]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_ValidUntil]  DEFAULT ('N') FOR [ValidUntil]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_Currency]  DEFAULT ('N') FOR [Currency]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_Amount]  DEFAULT ('N') FOR [Amount]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_Authorisation]  DEFAULT ('N') FOR [Authorisation]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_BankConfirmation]  DEFAULT ('N') FOR [BankConfirmation]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_TranslatedVersionReceived]  DEFAULT ('M') FOR [TranslatedVersionReceived]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_ActionDate]  DEFAULT ('O') FOR [ActionDate]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_ActionDescription]  DEFAULT ('O') FOR [ActionDescription]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_ReminderGroup]  DEFAULT ('O') FOR [ReminderGroup]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_Confidential]  DEFAULT ('M') FOR [Confidential]
GO
ALTER TABLE [dbo].[DocumentType] ADD  CONSTRAINT [DF_DocumentType_IsAppendix]  DEFAULT ((0)) FOR [IsAppendix]
GO
ALTER TABLE [dbo].[DocumentType] ADD  DEFAULT ((1)) FOR [IsEnabled]
GO
ALTER TABLE [dbo].[DocumentType] ADD  DEFAULT ('M') FOR [CounterPartyAlpha]
GO
ALTER TABLE [dbo].[DocumentType] ADD  DEFAULT ('O') FOR [SendingOutDate]
GO
ALTER TABLE [dbo].[DocumentType] ADD  DEFAULT ('O') FOR [ForwardedToSignatoriesDate]
GO
ALTER TABLE [dbo].[CounterParty]  WITH CHECK ADD  CONSTRAINT [FK__CounterPa__Count__3D5E1FD2] FOREIGN KEY([Country])
REFERENCES [dbo].[Country] ([CountryCode])
GO
ALTER TABLE [dbo].[CounterParty] CHECK CONSTRAINT [FK__CounterPa__Count__3D5E1FD2]
GO
ALTER TABLE [dbo].[CounterPartyRelation]  WITH CHECK ADD FOREIGN KEY([RelationType])
REFERENCES [dbo].[CounterPartyRelationType] ([ID])
GO
ALTER TABLE [dbo].[Document]  WITH CHECK ADD  CONSTRAINT [FK__Document__Counte__5EBF139D] FOREIGN KEY([CounterPartyId])
REFERENCES [dbo].[CounterParty] ([CounterPartyId])
GO
ALTER TABLE [dbo].[Document] CHECK CONSTRAINT [FK__Document__Counte__5EBF139D]
GO
ALTER TABLE [dbo].[Document]  WITH CHECK ADD FOREIGN KEY([CurrencyCode])
REFERENCES [dbo].[Currency] ([CurrencyCode])
GO
ALTER TABLE [dbo].[Document]  WITH CHECK ADD  CONSTRAINT [FK__Document__Docume__5FB337D6] FOREIGN KEY([DocumentNameId])
REFERENCES [dbo].[DocumentName] ([ID])
GO
ALTER TABLE [dbo].[Document] CHECK CONSTRAINT [FK__Document__Docume__5FB337D6]
GO
ALTER TABLE [dbo].[Document]  WITH CHECK ADD FOREIGN KEY([DT_ID])
REFERENCES [dbo].[DocumentType] ([DT_ID])
GO
ALTER TABLE [dbo].[Document]  WITH CHECK ADD  CONSTRAINT [FK__Document__FileId__5AEE82B9] FOREIGN KEY([FileId])
REFERENCES [dbo].[DocumentFile] ([Id])
GO
ALTER TABLE [dbo].[Document] CHECK CONSTRAINT [FK__Document__FileId__5AEE82B9]
GO
ALTER TABLE [dbo].[Document]  WITH CHECK ADD FOREIGN KEY([FileId])
REFERENCES [dbo].[DocumentFile] ([Id])
GO
ALTER TABLE [dbo].[DocumentName]  WITH NOCHECK ADD  CONSTRAINT [FK__DocumentN__Docum__48CFD27E] FOREIGN KEY([DocumentTypeId])
REFERENCES [dbo].[DocumentType] ([DT_ID])
GO
ALTER TABLE [dbo].[DocumentName] CHECK CONSTRAINT [FK__DocumentN__Docum__48CFD27E]
GO
ALTER TABLE [dbo].[OldCounterParty]  WITH CHECK ADD FOREIGN KEY([Country])
REFERENCES [dbo].[Country] ([CountryCode])
GO
ALTER TABLE [dbo].[UserPermissions]  WITH CHECK ADD  CONSTRAINT [FK__UserPermi__Count__6A30C649] FOREIGN KEY([CounterPartyId])
REFERENCES [dbo].[CounterParty] ([CounterPartyId])
GO
ALTER TABLE [dbo].[UserPermissions] CHECK CONSTRAINT [FK__UserPermi__Count__6A30C649]
GO
ALTER TABLE [dbo].[UserPermissions]  WITH CHECK ADD FOREIGN KEY([CountryCode])
REFERENCES [dbo].[Country] ([CountryCode])
GO
ALTER TABLE [dbo].[UserPermissions]  WITH CHECK ADD FOREIGN KEY([DocumentTypeId])
REFERENCES [dbo].[DocumentType] ([DT_ID])
GO
EXEC sys.sp_addextendedproperty @name=N'AllowZeroLength', @value=N'False' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Tabelle1_remote', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'AppendOnly', @value=N'False' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Tabelle1_remote', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'Attributes', @value=N'17' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Tabelle1_remote', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'CollatingOrder', @value=N'1033' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Tabelle1_remote', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'DataUpdatable', @value=N'False' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Tabelle1_remote', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'Name', @value=N'ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Tabelle1_remote', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'OrdinalPosition', @value=N'1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Tabelle1_remote', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'Required', @value=N'True' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Tabelle1_remote', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'Size', @value=N'4' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Tabelle1_remote', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'SourceField', @value=N'ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Tabelle1_remote', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'SourceTable', @value=N'Tabelle1_remote' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Tabelle1_remote', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'Type', @value=N'4' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Tabelle1_remote', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'Attributes', @value=N'536870912' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Tabelle1_remote'
GO
EXEC sys.sp_addextendedproperty @name=N'Connect', @value=N'ODBC;DSN=DocuScanCH;Trusted_Connection=Yes;APP=2007 Microsoft Office system;DATABASE=IkeaDocumentScanningCH;AutoTranslate=No;UseProcForPrepare=0' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Tabelle1_remote'
GO
EXEC sys.sp_addextendedproperty @name=N'DateCreated', @value=N'13.06.2014 09:57:55' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Tabelle1_remote'
GO
EXEC sys.sp_addextendedproperty @name=N'LastUpdated', @value=N'13.06.2014 09:57:56' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Tabelle1_remote'
GO
EXEC sys.sp_addextendedproperty @name=N'Name', @value=N'Tabelle1_remote' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Tabelle1_remote'
GO
EXEC sys.sp_addextendedproperty @name=N'RecordCount', @value=N'-1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Tabelle1_remote'
GO
EXEC sys.sp_addextendedproperty @name=N'SourceTableName', @value=N'dbo.Tabelle1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Tabelle1_remote'
GO
EXEC sys.sp_addextendedproperty @name=N'Updatable', @value=N'False' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Tabelle1_remote'
GO
