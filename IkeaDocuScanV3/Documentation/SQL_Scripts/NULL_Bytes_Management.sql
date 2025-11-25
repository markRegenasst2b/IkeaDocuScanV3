-- ============================================================================
-- NULL Bytes Management Scripts
-- ============================================================================
-- Purpose: Scripts to safely NULL out DocumentFile.Bytes for testing or
--          storage optimization while maintaining data integrity
-- Created: 2025-01-25
-- ============================================================================

-- ============================================================================
-- 1. ANALYZE CURRENT STATE
-- ============================================================================

-- Count documents with files
SELECT
    'Total Documents' AS Category,
    COUNT(*) AS Count
FROM Document
UNION ALL
SELECT
    'Documents with FileId',
    COUNT(*)
FROM Document
WHERE FileId IS NOT NULL
UNION ALL
SELECT
    'Documents without FileId',
    COUNT(*)
FROM Document
WHERE FileId IS NULL;

-- Analyze DocumentFile storage
SELECT
    COUNT(*) AS TotalFiles,
    SUM(CASE WHEN Bytes IS NOT NULL THEN 1 ELSE 0 END) AS FilesWithContent,
    SUM(CASE WHEN Bytes IS NULL THEN 1 ELSE 0 END) AS FilesWithoutContent,
    SUM(DATALENGTH(Bytes))/1024/1024 AS TotalMB,
    AVG(DATALENGTH(Bytes))/1024 AS AvgKB,
    MAX(DATALENGTH(Bytes))/1024/1024 AS MaxMB,
    MIN(DATALENGTH(Bytes))/1024 AS MinKB
FROM DocumentFile
WHERE Bytes IS NOT NULL;

-- Find documents that would be affected (have files with content)
SELECT
    d.Id,
    d.BarCode,
    d.Name,
    df.Id AS FileId,
    df.FileName,
    df.FileType,
    DATALENGTH(df.Bytes)/1024 AS SizeKB,
    d.CreatedBy,
    d.CreatedOn
FROM Document d
INNER JOIN DocumentFile df ON d.FileId = df.Id
WHERE df.Bytes IS NOT NULL
ORDER BY d.CreatedOn DESC;

-- ============================================================================
-- 2. BACKUP STRATEGY (OPTIONAL)
-- ============================================================================

-- Create backup table with file bytes (CAUTION: Can be very large!)
-- Uncomment to execute:
/*
IF OBJECT_ID('DocumentFile_Backup', 'U') IS NOT NULL
    DROP TABLE DocumentFile_Backup;

SELECT *
INTO DocumentFile_Backup
FROM DocumentFile
WHERE Bytes IS NOT NULL;

PRINT 'Backup created: DocumentFile_Backup';
PRINT 'Records backed up: ' + CAST(@@ROWCOUNT AS VARCHAR(10));
*/

-- ============================================================================
-- 3. NULL OUT BYTES (MAIN OPERATION)
-- ============================================================================

-- OPTION A: NULL all file bytes (CAUTION: Cannot be undone without backup!)
-- Uncomment to execute:
/*
BEGIN TRANSACTION;

DECLARE @RowsAffected INT;
DECLARE @TotalBytesBefore BIGINT;

-- Get total size before
SELECT @TotalBytesBefore = SUM(DATALENGTH(Bytes))
FROM DocumentFile
WHERE Bytes IS NOT NULL;

-- NULL out all bytes
UPDATE DocumentFile
SET Bytes = NULL
WHERE Bytes IS NOT NULL;

SET @RowsAffected = @@ROWCOUNT;

PRINT 'Files updated: ' + CAST(@RowsAffected AS VARCHAR(10));
PRINT 'Storage freed: ' + CAST(@TotalBytesBefore/1024/1024 AS VARCHAR(10)) + ' MB';

-- Verify the change
SELECT
    COUNT(*) AS TotalFiles,
    SUM(CASE WHEN Bytes IS NULL THEN 1 ELSE 0 END) AS FilesWithNullBytes,
    SUM(CASE WHEN Bytes IS NOT NULL THEN 1 ELSE 0 END) AS FilesWithContent
FROM DocumentFile;

-- IMPORTANT: Review the results above before committing!
-- To commit: COMMIT TRANSACTION;
-- To rollback: ROLLBACK TRANSACTION;

-- Uncomment ONE of the following:
-- COMMIT TRANSACTION;    -- Make changes permanent
ROLLBACK TRANSACTION;  -- Undo changes (default for safety)
*/

