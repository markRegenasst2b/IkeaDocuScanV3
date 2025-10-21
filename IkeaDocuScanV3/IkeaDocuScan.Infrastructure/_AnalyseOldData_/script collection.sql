
-- check counterparty table for mismached numeric and alpha counter party numbers
  select * 
  FROM [IkeaDocumentScanningCH].[dbo].[CounterParty]
  where CounterPartyNoAlpha <> cast(CounterPartyNo as varchar(20))

-- check counterparty table with AffiliatedTo is not null
  select * 
  FROM [IkeaDocumentScanningCH].[dbo].[CounterParty]
  where AffiliatedTo is not null

-- check relation table for mismatched numeric and alpha counterparty numbers
select * 
from CounterPartyRelation 
where ChildCounterPartyNoAlpha <> CAST(ChildCounterPartyNo as varchar(20)) 
or   (ChildCounterPartyNo is null and ChildCounterPartyNoAlpha is not null)
or   (ChildCounterPartyNo is not null and ChildCounterPartyNoAlpha is null)
or   ParentCounterPartyNoAlpha <> CAST(ParentCounterPartyNo as varchar(20)) 
or   (ParentCounterPartyNo is null and ParentCounterPartyNoAlpha is not null)
or   (ParentCounterPartyNo is not null and ParentCounterPartyNoAlpha is null)
or   (ParentCounterPartyNo is not null and TRY_CAST(ParentCounterPartyNoAlpha as decimal) is null)
or   (ChildCounterPartyNo is not null and TRY_CAST(ChildCounterPartyNoAlpha as decimal) is null)

select * 
from CounterPartyRelation 
where TRY_CAST(ParentCounterPartyNoAlpha as decimal) is null
or    TRY_CAST(ChildCounterPartyNoAlpha as decimal) is null



-- orphaned counterparty relations:
select * from CounterPartyRelation cpr where cpr.ParentCounterPartyNoAlpha is not null and not exists (select null from CounterParty cp where cp.CounterPartyNoAlpha = cpr.ParentCounterPartyNoAlpha)
select * from CounterPartyRelation cpr where cpr.ParentCounterPartyNo is not null and not exists (select null from CounterParty cp where cp.CounterPartyNo = cpr.ParentCounterPartyNo)
select * from CounterPartyRelation cpr where cpr.ChildCounterPartyNoAlpha is not null and not exists (select null from CounterParty cp where cp.CounterPartyNoAlpha = cpr.ChildCounterPartyNoAlpha)
select * from CounterPartyRelation cpr where cpr.ChildCounterPartyNo is not null and not exists (select null from CounterParty cp where cp.CounterPartyNo = cpr.ChildCounterPartyNo)

select * from CounterParty where CounterPartyNo = 13362 or TRY_CAST(counterpartynoalpha as varchar(29)) = '13362';


-- check if third party is used
select * from Document where ThirdPartyId is not null
select * from Document where ThirdParty is not null

-- check if ForwardedToSignatoriesDate is used
select * from Document where ForwardedToSignatoriesDate is not null
-- check if ReminderGroup is used
select * from Document where ReminderGroup is not null
-- check if SendingOutDate is used
select * from Document where SendingOutDate is not null



-- LIST TABLE COLUMNS
select * from sys.all_objects where type='U'
select t.name, c.name, c.system_type_id from sys.all_columns c, sys.all_objects t where t.type='U' and t.object_id =c.object_id order by 1,2