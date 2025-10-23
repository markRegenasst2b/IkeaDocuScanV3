USE [IkeaDocumentScanningCH]
GO

/****** Object:  StoredProcedure [dbo].[GetSimilarRegistrations]    Script Date: 23.10.2025 12:49:25 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		A.Willimann (T2B AG)
-- Create date: January 2021
-- Description:	Returns the list of barcodes for a given set of (dtid, documentNo, versionNo)
-- =============================================
CREATE PROCEDURE [dbo].[GetSimilarRegistrations] 
	@pDTId int,
	@pDocumentNo varchar(255),
	@pVersionNo varchar(255)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @documentNo varchar(255) = isnull(@pDocumentNo,'');
	declare @versionNo varchar(255) = isnull(@pVersionNo,'');

	SELECT barcode 
	FROM [dbo].[Document]
	where dt_id=@pDTId and isnull(DocumentNo, '') = @documentNo and isnull(VersionNo, '') = @versionNo;

END

/*
-- test it
exec dbo.GetSimilarRegistrations 11, '101515', '';
exec dbo.GetSimilarRegistrations 11, '101515', null;
exec dbo.GetSimilarRegistrations 1,'00108018','65.0';
*/


GO

