USE [IkeaDocumentScanningCH]
GO

/****** Object:  StoredProcedure [dbo].[InsertToAuditTrail]    Script Date: 23.10.2025 12:53:04 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[InsertToAuditTrail]
(
	@User VARCHAR(128),
	@Action VARCHAR(128),
	@Details VARCHAR(500),
	@BarCode VARCHAR(10)
)
AS
	INSERT INTO AuditTrail ([Timestamp], [User], [Action], [Details], [BarCode])
	VALUES (GETDATE(), @User, @Action, @Details, @BarCode)

GO


