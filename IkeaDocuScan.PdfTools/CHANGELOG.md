# Changelog

All notable changes to IkeaDocuScan.PdfTools will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-01-27

### Added
- Initial release of IkeaDocuScan.PdfTools
- PDF text extraction from file paths
- PDF text extraction from byte arrays (for database scenarios)
- PDF text extraction from streams
- Full async/await support for all operations
- PDF metadata extraction (title, author, pages, etc.)
- PDF comparison functionality with similarity metrics
- Custom exception types (PdfToolsException, PdfEncryptedException, PdfCorruptedException)
- Comprehensive XML documentation for all public APIs
- CSnakes integration for embedded Python runtime
- PyPDF2 3.0.1 for PDF processing
- Support for .NET 9.0 and .NET 10
- Windows Server compatibility
- Complete README with usage examples
- Detailed SETUP_GUIDE for deployment
- Full-text search integration examples

### Technical Details
- Embedded Python runtime via CSnakes (no external Python required)
- PyPDF2 3.0.1 for text extraction
- Multiple input type support: file path, byte[], Stream
- Error handling for encrypted, corrupted, and invalid PDFs
- Performance optimized for large files with async operations

### Documentation
- README.md with comprehensive usage examples
- SETUP_GUIDE.md with detailed deployment instructions
- XML documentation comments on all public APIs
- Full-text search integration example
- Database integration patterns (varbinary(max) support)

### Known Limitations
- No OCR support (text-layer extraction only)
- No password support for encrypted PDFs
- Basic character-based comparison (no advanced diff)

## [Unreleased]

### Planned Features
- OCR support for scanned documents
- Password-protected PDF support
- Page-by-page text extraction
- Table extraction capabilities
- Form field extraction
- PDF to Markdown conversion
- Advanced diff visualization
- Performance optimizations for very large PDFs (>100MB)
- NuGet package distribution
- Linux support (currently Windows Server only)

---

## Version History Summary

- **1.0.0** (2025-01-27): Initial release with core text extraction and comparison features
