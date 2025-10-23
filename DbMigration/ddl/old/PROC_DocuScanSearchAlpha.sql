USE [IkeaDocumentScanningCH]
GO

/****** Object:  StoredProcedure [dbo].[DocuScanSearchAlpha]    Script Date: 23.10.2025 12:51:46 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[DocuScanSearchAlpha](@user NVARCHAR(100),
                                @barcodes NVARCHAR(MAX),  -- comma separated list of barcodes
                                @document_types NVARCHAR(MAX), -- comma separated list of document type ids
                                @free_text  NVARCHAR(MAX),
                                @counter_party_no_alpha nvarchar(32),
                                @counter_party_name NVARCHAR(MAX),
								@document_no NVARCHAR(1000),
								@associated_to_pua NVARCHAR(1000),
								@associated_to_appendix NVARCHAR(1000),
								@authorisation_to NVARCHAR(1000),
								@country_code NVARCHAR(10),
								@city_name NVARCHAR(255),
								@document_name NVARCHAR(255),
								@currency_code NVARCHAR(10),
								@date_of_contract_from DATETIME,
								@date_of_contract_to DATETIME,
								@receiving_date_from DATETIME,
								@receiving_date_to DATETIME,
								@sending_out_date_from DATETIME,
								@sending_out_date_to DATETIME,
								@forwarded_to_signatories_date_from DATETIME,
								@forwarded_to_signatories_date_to DATETIME,
								@dispatch_date_from DATETIME,
								@dispatch_date_to DATETIME,
								@action_date_from DATETIME,
								@action_date_to DATETIME,
								@amount_from DECIMAL,
								@amount_to DECIMAL,
								@version_no NVARCHAR(1000),
								@fax BIT,
								@original_received BIT,
								@confidential BIT,
								@bank_confirmation BIT)
AS
	DECLARE @sql NVARCHAR(MAX);
	DECLARE @sql1 NVARCHAR(MAX);
	DECLARE @sql2 NVARCHAR(MAX);
	DECLARE @parameterdefinitions NVARCHAR(MAX);
	DECLARE @inclause NVARCHAR(MAX);
	--DECLARE @userAccountId INT;

    --insert into dbo.trashcan values(@user);
	
	--SELECT @userAccountId=Id FROM dbo.UserAccount WHERE AccountName=@user;

    SET @sql = N'
	SELECT docu.* 
	FROM   dbo.document        AS docu
		   left join dbo.CounterParty  AS copa ON (copa.CounterPartyId=docu.CounterPartyId)
		   left join dbo.DocumentType dt on dt.DT_ID=docu.DT_ID
    WHERE isnull(dt.IsEnabled, 1)=1 
		AND EXISTS 
          (SELECT null
           FROM   dbo.UserPermissions  as uspe
           WHERE  uspe.AccountName = @user
           AND    (docu.CounterPartyId IS NULL OR uspe.CounterPartyId IS NULL OR docu.CounterPartyId = uspe.CounterPartyId)
           AND    (copa.Country        IS NULL OR uspe.CountryCode    IS NULL OR copa.Country = uspe.CountryCode)        
           AND    (docu.DT_ID          IS NULL OR uspe.DocumentTypeId IS NULL or docu.DT_ID = uspe.DocumentTypeId))';

    --WHERE (docu.CounterPartyId IS NULL OR EXISTS 
    --         (SELECT null FROM dbo.CounterPartyPermission
    --          WHERE (UserAccountId IS NULL AND CounterPartyId IS NULL)
    --          OR (UserAccountId=@userAccountId AND (CounterPartyId IS NULL OR CounterPartyId=docu.CounterPartyId))))
    --AND   (copa.Country IS NULL OR EXISTS
    --         (SELECT null FROM dbo.CountryPermission
    --         WHERE (UserAccountId IS NULL AND CountryCode IS NULL)
    --          OR (UserAccountId=@userAccountId AND (CountryCode IS NULL OR CountryCode=copa.Country))))
    --AND   (docu.DT_ID IS NULL OR EXISTS
    --         (SELECT null FROM dbo.DocumentTypePermission
    --          WHERE (UserAccountId IS NULL AND DocumentTypeId IS NULL)
    --          OR (UserAccountId=@userAccountId AND (DocumentTypeId IS NULL OR DocumentTypeId=docu.DT_ID)))) ';
	
	IF (@barcodes IS NOT NULL) 
	BEGIN
	    SELECT @inclause = dbo.build_in_clause(@barcodes);
	    SET @sql = @sql + N' AND docu.barcode in ' + @inclause;
	END
	
	IF (@document_types IS NOT NULL) 
	BEGIN
	    SELECT @inclause = dbo.build_in_clause(@document_types);
	    SET @sql = @sql + N' AND docu.DT_ID in ' + @inclause;
	END

	IF (@counter_party_no_alpha IS NOT NULL)
		SET @sql = @sql + N' AND    copa.CounterPartyNoAlpha  = @counter_party_no_alpha'
	IF (@country_code IS NOT NULL)
		SET @sql = @sql + N' AND    copa.Country         = @country_code'
	IF (@currency_code IS NOT NULL)
		SET @sql = @sql + N' AND    docu.CurrencyCode    = @currency_code'
	IF (@date_of_contract_from IS NOT NULL)
		SET @sql = @sql + N' AND    docu.DateOfContract >= @date_of_contract_from'
	IF (@date_of_contract_to IS NOT NULL)
		SET @sql = @sql + N' AND    docu.DateOfContract <= @date_of_contract_to'
	IF (@receiving_date_from IS NOT NULL)
		SET @sql = @sql + N' AND    docu.ReceivingDate  >= @receiving_date_from'
	IF (@receiving_date_to IS NOT NULL)
		SET @sql = @sql + N' AND    docu.ReceivingDate  <= @receiving_date_to'

		-- T2B AG 
	IF (@sending_out_date_from IS NOT NULL)
		SET @sql = @sql + N' AND    docu.SendingOutDate  >= @sending_out_date_from'
	IF (@sending_out_date_to IS NOT NULL)
		SET @sql = @sql + N' AND    docu.SendingOutDate  <= @sending_out_date_to'

	IF (@forwarded_to_signatories_date_from IS NOT NULL)
		SET @sql = @sql + N' AND    docu.ForwardedToSignatoriesDate  >= @forwarded_to_signatories_date_from'
	IF (@forwarded_to_signatories_date_to IS NOT NULL)
		SET @sql = @sql + N' AND    docu.ForwardedToSignatoriesDate  <= @forwarded_to_signatories_date_to'

	IF (@dispatch_date_from IS NOT NULL)
		SET @sql = @sql + N' AND    docu.DispatchDate   >= @dispatch_date_from'
	IF (@dispatch_date_to IS NOT NULL)
		SET @sql = @sql + N' AND    docu.DispatchDate   <= @dispatch_date_to'
	IF (@action_date_from IS NOT NULL)
		SET @sql = @sql + N' AND    docu.ActionDate     >= @action_date_from'
	IF (@action_date_to IS NOT NULL)
		SET @sql = @sql + N' AND    docu.ActionDate     <= @action_date_to'
	IF (@amount_from IS NOT NULL)
		SET @sql = @sql + N' AND    docu.Amount         >= @amount_from'
	IF (@amount_to IS NOT NULL)
		SET @sql = @sql + N' AND    docu.Amount         <= @amount_to'
    IF (@fax IS NOT NULL) 
        SET @sql = @sql + N' AND    docu.Fax             = @fax'
    IF (@original_received IS NOT NULL) 
        SET @sql = @sql + N' AND    docu.OriginalReceived= @original_received'
    IF (@confidential IS NOT NULL) 
        SET @sql = @sql + N' AND    docu.Confidential    = @confidential'
    IF (@bank_confirmation IS NOT NULL)
        SET @sql = @sql + N' AND    docu.BankConfirmation= @bank_confirmation'
		
	IF (@free_text IS NOT NULL)
		SET @sql = @sql + N' AND    EXISTS (SELECT null 
											FROM   DocumentFile   dofi
											WHERE  dofi.Id           = docu.FileId
											AND    freetext (Bytes, @free_text))'
	IF (@document_name IS NOT NULL)
		SET @sql = @sql + N' AND    EXISTS (SELECT null
		                                    FROM   DocumentName    dona
		                                    WHERE  dona.id = docu.documentnameid
		                                    AND    dona.id = @document_name)'
		
	IF (@city_name IS NOT NULL)
		SET @sql = @sql + N' AND   (freetext (copa.City, @city_name) OR copa.City like ''%''+@city_name+''%'')'
	IF (@document_no IS NOT NULL)
		SET @sql = @sql + N' AND   (contains (docu.DocumentNo          , @document_no           ) or docu.DocumentNo = @document_no)'
	IF (@associated_to_pua IS NOT NULL)
		SET @sql = @sql + N' AND   (contains (docu.AssociatedToPUA     , @associated_to_pua     ) or docu.AssociatedToPUA = @associated_to_pua)'
	IF (@associated_to_appendix IS NOT NULL)
		SET @sql = @sql + N' AND   (contains (docu.AssociatedToAppendix, @associated_to_appendix) or docu.AssociatedToAppendix = @associated_to_appendix)'
	IF (@authorisation_to IS NOT NULL)
		SET @sql = @sql + N' AND   (contains (docu.Authorisation       , @authorisation_to      ) or docu.Authorisation = @authorisation_to)';    
	IF (@version_no IS NOT NULL)
		SET @sql = @sql + N' AND   (contains (docu.VersionNo           , @version_no            ) or docu.VersionNo = @version_no)';    
	IF (@counter_party_name IS NOT NULL)
	BEGIN
		SET @sql1 = @sql + N' AND   copa.CounterPartyNo in
										  (SELECT CounterPartyNo
										   FROM   CounterParty copa2
										   WHERE  freetext( Name, @counter_party_name)
										   OR     Name like ''%''+@counter_party_name+''%'')'
        SET @sql2 = @sql + N' AND (freetext (docu.ThirdParty, @counter_party_name) OR docu.ThirdParty like ''%''+@counter_party_name+''%'')'
        SET @sql = @sql1 + N' UNION ' + @sql2
    END       
		


	SET @parameterdefinitions = N'@user NVARCHAR(100),
				@free_text NVARCHAR(1000),@counter_party_no_alpha nvarchar(32),@counter_party_name NVARCHAR(1000),
				@document_no NVARCHAR(1000),@associated_to_pua NVARCHAR(1000),
				@associated_to_appendix NVARCHAR(1000),@authorisation_to NVARCHAR(1000),@country_code NVARCHAR(10),
				@city_name NVARCHAR(1000),@document_name NVARCHAR(1000),@date_of_contract_from DATETIME,
				@date_of_contract_to DATETIME,
				@receiving_date_from DATETIME,@receiving_date_to DATETIME,
				@sending_out_date_from DATETIME, @sending_out_date_to DATETIME,
				@forwarded_to_signatories_date_from DATETIME, @forwarded_to_signatories_date_to DATETIME,
				@dispatch_date_from DATETIME,
				@dispatch_date_to DATETIME,@action_date_from DATETIME,@action_date_to DATETIME,@currency_code NVARCHAR(10),
				@amount_from DECIMAL,@amount_to DECIMAL,@version_no NVARCHAR(1000),@fax BIT,
				@original_received BIT,@confidential BIT,@bank_confirmation BIT';
				
	--select @sql
				
	EXEC sp_executesql @sql, @parameterdefinitions, @user,
						@free_text             ,@counter_party_no_alpha,@counter_party_name    ,
						@document_no           ,@associated_to_pua     ,
						@associated_to_appendix,@authorisation_to      ,@country_code          ,
						@city_name             ,@document_name         ,@date_of_contract_from ,
						@date_of_contract_to   ,@receiving_date_from   ,@receiving_date_to     ,
						@sending_out_date_from, @sending_out_date_to,
						@forwarded_to_signatories_date_from,@forwarded_to_signatories_date_to,
						@dispatch_date_from    ,@dispatch_date_to      ,@action_date_from      ,
						@action_date_to        ,@currency_code         ,@amount_from           ,
						@amount_to             ,@version_no            ,@fax                   ,
						@original_received     ,@confidential          ,@bank_confirmation;


GO


