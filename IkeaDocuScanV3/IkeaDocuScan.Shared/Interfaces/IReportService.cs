using IkeaDocuScan.Shared.DTOs.Reports;

namespace IkeaDocuScan.Shared.Interfaces;

/// <summary>
/// Service for generating special reports
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Get Barcode Gaps report - identifies missing barcodes in the sequence
    /// </summary>
    Task<List<BarcodeGapReportDto>> GetBarcodeGapsReportAsync();

    /// <summary>
    /// Get Duplicate Documents report - identifies potential duplicate documents
    /// </summary>
    Task<List<DuplicateDocumentsReportDto>> GetDuplicateDocumentsReportAsync();

    /// <summary>
    /// Get Unlinked Registrations report - documents registered but not linked to files
    /// </summary>
    Task<List<UnlinkedRegistrationsReportDto>> GetUnlinkedRegistrationsReportAsync();

    /// <summary>
    /// Get Scan Copies report - scanned files and their status
    /// </summary>
    Task<List<ScanCopiesReportDto>> GetScanCopiesReportAsync();

    /// <summary>
    /// Get Suppliers report - counterparty/supplier statistics
    /// </summary>
    Task<List<SuppliersReportDto>> GetSuppliersReportAsync();

    /// <summary>
    /// Get All Documents report - exports all documents in the system
    /// </summary>
    Task<List<AllDocumentsReportDto>> GetAllDocumentsReportAsync();
}
