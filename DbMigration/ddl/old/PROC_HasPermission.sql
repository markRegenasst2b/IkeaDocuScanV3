USE [IkeaDocumentScanningCH]
GO

/****** Object:  StoredProcedure [dbo].[HasPermission]    Script Date: 23.10.2025 12:52:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[HasPermission]
(
    @User NVARCHAR(128),
    @DocumentTypeId INT,
    @CounterPartyId INT,
    @CountryCode NVARCHAR(2)
)
AS
BEGIN
    DECLARE @result BIT;
    DECLARE @accountId INT;
    
    SET @result = 1; -- we are always optimistic
    IF @User IS NOT NULL
		BEGIN
   		   -- a null value in the countryCode, CounterPartyId or DocumentId (UserPermissions) means that the user account has access to all of them
   		   -- a null value from the document attribute means everyone can see it.
		   IF EXISTS (SELECT null
					  FROM   dbo.UserPermissions  as uspe
					  WHERE  uspe.AccountName = @user
					  AND    (@CounterPartyId IS NULL OR uspe.CounterPartyId IS NULL OR @CounterPartyId = uspe.CounterPartyId)
					  AND    (@CountryCode    IS NULL OR uspe.CountryCode    IS NULL OR @CountryCode    = uspe.CountryCode)        
					  AND    (@DocumentTypeId IS NULL OR uspe.DocumentTypeId IS NULL or @DocumentTypeId = uspe.DocumentTypeId))
				SET @result = 1;
		   ELSE
				SET @result = 0;
		END;
    ELSE
       SET @result = 0; -- no user, no permissions
    
    SELECT @result;
END
    

GO


