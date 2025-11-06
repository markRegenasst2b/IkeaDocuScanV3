using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Shared.DTOs.Reports;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan_Web.Services;

/// <summary>
/// Service for generating special reports
/// </summary>
public class ReportService : IReportService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        IDbContextFactory<AppDbContext> contextFactory,
        ICurrentUserService currentUserService,
        ILogger<ReportService> logger)
    {
        _contextFactory = contextFactory;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Get Barcode Gaps report - identifies missing barcodes in the sequence
    /// </summary>
    public async Task<List<BarcodeGapReportDto>> GetBarcodeGapsReportAsync()
    {
        var currentUser = await _currentUserService.GetCurrentUserAsync();
        _logger.LogInformation("User {User} requested Barcode Gaps report", currentUser.AccountName);

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Execute SQL query directly on server using window function LEAD()
        var gaps = await context.Database.SqlQueryRaw<BarcodeGapReportDto>(@"
            WITH barcodes AS (
                SELECT BarCode,
                       LEAD(BarCode, 1, 0) OVER (ORDER BY BarCode) AS NextBarcode
                FROM dbo.Document
            )
            SELECT
                BarCode + 1 AS GapStart,
                NextBarcode - 1 AS GapEnd,
                NextBarcode - BarCode - 1 AS GapSize,
                BarCode AS PreviousBarcode,
                NextBarcode AS NextBarcode,
                NULL AS ExportedAt
            FROM barcodes
            WHERE BarCode + 1 <> NextBarcode AND NextBarcode <> 0
            ORDER BY BarCode
        ").ToListAsync();

        _logger.LogInformation("Found {Count} barcode gaps for user {User}", gaps.Count, currentUser.AccountName);
        return gaps;
    }

    /// <summary>
    /// Get Duplicate Documents report - identifies potential duplicate documents
    /// </summary>
    public async Task<List<DuplicateDocumentsReportDto>> GetDuplicateDocumentsReportAsync()
    {
        var currentUser = await _currentUserService.GetCurrentUserAsync();
        _logger.LogInformation("User {User} requested Duplicate Documents report", currentUser.AccountName);

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Execute SQL query to find duplicate documents
        // Groups by Document Type, Document No, Version No, and Counter Party
        // Returns only groups with more than 1 document
        var duplicates = await context.Database.SqlQueryRaw<DuplicateDocumentsReportDto>(@"
            WITH docs AS (
                SELECT
                    dt.DT_Name AS [Document type],
                    d.DocumentNo AS [Document No],
                    d.VersionNo,
                    cp.CounterPartyId,
                    cp.CounterPartyNoAlpha,
                    cp.Name AS Counterparty
                FROM dbo.Document d
                JOIN dbo.DocumentType dt ON dt.DT_ID = d.DT_ID
                JOIN CounterParty cp ON cp.CounterPartyId = d.CounterPartyId
            )
            SELECT
                [Document type] AS DocumentType,
                [Document No] AS DocumentNo,
                VersionNo,
                CounterPartyNoAlpha,
                Counterparty,
                COUNT(*) AS [Count],
                NULL AS ExportedAt
            FROM docs
            GROUP BY [Document type], [Document No], VersionNo, CounterPartyNoAlpha, Counterparty
            HAVING COUNT(*) > 1
            ORDER BY [Document type], COUNT(*) DESC, [Document No], VersionNo, CounterPartyNoAlpha
        ").ToListAsync();

        _logger.LogInformation("Found {Count} duplicate document groups for user {User}", duplicates.Count, currentUser.AccountName);
        return duplicates;
    }

    /// <summary>
    /// Get Unlinked Registrations report - documents registered but not linked to files
    /// </summary>
    public async Task<List<UnlinkedRegistrationsReportDto>> GetUnlinkedRegistrationsReportAsync()
    {
        var currentUser = await _currentUserService.GetCurrentUserAsync();
        _logger.LogInformation("User {User} requested Unlinked Registrations report", currentUser.AccountName);

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Execute SQL query to find documents without linked files
        // Uses LEFT JOINs to handle documents without type, name, or counterparty
        var unlinked = await context.Database.SqlQueryRaw<UnlinkedRegistrationsReportDto>(@"
            WITH docs AS (
                SELECT
                    d.BarCode,
                    dt.DT_Name AS [Document type],
                    dn.Name AS [Document name],
                    d.DocumentNo AS [Document No],
                    cp.Name AS Counterparty,
                    cp.CounterPartyNoAlpha AS [Counterparty No],
                    FileId
                FROM dbo.Document d
                LEFT JOIN dbo.DocumentType dt ON dt.DT_ID = d.DT_ID
                LEFT JOIN dbo.DocumentName dn ON dn.ID = d.DocumentNameId
                LEFT JOIN dbo.CounterParty cp ON cp.CounterPartyId = d.CounterPartyId
            )
            SELECT
                BarCode,
                [Document type] AS DocumentType,
                [Document name] AS DocumentName,
                [Document No] AS DocumentNo,
                Counterparty,
                [Counterparty No] AS CounterpartyNo,
                NULL AS ExportedAt
            FROM docs
            WHERE docs.FileId IS NULL
            ORDER BY [Document type], [Document No]
        ").ToListAsync();

        _logger.LogInformation("Found {Count} unlinked registrations for user {User}", unlinked.Count, currentUser.AccountName);
        return unlinked;
    }

    /// <summary>
    /// Get Scan Copies report - scanned files and their status
    /// </summary>
    public async Task<List<ScanCopiesReportDto>> GetScanCopiesReportAsync()
    {
        var currentUser = await _currentUserService.GetCurrentUserAsync();
        _logger.LogInformation("User {User} requested Scan Copies report", currentUser.AccountName);

        // TODO: Implement scan copies logic
        // Logic:
        // 1. Query all files from DocumentFile table
        // 2. Check if linked to documents
        // 3. Calculate file sizes
        // 4. Determine status (linked/unlinked)
        // 5. Return list of ScanCopiesReportDto

        throw new NotImplementedException("Scan Copies report logic not yet implemented");
    }

    /// <summary>
    /// Get Suppliers report - counterparty/supplier statistics
    /// </summary>
    public async Task<List<SuppliersReportDto>> GetSuppliersReportAsync()
    {
        var currentUser = await _currentUserService.GetCurrentUserAsync();
        _logger.LogInformation("User {User} requested Suppliers report", currentUser.AccountName);

        // TODO: Implement suppliers report logic
        // Logic:
        // 1. Query CounterParty table with document counts
        // 2. Calculate statistics (total documents, active contracts, etc.)
        // 3. Aggregate amounts by supplier
        // 4. Include country and city information
        // 5. Return list of SuppliersReportDto

        throw new NotImplementedException("Suppliers report logic not yet implemented");
    }

    /// <summary>
    /// Get All Documents report - exports all documents in the system
    /// </summary>
    public async Task<List<AllDocumentsReportDto>> GetAllDocumentsReportAsync()
    {
        var currentUser = await _currentUserService.GetCurrentUserAsync();
        _logger.LogInformation("User {User} requested All Documents report", currentUser.AccountName);

        // TODO: Implement all documents export logic
        // Logic:
        // 1. Query all documents with joins to related tables
        // 2. Include DocumentType, CounterParty, Country, etc.
        // 3. Map to AllDocumentsReportDto
        // 4. Return complete list

        throw new NotImplementedException("All Documents report logic not yet implemented");
    }
}