-- OPTION B: NULL bytes for specific file types (e.g., only PDFs)
-- Uncomment to execute:
/*
BEGIN TRANSACTION;

UPDATE DocumentFile
SET Bytes = NULL
WHERE Bytes IS NOT NULL
  AND FileType IN ('.pdf', 'pdf');

PRINT 'PDF files updated: ' + CAST(@@ROWCOUNT AS VARCHAR(10));

-- Review before committing
SELECT FileType, COUNT(*) AS Count
FROM DocumentFile
WHERE Bytes IS NULL
GROUP BY FileType;

ROLLBACK TRANSACTION;  -- Change to COMMIT when ready
*/

-- OPTION C: NULL bytes for files older than a certain date
-- Uncomment to execute:
/*
BEGIN TRANSACTION;

UPDATE df
SET df.Bytes = NULL
FROM DocumentFile df
INNER JOIN Document d ON d.FileId = df.Id
WHERE df.Bytes IS NOT NULL
  AND d.CreatedOn < '2024-01-01';  -- Change date as needed

PRINT 'Old document files updated: ' + CAST(@@ROWCOUNT AS VARCHAR(10));

ROLLBACK TRANSACTION;  -- Change to COMMIT when ready
*/

-- ============================================================================
-- 4. VERIFY APPLICATION BEHAVIOR AFTER NULLING
-- ============================================================================

-- Check documents that have FileId but no content
SELECT
    d.Id,
    d.BarCode,
    d.Name,
    df.FileName,
    df.FileType,
    CASE
        WHEN df.Bytes IS NULL THEN 'No Content (NULL)'
        WHEN DATALENGTH(df.Bytes) = 0 THEN 'No Content (Empty)'
        ELSE 'Has Content'
    END AS ContentStatus,
    DATALENGTH(df.Bytes) AS BytesLength
FROM Document d
INNER JOIN DocumentFile df ON d.FileId = df.Id
WHERE df.Bytes IS NULL OR DATALENGTH(df.Bytes) = 0
ORDER BY d.CreatedOn DESC;

-- ============================================================================
-- 5. RESTORE STRATEGY (If you have backups)
-- ============================================================================

-- Restore from backup table
-- Uncomment to execute:
/*
BEGIN TRANSACTION;

UPDATE df
SET df.Bytes = bak.Bytes
FROM DocumentFile df
INNER JOIN DocumentFile_Backup bak ON df.Id = bak.Id
WHERE df.Bytes IS NULL
  AND bak.Bytes IS NOT NULL;

PRINT 'Files restored: ' + CAST(@@ROWCOUNT AS VARCHAR(10));

-- Verify restoration
SELECT
    COUNT(*) AS TotalFiles,
    SUM(CASE WHEN Bytes IS NOT NULL THEN 1 ELSE 0 END) AS FilesWithContent,
    SUM(CASE WHEN Bytes IS NULL THEN 1 ELSE 0 END) AS FilesWithNullBytes
FROM DocumentFile;

ROLLBACK TRANSACTION;  -- Change to COMMIT when ready
*/

-- ============================================================================
-- 6. CLEANUP (After successful restoration)
-- ============================================================================

-- Drop backup table (only after verifying restoration worked)
-- Uncomment to execute:
/*
IF OBJECT_ID('DocumentFile_Backup', 'U') IS NOT NULL
BEGIN
    DROP TABLE DocumentFile_Backup;
    PRINT 'Backup table dropped';
END
*/

-- ============================================================================
-- 7. MONITORING QUERIES
-- ============================================================================

-- Monitor file content availability over time
SELECT
    CAST(d.CreatedOn AS DATE) AS CreatedDate,
    COUNT(*) AS TotalDocuments,
    SUM(CASE WHEN d.FileId IS NOT NULL THEN 1 ELSE 0 END) AS WithFileId,
    SUM(CASE WHEN df.Bytes IS NOT NULL THEN 1 ELSE 0 END) AS WithContent,
    SUM(CASE WHEN d.FileId IS NOT NULL AND df.Bytes IS NULL THEN 1 ELSE 0 END) AS MissingContent
FROM Document d
LEFT JOIN DocumentFile df ON d.FileId = df.Id
WHERE d.CreatedOn >= DATEADD(MONTH, -3, GETDATE())
GROUP BY CAST(d.CreatedOn AS DATE)
ORDER BY CreatedDate DESC;

-- Find documents accessed recently that have no content
-- (Requires audit trail implementation)
/*
SELECT DISTINCT
    d.Id,
    d.BarCode,
    d.Name,
    df.FileName,
    MAX(at.Timestamp) AS LastAccessed
FROM Document d
INNER JOIN DocumentFile df ON d.FileId = df.Id
LEFT JOIN AuditTrail at ON at.BarCode = CAST(d.BarCode AS VARCHAR(50))
WHERE df.Bytes IS NULL
  AND at.Action IN ('View', 'Download', 'Email')
  AND at.Timestamp >= DATEADD(DAY, -30, GETDATE())
GROUP BY d.Id, d.BarCode, d.Name, df.FileName
ORDER BY LastAccessed DESC;
*/
