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
    /// Get Scan Copies report - documents that are fax copies but original not yet received
    /// </summary>
    public async Task<List<ScanCopiesReportDto>> GetScanCopiesReportAsync()
    {
        var currentUser = await _currentUserService.GetCurrentUserAsync();
        _logger.LogInformation("User {User} requested Scan Copies report", currentUser.AccountName);

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Execute SQL query to find fax copies where original not received
        // Uses LEFT JOINs to handle documents without type, name, or counterparty
        var scanCopies = await context.Database.SqlQueryRaw<ScanCopiesReportDto>(@"
            WITH docs AS (
                SELECT
                    d.BarCode,
                    dt.DT_Name AS [Document type],
                    dn.Name AS [Document name],
                    d.DocumentNo AS [Document No],
                    cp.Name AS Counterparty,
                    cp.CounterPartyNoAlpha AS [Counterparty No]
                FROM dbo.Document d
                LEFT JOIN dbo.DocumentType dt ON dt.DT_ID = d.DT_ID
                LEFT JOIN dbo.DocumentName dn ON dn.ID = d.DocumentNameId
                LEFT JOIN dbo.CounterParty cp ON cp.CounterPartyId = d.CounterPartyId
                WHERE d.Fax = 1 AND d.OriginalReceived = 0
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
            ORDER BY [Document type], [Document No]
        ").ToListAsync();

        _logger.LogInformation("Found {Count} scan copies for user {User}", scanCopies.Count, currentUser.AccountName);
        return scanCopies;
    }

    /// <summary>
    /// Get Suppliers report - counterparty/supplier list for check-in display
    /// </summary>
    public async Task<List<SuppliersReportDto>> GetSuppliersReportAsync()
    {
        var currentUser = await _currentUserService.GetCurrentUserAsync();
        _logger.LogInformation("User {User} requested Suppliers report", currentUser.AccountName);

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Execute SQL query to get suppliers/counterparties displayed at check-in
        var suppliers = await context.Database.SqlQueryRaw<SuppliersReportDto>(@"
            SELECT
                cp.CounterPartyNoAlpha,
                cp.Name,
                cp.Country,
                cp.AffiliatedTo,
                NULL AS ExportedAt
            FROM dbo.CounterParty cp
            WHERE cp.DisplayAtCheckIn = 1
            ORDER BY cp.Name, cp.Country
        ").ToListAsync();

        _logger.LogInformation("Found {Count} suppliers for user {User}", suppliers.Count, currentUser.AccountName);
        return suppliers;
    }

    /// <summary>
    /// Get All Documents report - exports all documents in the system with full details
    /// </summary>
    public async Task<List<AllDocumentsReportDto>> GetAllDocumentsReportAsync()
    {
        var currentUser = await _currentUserService.GetCurrentUserAsync();
        _logger.LogInformation("User {User} requested All Documents report", currentUser.AccountName);

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Execute comprehensive SQL query to get all documents with full details
        // Includes CounterParty, Country, DocumentType, DocumentFile information
        var allDocuments = await context.Database.SqlQueryRaw<AllDocumentsReportDto>(@"
            SELECT
                dbo.CounterParty.CounterPartyNoAlpha AS CPNo,
                dbo.CounterParty.CounterPartyNoAlpha AS CPNoAlpha,
                dbo.CounterParty.Name AS CPName,
                dbo.Country.CountryCode,
                dbo.Country.Name AS Country,
                dbo.CounterParty.City,
                dbo.CounterParty.AffiliatedTo,
                dbo.[Document].BarCode AS DocBarcode,
                dbo.DocumentType.DT_Name AS DocType,
                dbo.DocumentFile.FileName,
                dbo.[Document].DateOfContract,
                dbo.[Document].Comment,
                dbo.[Document].ReceivingDate,
                dbo.[Document].DispatchDate,
                dbo.[Document].Fax,
                dbo.[Document].OriginalReceived,
                dbo.[Document].ActionDate,
                dbo.[Document].ActionDescription,
                dbo.[Document].DocumentNo,
                dbo.[Document].AssociatedToPUA,
                dbo.[Document].VersionNo,
                dbo.[Document].AssociatedToAppendix,
                dbo.[Document].ValidUntil,
                dbo.[Document].CurrencyCode,
                dbo.[Document].Amount,
                dbo.[Document].Confidential,
                dbo.[Document].ThirdParty,
                dbo.[Document].Authorisation,
                dbo.[Document].BankConfirmation,
                dbo.[Document].TranslatedVersionReceived,
                NULL AS ExportedAt
            FROM dbo.CounterParty
            LEFT JOIN dbo.Country ON dbo.CounterParty.Country = dbo.Country.CountryCode
            LEFT JOIN dbo.[Document] ON dbo.CounterParty.CounterPartyId = dbo.[Document].CounterPartyId
            LEFT JOIN dbo.DocumentName ON dbo.[Document].DocumentNameId = dbo.DocumentName.ID
                AND dbo.DocumentName.DocumentTypeId = dbo.Document.DT_ID
            INNER JOIN dbo.DocumentType ON dbo.[Document].DT_ID = dbo.DocumentType.DT_ID
            LEFT JOIN dbo.DocumentFile ON dbo.[Document].FileId = dbo.DocumentFile.Id
                AND dbo.[Document].FileId = dbo.DocumentFile.Id
        ").ToListAsync();

        _logger.LogInformation("Found {Count} documents for user {User}", allDocuments.Count, currentUser.AccountName);
        return allDocuments;
    }
}
