SET IDENTITY_INSERT DocumentName OFF;

-- Bulk insert from CSV file
BULK INSERT DocumentName
FROM 'C:\Users\markr\source\repos\markRegenasst2b\IkeaDocuScan-V3\IkeaDocuScanV3\Dev-Tools\DocumentNames\FROM PROD documentName.dat' -- <-- update with actual file path
WITH
(
    FIRSTROW = 2,         -- Skip header row
    FIELDTERMINATOR = ',',-- Columns are separated by commas
    ROWTERMINATOR = '\n', -- Each row ends with a newline
    TABLOCK,
    CODEPAGE = '65001'    -- UTF-8 encoding to handle special characters
);

-- Disable IDENTITY_INSERT after insert
SET IDENTITY_INSERT DocumentName ON;