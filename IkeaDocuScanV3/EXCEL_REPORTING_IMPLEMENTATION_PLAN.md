# Excel Reporting Implementation Plan

## Document Information

**Created:** 2025-01-27
**Version:** 1.0
**Project:** IkeaDocuScan-V3 Excel Reporting Feature
**Target Framework:** .NET 9.0 / .NET 10.0
**Status:** Implementation-Ready

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Project Structure and Namespace Layout](#project-structure-and-namespace-layout)
3. [Key Classes, Interfaces, and Responsibilities](#key-classes-interfaces-and-responsibilities)
4. [Implementation Sequence](#implementation-sequence)
5. [Setup Details](#setup-details)
6. [Blazor UI/UX Considerations](#blazor-uiux-considerations)
7. [Testing and Validation Strategy](#testing-and-validation-strategy)
8. [Extensibility Considerations](#extensibility-considerations)
9. [Open Questions and Assumptions](#open-questions-and-assumptions)

---

## Executive Summary

### Purpose

This plan defines the architecture and implementation approach for adding Excel export functionality to the IkeaDocuScan application. The feature enables users to preview collections of DTO objects in a dynamic, paginated data grid and export the data to Excel files with proper formatting.

### Key Components

1. **ExcelReporting Class Library** - Standalone DLL for Excel generation using Syncfusion XlsIO
2. **ExcelExport Attribute** - Custom attribute for DTO property metadata (display name, data type, format)
3. **Abstract DTO Base Class** - Common base class for all exportable DTOs
4. **ExcelPreview.razor Component** - Blazor page for data preview and export
5. **Service Layer Integration** - DI-based service for Excel generation

### Design Principles

- **Separation of Concerns:** Excel generation logic isolated in separate library
- **Metadata-Driven:** Attribute-based configuration for grid rendering and export
- **Reflection-Based Rendering:** Dynamic grid generation without hardcoded columns
- **Extensibility:** Support for new DTO types without code changes
- **Consistency:** Follows existing IkeaDocuScan architectural patterns

---

## Project Structure and Namespace Layout

### New Project: ExcelReporting

**Project Type:** Class Library (.NET 9.0 / .NET 10.0)
**Location:** `IkeaDocuScanV3/ExcelReporting/`
**Output:** `ExcelReporting.dll`

```
IkeaDocuScanV3/
├── ExcelReporting/                      # NEW - Excel generation library
│   ├── ExcelReporting.csproj
│   ├── Attributes/
│   │   └── ExcelExportAttribute.cs      # Custom attribute for DTO properties
│   ├── Models/
│   │   ├── ExportableBase.cs            # Abstract base class for DTOs
│   │   ├── ExcelExportMetadata.cs       # Metadata extracted from attributes
│   │   ├── ExcelExportOptions.cs        # Configuration options for export
│   │   └── ExcelDataType.cs             # Enum: String, Number, Date, Currency, Boolean
│   ├── Services/
│   │   ├── IExcelExportService.cs       # Service interface
│   │   ├── ExcelExportService.cs        # Implementation using Syncfusion XlsIO
│   │   └── PropertyMetadataExtractor.cs # Reflection utility for attribute extraction
│   └── Extensions/
│       └── ServiceCollectionExtensions.cs # DI registration helper
│
├── IkeaDocuScan.Shared/                 # UPDATED - Add export DTOs
│   └── DTOs/
│       └── Excel/                       # NEW - DTOs for export scenarios
│           ├── DocumentExportDto.cs     # Example: Document export DTO
│           └── ... (other export DTOs)
│
└── IkeaDocuScan-Web.Client/             # UPDATED - Add UI component
    ├── Pages/
    │   └── ExcelPreview.razor           # NEW - Preview and export page
    │       └── ExcelPreview.razor.cs    # Code-behind
    └── Services/                        # Optional: Client-side wrapper
        └── ExcelExportHttpService.cs    # If server-side export endpoint needed
```

### Namespace Conventions

```csharp
// ExcelReporting library
namespace ExcelReporting.Attributes;
namespace ExcelReporting.Models;
namespace ExcelReporting.Services;
namespace ExcelReporting.Extensions;

// Shared DTOs
namespace IkeaDocuScan.Shared.DTOs.Excel;

// Blazor component
namespace IkeaDocuScan_Web.Client.Pages;
```

---

## Key Classes, Interfaces, and Responsibilities

### 1. ExcelExportAttribute

**Location:** `ExcelReporting/Attributes/ExcelExportAttribute.cs`
**Purpose:** Decorate DTO properties with export metadata

```csharp
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ExcelExportAttribute : Attribute
{
    public string DisplayName { get; }
    public ExcelDataType DataType { get; }
    public string Format { get; }
    public int Order { get; set; } = 0;           // Column ordering
    public bool IsExportable { get; set; } = true; // Allow opt-out

    public ExcelExportAttribute(
        string displayName,
        ExcelDataType dataType = ExcelDataType.String,
        string format = null)
    {
        DisplayName = displayName;
        DataType = dataType;
        Format = format ?? GetDefaultFormat(dataType);
    }
}
```

**Responsibilities:**
- Store metadata for property display and formatting
- Support column ordering
- Allow properties to opt-out of export

---

### 2. ExcelDataType Enum

**Location:** `ExcelReporting/Models/ExcelDataType.cs`
**Purpose:** Define supported data types for Excel export

```csharp
public enum ExcelDataType
{
    String,      // General text
    Number,      // Numeric values (int, decimal, double)
    Date,        // DateTime values
    Currency,    // Monetary values
    Percentage,  // Percentage values (0.0 - 1.0)
    Boolean,     // True/False values
    Hyperlink    // URLs or clickable links
}
```

**Responsibilities:**
- Define data type semantics
- Guide cell formatting in Excel
- Enable type-specific validation

---

### 3. ExportableBase Abstract Class

**Location:** `ExcelReporting/Models/ExportableBase.cs`
**Purpose:** Base class for all exportable DTOs

```csharp
public abstract class ExportableBase
{
    // Optional: Common metadata for all DTOs
    [ExcelExport("Export Timestamp", ExcelDataType.Date, "yyyy-MM-dd HH:mm:ss")]
    public DateTime? ExportedAt { get; set; }

    // Virtual method for custom validation before export
    public virtual bool ValidateForExport(out string errorMessage)
    {
        errorMessage = null;
        return true;
    }

    // Virtual method for pre-export transformations
    public virtual void PrepareForExport()
    {
        ExportedAt = DateTime.Now;
    }
}
```

**Responsibilities:**
- Enforce common contract for exportable DTOs
- Provide extensibility points for validation and transformation
- Optional common properties (export timestamp, user, etc.)

---

### 4. ExcelExportMetadata

**Location:** `ExcelReporting/Models/ExcelExportMetadata.cs`
**Purpose:** Extracted metadata from attributes for a single property

```csharp
public class ExcelExportMetadata
{
    public PropertyInfo Property { get; set; }
    public string DisplayName { get; set; }
    public ExcelDataType DataType { get; set; }
    public string Format { get; set; }
    public int Order { get; set; }
    public bool IsExportable { get; set; }

    // Helper method to get formatted value
    public string GetFormattedValue(object instance)
    {
        var value = Property.GetValue(instance);
        if (value == null) return string.Empty;

        return DataType switch
        {
            ExcelDataType.Date => ((DateTime)value).ToString(Format),
            ExcelDataType.Currency => ((decimal)value).ToString(Format),
            ExcelDataType.Number => value.ToString(),
            ExcelDataType.Percentage => ((decimal)value).ToString(Format),
            ExcelDataType.Boolean => ((bool)value) ? "Yes" : "No",
            _ => value.ToString()
        };
    }
}
```

**Responsibilities:**
- Store extracted metadata for runtime use
- Provide formatted value retrieval
- Cache PropertyInfo for performance

---

### 5. PropertyMetadataExtractor

**Location:** `ExcelReporting/Services/PropertyMetadataExtractor.cs`
**Purpose:** Extract and cache metadata from DTO types using reflection

```csharp
public class PropertyMetadataExtractor
{
    private readonly ConcurrentDictionary<Type, List<ExcelExportMetadata>> _metadataCache;

    public PropertyMetadataExtractor()
    {
        _metadataCache = new ConcurrentDictionary<Type, List<ExcelExportMetadata>>();
    }

    public List<ExcelExportMetadata> ExtractMetadata<T>() where T : ExportableBase
    {
        return ExtractMetadata(typeof(T));
    }

    public List<ExcelExportMetadata> ExtractMetadata(Type type)
    {
        return _metadataCache.GetOrAdd(type, t =>
        {
            var properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var metadata = new List<ExcelExportMetadata>();

            foreach (var prop in properties)
            {
                var attr = prop.GetCustomAttribute<ExcelExportAttribute>();
                if (attr != null && attr.IsExportable)
                {
                    metadata.Add(new ExcelExportMetadata
                    {
                        Property = prop,
                        DisplayName = attr.DisplayName,
                        DataType = attr.DataType,
                        Format = attr.Format,
                        Order = attr.Order,
                        IsExportable = attr.IsExportable
                    });
                }
            }

            return metadata.OrderBy(m => m.Order).ThenBy(m => m.DisplayName).ToList();
        });
    }
}
```

**Responsibilities:**
- Use reflection to extract attribute metadata
- Cache metadata by Type to avoid repeated reflection
- Sort properties by Order and DisplayName
- Filter out non-exportable properties

---

### 6. IExcelExportService Interface

**Location:** `ExcelReporting/Services/IExcelExportService.cs`
**Purpose:** Define contract for Excel generation

```csharp
public interface IExcelExportService
{
    /// <summary>
    /// Generates an Excel file from a collection of DTOs
    /// </summary>
    /// <typeparam name="T">DTO type inheriting from ExportableBase</typeparam>
    /// <param name="data">Collection of data to export</param>
    /// <param name="options">Optional configuration for export</param>
    /// <returns>Memory stream containing the Excel file</returns>
    Task<MemoryStream> GenerateExcelAsync<T>(
        IEnumerable<T> data,
        ExcelExportOptions options = null)
        where T : ExportableBase;

    /// <summary>
    /// Gets metadata for a DTO type without generating Excel
    /// </summary>
    List<ExcelExportMetadata> GetMetadata<T>() where T : ExportableBase;

    /// <summary>
    /// Gets metadata for a DTO type by Type object
    /// </summary>
    List<ExcelExportMetadata> GetMetadata(Type type);
}
```

**Responsibilities:**
- Define public API for Excel generation
- Support generic and non-generic operations
- Allow metadata retrieval for UI rendering

---

### 7. ExcelExportService Implementation

**Location:** `ExcelReporting/Services/ExcelExportService.cs`
**Purpose:** Implement Excel generation using Syncfusion XlsIO

```csharp
public class ExcelExportService : IExcelExportService
{
    private readonly PropertyMetadataExtractor _metadataExtractor;

    public ExcelExportService(PropertyMetadataExtractor metadataExtractor)
    {
        _metadataExtractor = metadataExtractor;
    }

    public async Task<MemoryStream> GenerateExcelAsync<T>(
        IEnumerable<T> data,
        ExcelExportOptions options = null)
        where T : ExportableBase
    {
        // Implementation using Syncfusion XlsIO
        // 1. Create workbook and worksheet
        // 2. Extract metadata
        // 3. Write header row with formatting
        // 4. Write data rows with type-specific formatting
        // 5. Apply column widths and styles
        // 6. Save to MemoryStream
    }

    public List<ExcelExportMetadata> GetMetadata<T>() where T : ExportableBase
    {
        return _metadataExtractor.ExtractMetadata<T>();
    }

    public List<ExcelExportMetadata> GetMetadata(Type type)
    {
        return _metadataExtractor.ExtractMetadata(type);
    }
}
```

**Key Implementation Details:**
- Use `ExcelEngine` from Syncfusion.XlsIO
- Apply cell formatting based on `ExcelDataType`
- Support date formats, number formats, currency formats
- Add header row with bold styling and background color
- Auto-fit column widths or use specified widths
- Return `MemoryStream` for direct download

**Responsibilities:**
- Generate Excel workbook from DTO collection
- Apply formatting rules from metadata
- Handle large datasets efficiently
- Provide memory-efficient streaming

---

### 8. ExcelExportOptions

**Location:** `ExcelReporting/Models/ExcelExportOptions.cs`
**Purpose:** Configuration options for Excel export

```csharp
public class ExcelExportOptions
{
    public string SheetName { get; set; } = "Export";
    public bool IncludeHeader { get; set; } = true;
    public bool AutoFitColumns { get; set; } = true;
    public bool ApplyHeaderFormatting { get; set; } = true;
    public string HeaderBackgroundColor { get; set; } = "#4472C4"; // Blue
    public string HeaderFontColor { get; set; } = "#FFFFFF";       // White
    public bool FreezeHeaderRow { get; set; } = true;
    public bool EnableFilters { get; set; } = true;
    public int? MaxColumnWidth { get; set; } = 50; // Characters
    public string DateFormat { get; set; } = "yyyy-MM-dd";
    public string CurrencyFormat { get; set; } = "$#,##0.00";
    public string NumberFormat { get; set; } = "#,##0.00";
    public string PercentageFormat { get; set; } = "0.00%";
}
```

**Responsibilities:**
- Provide default formatting preferences
- Allow per-export customization
- Centralize styling configuration

---

### 9. ExcelPreview.razor Component

**Location:** `IkeaDocuScan-Web.Client/Pages/ExcelPreview.razor`
**Purpose:** Blazor page for data preview and export

**Component Structure:**
```razor
@page "/excel-preview"
@rendermode @(new InteractiveWebAssemblyRenderMode(prerender: false))
@using ExcelReporting.Models
@using ExcelReporting.Services
@inject IExcelExportService ExcelExportService
@inject IJSRuntime JSRuntime

<h3>Data Preview</h3>

<!-- Paging controls -->
<div class="paging-controls">
    <label>Rows per page:</label>
    <select @bind="rowsPerPage">
        <option value="10">10</option>
        <option value="25">25</option>
        <option value="100">100</option>
    </select>
    <span>Page @currentPage of @totalPages</span>
    <button @onclick="PreviousPage">Previous</button>
    <button @onclick="NextPage">Next</button>
</div>

<!-- Dynamic data grid -->
<table class="table">
    <thead>
        <tr>
            @foreach (var column in Columns)
            {
                <th>@column.DisplayName</th>
            }
        </tr>
    </thead>
    <tbody>
        @foreach (var item in PagedData)
        {
            <tr>
                @foreach (var column in Columns)
                {
                    <td>@column.GetFormattedValue(item)</td>
                }
            </tr>
        }
    </tbody>
</table>

<!-- Export button -->
<button class="btn btn-primary" @onclick="ExportToExcel">
    Download as Excel
</button>
```

**Code-Behind (ExcelPreview.razor.cs):**
```csharp
public partial class ExcelPreview<T> : ComponentBase where T : ExportableBase
{
    [Parameter]
    public IEnumerable<T> Data { get; set; }

    [Parameter]
    public ExcelExportOptions ExportOptions { get; set; }

    private List<ExcelExportMetadata> Columns { get; set; }
    private int currentPage = 1;
    private int rowsPerPage = 25;
    private IEnumerable<T> PagedData => GetPagedData();
    private int totalPages => (int)Math.Ceiling((double)Data.Count() / rowsPerPage);

    protected override void OnParametersSet()
    {
        Columns = ExcelExportService.GetMetadata<T>();
    }

    private IEnumerable<T> GetPagedData()
    {
        return Data.Skip((currentPage - 1) * rowsPerPage).Take(rowsPerPage);
    }

    private async Task ExportToExcel()
    {
        var stream = await ExcelExportService.GenerateExcelAsync(Data, ExportOptions);
        var fileName = $"Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

        // Use JS interop to trigger download
        await JSRuntime.InvokeVoidAsync(
            "downloadFileFromStream",
            fileName,
            Convert.ToBase64String(stream.ToArray()));
    }

    private void NextPage() { if (currentPage < totalPages) currentPage++; }
    private void PreviousPage() { if (currentPage > 1) currentPage--; }
}
```

**Responsibilities:**
- Accept generic DTO collection as parameter
- Dynamically render grid columns from metadata
- Implement paging logic
- Trigger Excel export and file download
- Support customizable export options

---

### 10. JavaScript Interop for File Download

**Location:** `IkeaDocuScan-Web.Client/wwwroot/js/excelDownload.js`
**Purpose:** Handle file download in browser

```javascript
window.downloadFileFromStream = async (fileName, base64String) => {
    const blob = base64ToBlob(base64String, 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet');
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};

function base64ToBlob(base64, contentType) {
    const byteCharacters = atob(base64);
    const byteArrays = [];
    for (let offset = 0; offset < byteCharacters.length; offset += 512) {
        const slice = byteCharacters.slice(offset, offset + 512);
        const byteNumbers = new Array(slice.length);
        for (let i = 0; i < slice.length; i++) {
            byteNumbers[i] = slice.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        byteArrays.push(byteArray);
    }
    return new Blob(byteArrays, { type: contentType });
}
```

**Responsibilities:**
- Convert base64 stream to Blob
- Trigger browser download
- Clean up object URLs

---

### 11. ServiceCollectionExtensions

**Location:** `ExcelReporting/Extensions/ServiceCollectionExtensions.cs`
**Purpose:** DI registration helper

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExcelReporting(this IServiceCollection services)
    {
        services.AddSingleton<PropertyMetadataExtractor>();
        services.AddScoped<IExcelExportService, ExcelExportService>();
        return services;
    }
}
```

**Responsibilities:**
- Simplify DI registration
- Follow existing IkeaDocuScan patterns
- Register all required services

---

## Implementation Sequence

### Phase 1: ExcelReporting Library Foundation

**Estimated Effort:** 4-6 hours

#### Step 1.1: Create ExcelReporting Project
1. Create new Class Library project targeting .NET 9.0/10.0
2. Add NuGet package: `Syncfusion.XlsIO.Net.Core` (latest version)
3. Create folder structure: `Attributes/`, `Models/`, `Services/`, `Extensions/`
4. Set up project references (if any shared dependencies needed)

#### Step 1.2: Implement Core Models and Attributes
1. Create `ExcelDataType` enum with all supported types
2. Create `ExcelExportAttribute` class with all properties
3. Create `ExportableBase` abstract class
4. Create `ExcelExportMetadata` class with value formatting
5. Create `ExcelExportOptions` class with default values
6. **Validation:** Unit test attribute extraction and metadata creation

#### Step 1.3: Implement Metadata Extraction
1. Create `PropertyMetadataExtractor` class
2. Implement reflection-based metadata extraction
3. Implement caching with `ConcurrentDictionary`
4. Add sorting by Order and DisplayName
5. Handle edge cases (no attributes, non-exportable properties)
6. **Validation:** Test with sample DTO classes

#### Step 1.4: Implement Excel Export Service Interface
1. Create `IExcelExportService` interface
2. Create `ExcelExportService` class skeleton
3. Implement `GetMetadata()` methods
4. **Validation:** Ensure interface can be mocked for testing

---

### Phase 2: Syncfusion XlsIO Integration

**Estimated Effort:** 6-8 hours

#### Step 2.1: Implement Basic Excel Generation
1. Implement `GenerateExcelAsync()` method in `ExcelExportService`
2. Create workbook and worksheet using Syncfusion API
3. Write header row with column names from metadata
4. Write data rows using reflection to get property values
5. Save to `MemoryStream` and return
6. **Validation:** Generate Excel with simple DTO (all strings)

#### Step 2.2: Implement Type-Specific Formatting
1. Add cell formatting based on `ExcelDataType`
   - **Date:** Apply date format (`yyyy-MM-dd`)
   - **Currency:** Apply currency format (`$#,##0.00`)
   - **Number:** Apply number format (`#,##0.00`)
   - **Percentage:** Apply percentage format (`0.00%`)
   - **Boolean:** Display as "Yes"/"No" or checkboxes
   - **Hyperlink:** Create clickable hyperlinks
2. Handle null values gracefully
3. **Validation:** Test each data type with sample data

#### Step 2.3: Implement Header and Sheet Styling
1. Apply header row formatting:
   - Bold font
   - Background color (from options)
   - Font color (from options)
   - Center alignment
2. Freeze header row (if enabled in options)
3. Enable auto-filters (if enabled in options)
4. Auto-fit column widths (respecting max width)
5. Set sheet name from options
6. **Validation:** Verify styling with sample exports

#### Step 2.4: Optimize for Large Datasets
1. Use streaming where possible
2. Process data in batches if needed
3. Monitor memory usage during generation
4. Add progress reporting (optional, for future use)
5. **Validation:** Test with 1,000+ row datasets

---

### Phase 3: Dependency Injection Configuration

**Estimated Effort:** 1-2 hours

#### Step 3.1: Create DI Extension Method
1. Implement `ServiceCollectionExtensions.AddExcelReporting()`
2. Register `PropertyMetadataExtractor` as singleton (thread-safe caching)
3. Register `IExcelExportService` as scoped (matches existing patterns)
4. **Validation:** Ensure services resolve correctly

#### Step 3.2: Register in IkeaDocuScan-Web
1. Add project reference to `ExcelReporting.csproj` in `IkeaDocuScan-Web.csproj`
2. Call `builder.Services.AddExcelReporting();` in `Program.cs`
3. Verify service registration in DI container
4. **Validation:** Run application and check service resolution

#### Step 3.3: Register in IkeaDocuScan-Web.Client (if needed)
1. Add project reference to `ExcelReporting.csproj` in client project
2. Register services in `IkeaDocuScan-Web.Client/Program.cs`
3. Note: Client-side may only need metadata extraction, not full Excel generation
4. **Alternative:** Keep Excel generation server-side only, expose via API endpoint

---

### Phase 4: Sample DTO Implementation

**Estimated Effort:** 2-3 hours

#### Step 4.1: Create DocumentExportDto
1. Create `IkeaDocuScan.Shared/DTOs/Excel/DocumentExportDto.cs`
2. Inherit from `ExportableBase`
3. Add properties with `[ExcelExport]` attributes:
   ```csharp
   [ExcelExport("Document ID", ExcelDataType.Number, Order = 1)]
   public int Id { get; set; }

   [ExcelExport("Document Name", ExcelDataType.String, Order = 2)]
   public string Name { get; set; }

   [ExcelExport("Created Date", ExcelDataType.Date, "yyyy-MM-dd", Order = 3)]
   public DateTime CreatedDate { get; set; }

   [ExcelExport("Amount", ExcelDataType.Currency, "$#,##0.00", Order = 4)]
   public decimal Amount { get; set; }

   [ExcelExport("Country", ExcelDataType.String, Order = 5)]
   public string CountryName { get; set; }

   [ExcelExport("Document Type", ExcelDataType.String, Order = 6)]
   public string DocumentTypeName { get; set; }
   ```
4. Create mapping method from `DocumentDto` to `DocumentExportDto`
5. **Validation:** Ensure all attributes are correctly defined

#### Step 4.2: Create Service Method for Export Data
1. Add method to `DocumentService.cs`:
   ```csharp
   public async Task<IEnumerable<DocumentExportDto>> GetDocumentsForExportAsync(
       int? counterPartyId = null,
       DateTime? startDate = null,
       DateTime? endDate = null)
   ```
2. Query database with filters
3. Project to `DocumentExportDto` with all necessary joins
4. Return flattened data ready for export
5. **Validation:** Test query performance with large datasets

---

### Phase 5: Blazor Component Implementation

**Estimated Effort:** 6-8 hours

#### Step 5.1: Create ExcelPreview.razor Component
1. Create `IkeaDocuScan-Web.Client/Pages/ExcelPreview.razor`
2. Set render mode: `@rendermode @(new InteractiveWebAssemblyRenderMode(prerender: false))`
3. Define component parameters:
   - `[Parameter] public IEnumerable<T> Data { get; set; }`
   - `[Parameter] public ExcelExportOptions ExportOptions { get; set; }`
4. Inject `IExcelExportService` and `IJSRuntime`
5. **Validation:** Component compiles and loads

#### Step 5.2: Implement Dynamic Grid Rendering
1. Extract metadata in `OnParametersSet()`
2. Render table header using `foreach` over `Columns`
3. Render table rows using nested `foreach` over `PagedData` and `Columns`
4. Use `ExcelExportMetadata.GetFormattedValue()` for cell values
5. Apply CSS classes for styling (use Blazorise or Bootstrap)
6. **Validation:** Grid displays correctly with sample data

#### Step 5.3: Implement Paging Controls
1. Add state variables: `currentPage`, `rowsPerPage`
2. Create `GetPagedData()` method using LINQ `Skip()` and `Take()`
3. Add dropdown for rows per page (10, 25, 100)
4. Add Previous/Next buttons
5. Display current page and total pages
6. Disable buttons when at first/last page
7. **Validation:** Paging works smoothly, no performance issues

#### Step 5.4: Implement Excel Export
1. Create `ExportToExcel()` method
2. Call `ExcelExportService.GenerateExcelAsync(Data, ExportOptions)`
3. Convert `MemoryStream` to base64 string
4. Generate filename with timestamp
5. Call JavaScript interop for download
6. Add error handling with user-friendly messages
7. Add loading indicator during export
8. **Validation:** Excel file downloads correctly

#### Step 5.5: Add JavaScript File for Download
1. Create `IkeaDocuScan-Web.Client/wwwroot/js/excelDownload.js`
2. Implement `downloadFileFromStream()` function
3. Implement `base64ToBlob()` helper
4. Reference script in `index.html` or component
5. **Validation:** Download triggers correctly in all browsers

#### Step 5.6: Add Error Handling and Loading States
1. Add try-catch in export method
2. Display loading spinner during export
3. Show success message after download
4. Show error message on failure
5. Log errors to console for debugging
6. **Validation:** User sees appropriate feedback

---

### Phase 6: Integration and Usage Examples

**Estimated Effort:** 3-4 hours

#### Step 6.1: Create Example Usage in Documents Page
1. Add "Export to Excel" button to `Documents.razor`
2. Navigate to `ExcelPreview.razor` with document data as parameter
3. **Alternative:** Open modal with preview component
4. Pass filtered data from current grid view
5. **Validation:** Navigation works, data passes correctly

#### Step 6.2: Create API Endpoint (if server-side export needed)
1. Create `IkeaDocuScan-Web/Endpoints/ExcelExportEndpoints.cs`
2. Add endpoint: `GET /api/excel/export/documents`
3. Accept filter parameters (counterPartyId, dateRange, etc.)
4. Call `DocumentService.GetDocumentsForExportAsync()`
5. Call `ExcelExportService.GenerateExcelAsync()`
6. Return file with `Results.File()`
7. **Validation:** Endpoint returns valid Excel file

#### Step 6.3: Create Client-Side HTTP Service (if needed)
1. Create `ExcelExportHttpService.cs` in client project
2. Implement method to call export endpoint
3. Handle file download from HTTP response
4. **Validation:** Client can trigger server-side export

---

### Phase 7: Testing and Documentation

**Estimated Effort:** 4-6 hours

#### Step 7.1: Unit Testing
1. Test `PropertyMetadataExtractor`:
   - Extract metadata from sample DTOs
   - Verify ordering and filtering
   - Test caching behavior
2. Test `ExcelExportMetadata.GetFormattedValue()`:
   - Test each data type
   - Test null values
   - Test format strings
3. Test `ExcelExportAttribute`:
   - Verify default values
   - Test attribute inheritance

#### Step 7.2: Integration Testing
1. Test `ExcelExportService`:
   - Generate Excel from sample data
   - Verify cell values and formatting
   - Test with empty data
   - Test with large datasets (1,000+ rows)
2. Test end-to-end flow:
   - Load data in Blazor component
   - Render grid
   - Export to Excel
   - Open Excel file and verify contents

#### Step 7.3: Browser Testing
1. Test ExcelPreview component in:
   - Chrome
   - Edge
   - Firefox
   - Safari (if accessible)
2. Verify download functionality in each browser
3. Test paging with different row counts
4. Test with mobile browsers (responsive design)

#### Step 7.4: Performance Testing
1. Test export with 100 rows (baseline)
2. Test export with 1,000 rows
3. Test export with 10,000 rows
4. Monitor memory usage during export
5. Measure export time
6. Optimize if performance is unacceptable

#### Step 7.5: Documentation
1. Add XML comments to all public classes and methods
2. Create usage examples in README
3. Document attribute properties and options
4. Create developer guide for adding new export DTOs
5. Update `CLAUDE.md` with Excel reporting information

---

## Setup Details

### NuGet Packages

#### ExcelReporting Project
```xml
<PackageReference Include="Syncfusion.XlsIO.Net.Core" Version="27.1.48" />
```

**Note:** Check for latest stable version at implementation time. Syncfusion requires a license for commercial use (community license available for small teams).

**Alternative Libraries (if licensing is a concern):**
- EPPlus (commercial license required for some uses)
- ClosedXML (open-source, based on OpenXML SDK)
- NPOI (open-source, less feature-rich)

#### No Additional Client Packages Required
- Blazor and JSRuntime are already available

---

### Dependency Injection Configuration

#### IkeaDocuScan-Web/Program.cs

Add after existing service registrations:

```csharp
// Excel Reporting Services
builder.Services.AddExcelReporting();
```

**Location:** After line ~120 (after existing service registrations)

**Full DI Registration in ExcelReporting:**
```csharp
public static IServiceCollection AddExcelReporting(this IServiceCollection services)
{
    services.AddSingleton<PropertyMetadataExtractor>(); // Thread-safe caching
    services.AddScoped<IExcelExportService, ExcelExportService>();
    return services;
}
```

---

### Project References

#### IkeaDocuScan-Web.csproj
```xml
<ItemGroup>
  <ProjectReference Include="..\ExcelReporting\ExcelReporting.csproj" />
</ItemGroup>
```

#### IkeaDocuScan-Web.Client.csproj (if client-side metadata needed)
```xml
<ItemGroup>
  <ProjectReference Include="..\ExcelReporting\ExcelReporting.csproj" />
</ItemGroup>
```

#### IkeaDocuScan.Shared.csproj
```xml
<ItemGroup>
  <ProjectReference Include="..\ExcelReporting\ExcelReporting.csproj" />
</ItemGroup>
```

---

### JavaScript File Registration

#### IkeaDocuScan-Web.Client/wwwroot/index.html

Add before closing `</body>` tag:

```html
<script src="js/excelDownload.js"></script>
```

---

### Configuration (Optional)

If export options should be configurable via `appsettings.json`:

#### appsettings.json
```json
{
  "ExcelExport": {
    "DefaultRowsPerPage": 25,
    "MaxRowsPerPage": 100,
    "DefaultSheetName": "Export",
    "HeaderBackgroundColor": "#4472C4",
    "HeaderFontColor": "#FFFFFF"
  }
}
```

#### Create Configuration Class
```csharp
// IkeaDocuScan.Shared/Configuration/ExcelExportConfiguration.cs
public class ExcelExportConfiguration
{
    public int DefaultRowsPerPage { get; set; } = 25;
    public int MaxRowsPerPage { get; set; } = 100;
    public string DefaultSheetName { get; set; } = "Export";
    public string HeaderBackgroundColor { get; set; } = "#4472C4";
    public string HeaderFontColor { get; set; } = "#FFFFFF";
}
```

#### Register Configuration
```csharp
// Program.cs
builder.Services.Configure<ExcelExportConfiguration>(
    builder.Configuration.GetSection("ExcelExport"));
```

---

## Blazor UI/UX Considerations

### 1. Grid Rendering Performance

**Challenge:** Rendering large datasets in the browser can be slow.

**Solutions:**
- Implement virtual scrolling (Blazorise DataGrid supports this)
- Use paging to limit rendered rows (default 25)
- Consider server-side filtering and paging for very large datasets
- Use `@key` directive to optimize re-renders

**Recommended Approach:**
```razor
<DataGrid TItem="T" Data="@PagedData" Responsive>
    <DataGridColumns>
        @foreach (var column in Columns)
        {
            <DataGridColumn Field="@column.Property.Name" Caption="@column.DisplayName">
                <DisplayTemplate>
                    @column.GetFormattedValue(context)
                </DisplayTemplate>
            </DataGridColumn>
        }
    </DataGridColumns>
</DataGrid>
```

**Alternative (Plain HTML Table):**
- Simpler, more control over styling
- Better for initial implementation
- Can upgrade to Blazorise DataGrid later if needed

---

### 2. User Feedback During Export

**Challenge:** Excel generation may take several seconds for large datasets.

**Solutions:**
- Show loading spinner with message: "Generating Excel file..."
- Disable export button during generation
- Show progress bar if possible (requires background task)
- Display success message after download starts

**Implementation:**
```razor
@if (isExporting)
{
    <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Generating Excel...</span>
    </div>
    <p>Generating Excel file, please wait...</p>
}
else
{
    <button class="btn btn-primary" @onclick="ExportToExcel">
        <i class="bi bi-download"></i> Download as Excel
    </button>
}
```

---

### 3. Responsive Design

**Challenge:** Data grid must work on various screen sizes.

**Solutions:**
- Use horizontal scrolling for wide tables on mobile
- Consider collapsible columns or detail rows for mobile
- Show fewer columns on mobile, expand on click
- Use Blazorise's responsive table classes

**CSS:**
```css
.excel-preview-table {
    overflow-x: auto;
    max-width: 100%;
}

@media (max-width: 768px) {
    .excel-preview-table th,
    .excel-preview-table td {
        white-space: nowrap;
        padding: 0.5rem;
        font-size: 0.875rem;
    }
}
```

---

### 4. Paging Controls UX

**Design:**
```
[Rows per page: [25 ▼]]  [Page 1 of 10]  [◄ Previous] [Next ►]
```

**Best Practices:**
- Place paging controls above and below grid (for long tables)
- Show total record count: "Showing 1-25 of 247 records"
- Disable Previous button on first page
- Disable Next button on last page
- Remember user's rows-per-page preference (local storage)

**Enhanced Implementation:**
```razor
<div class="d-flex justify-content-between align-items-center mb-3">
    <div>
        <label>Rows per page:</label>
        <select class="form-select d-inline-block w-auto" @bind="rowsPerPage">
            <option value="10">10</option>
            <option value="25">25</option>
            <option value="100">100</option>
        </select>
    </div>
    <div>
        Showing @((currentPage - 1) * rowsPerPage + 1)-@(Math.Min(currentPage * rowsPerPage, totalRecords))
        of @totalRecords records
    </div>
    <div>
        <button class="btn btn-sm btn-secondary" @onclick="FirstPage" disabled="@(currentPage == 1)">
            First
        </button>
        <button class="btn btn-sm btn-secondary" @onclick="PreviousPage" disabled="@(currentPage == 1)">
            Previous
        </button>
        <span class="mx-2">Page @currentPage of @totalPages</span>
        <button class="btn btn-sm btn-secondary" @onclick="NextPage" disabled="@(currentPage >= totalPages)">
            Next
        </button>
        <button class="btn btn-sm btn-secondary" @onclick="LastPage" disabled="@(currentPage >= totalPages)">
            Last
        </button>
    </div>
</div>
```

---

### 5. Accessibility

**Requirements:**
- Table headers use `<th>` with `scope="col"`
- Buttons have descriptive labels
- Loading state announced to screen readers
- Keyboard navigation supported
- Focus management after export

**ARIA Attributes:**
```razor
<button class="btn btn-primary"
        @onclick="ExportToExcel"
        aria-label="Download data as Excel file"
        disabled="@isExporting">
    Download as Excel
</button>

<div role="status" aria-live="polite" class="visually-hidden">
    @if (isExporting)
    {
        <text>Generating Excel file</text>
    }
</div>
```

---

### 6. Error Handling UI

**Scenarios:**
- No data to export
- Export service failure
- Browser download blocked
- Network error (if using API endpoint)

**Implementation:**
```razor
@if (errorMessage != null)
{
    <div class="alert alert-danger alert-dismissible fade show" role="alert">
        <strong>Export Failed:</strong> @errorMessage
        <button type="button" class="btn-close" @onclick="() => errorMessage = null"></button>
    </div>
}

@if (Data == null || !Data.Any())
{
    <div class="alert alert-warning" role="alert">
        No data available to export.
    </div>
}
```

---

### 7. Preview vs. Direct Export

**Two UX Patterns:**

#### Pattern A: Preview Then Export (Recommended)
1. User clicks "Export to Excel" button
2. Navigate to ExcelPreview page with data
3. User reviews data in grid
4. User clicks "Download" to export
5. **Pros:** User can verify data before downloading
6. **Cons:** Extra navigation step

#### Pattern B: Direct Export
1. User clicks "Export to Excel" button
2. Excel file downloads immediately
3. **Pros:** Faster for users who trust the data
4. **Cons:** No preview, potential for unwanted downloads

**Recommendation:** Offer both options:
- "Preview and Export" button (Pattern A)
- "Export Now" button with confirmation dialog (Pattern B)

---

### 8. Styling and Theming

**Grid Styling:**
```css
.excel-preview-grid {
    margin: 1rem 0;
}

.excel-preview-grid table {
    width: 100%;
    border-collapse: collapse;
}

.excel-preview-grid th {
    background-color: #4472C4;
    color: white;
    padding: 0.75rem;
    text-align: left;
    font-weight: 600;
}

.excel-preview-grid td {
    padding: 0.75rem;
    border-bottom: 1px solid #dee2e6;
}

.excel-preview-grid tr:hover {
    background-color: #f8f9fa;
}

.excel-preview-grid .text-end {
    text-align: right; /* For numbers and currency */
}
```

**Apply Data Type Styling:**
```razor
<td class="@GetCellClass(column.DataType)">
    @column.GetFormattedValue(item)
</td>

@code {
    private string GetCellClass(ExcelDataType dataType)
    {
        return dataType switch
        {
            ExcelDataType.Number => "text-end",
            ExcelDataType.Currency => "text-end",
            ExcelDataType.Percentage => "text-end",
            ExcelDataType.Date => "text-nowrap",
            _ => ""
        };
    }
}
```

---

## Testing and Validation Strategy

### 1. Unit Testing Strategy

#### ExcelReporting Library Tests

**Test Project:** `ExcelReporting.Tests` (xUnit or NUnit)

**Test Coverage:**

##### PropertyMetadataExtractor Tests
```csharp
[Fact]
public void ExtractMetadata_WithValidDto_ReturnsOrderedMetadata()
{
    // Arrange
    var extractor = new PropertyMetadataExtractor();

    // Act
    var metadata = extractor.ExtractMetadata<SampleDto>();

    // Assert
    Assert.NotEmpty(metadata);
    Assert.True(metadata[0].Order <= metadata[1].Order);
}

[Fact]
public void ExtractMetadata_CachesResults_OnSecondCall()
{
    // Verify caching improves performance
}

[Fact]
public void ExtractMetadata_FiltersNonExportableProperties()
{
    // Test IsExportable = false
}
```

##### ExcelExportMetadata Tests
```csharp
[Theory]
[InlineData(ExcelDataType.Date, "2025-01-27", "yyyy-MM-dd")]
[InlineData(ExcelDataType.Currency, "$1,234.56", "$#,##0.00")]
public void GetFormattedValue_FormatsCorrectly(ExcelDataType type, string expected, string format)
{
    // Test formatting for each data type
}

[Fact]
public void GetFormattedValue_HandlesNullValues()
{
    // Ensure no exceptions on null
}
```

##### ExcelExportService Tests (Integration)
```csharp
[Fact]
public async Task GenerateExcelAsync_WithSampleData_ReturnsValidStream()
{
    // Arrange
    var service = new ExcelExportService(new PropertyMetadataExtractor());
    var data = GetSampleData();

    // Act
    var stream = await service.GenerateExcelAsync(data);

    // Assert
    Assert.NotNull(stream);
    Assert.True(stream.Length > 0);
}

[Fact]
public async Task GenerateExcelAsync_WithEmptyData_ReturnsHeaderOnly()
{
    // Test edge case
}

[Fact]
public async Task GenerateExcelAsync_WithLargeDataset_CompletesInReasonableTime()
{
    // Performance test with 10,000 rows
    var data = Enumerable.Range(1, 10000).Select(i => new SampleDto { Id = i });
    var stopwatch = Stopwatch.StartNew();

    await service.GenerateExcelAsync(data);

    Assert.True(stopwatch.ElapsedMilliseconds < 5000); // 5 seconds max
}
```

---

### 2. Integration Testing

#### Blazor Component Tests

**Test Framework:** bUnit

**Test Coverage:**

```csharp
[Fact]
public void ExcelPreview_RendersGridWithData()
{
    // Arrange
    using var ctx = new TestContext();
    var data = GetSampleData();
    var excelService = new Mock<IExcelExportService>();
    ctx.Services.AddSingleton(excelService.Object);

    // Act
    var cut = ctx.RenderComponent<ExcelPreview<SampleDto>>(parameters => parameters
        .Add(p => p.Data, data));

    // Assert
    cut.MarkupMatches("<table>...</table>"); // Verify grid structure
}

[Fact]
public void ExcelPreview_PagingWorks()
{
    // Test Next/Previous buttons
}

[Fact]
public async Task ExcelPreview_ExportTriggersDownload()
{
    // Mock ExcelExportService
    // Verify GenerateExcelAsync called
    // Verify JSRuntime.InvokeVoidAsync called
}
```

---

### 3. End-to-End Testing

#### Manual Test Scenarios

**Test Scenario 1: Basic Export Flow**
1. Navigate to Documents page
2. Click "Export to Excel" button
3. Verify ExcelPreview page loads
4. Verify grid displays first 25 documents
5. Click "Download as Excel" button
6. Verify Excel file downloads
7. Open Excel file
8. Verify:
   - All columns present with correct headers
   - Data matches grid preview
   - Formatting applied (dates, currency, etc.)
   - Header row is bold with background color
   - Auto-filter enabled

**Test Scenario 2: Paging**
1. Load ExcelPreview with 100 documents
2. Verify page shows "Page 1 of 4" (25 rows per page)
3. Click "Next" button
4. Verify page shows "Page 2 of 4"
5. Verify different data displayed
6. Change rows per page to 100
7. Verify page shows "Page 1 of 1"
8. Verify all 100 rows visible

**Test Scenario 3: Large Dataset**
1. Load ExcelPreview with 5,000 documents
2. Verify grid loads within 2 seconds
3. Click "Download as Excel" button
4. Verify loading indicator appears
5. Wait for download to complete
6. Verify Excel file size is reasonable (<10MB)
7. Open Excel file
8. Verify all 5,000 rows present

**Test Scenario 4: Empty Data**
1. Load ExcelPreview with no data
2. Verify message: "No data available to export"
3. Verify "Download" button disabled or hidden

**Test Scenario 5: Different DTO Types**
1. Create second DTO type (e.g., CounterPartyExportDto)
2. Load ExcelPreview with counter party data
3. Verify different columns displayed based on attributes
4. Export and verify Excel contents

---

### 4. Browser Compatibility Testing

**Browsers to Test:**
- Chrome (latest)
- Edge (latest)
- Firefox (latest)
- Safari (latest, if accessible)

**Test Matrix:**

| Feature | Chrome | Edge | Firefox | Safari |
|---------|--------|------|---------|--------|
| Grid rendering | ✓ | ✓ | ✓ | ✓ |
| Paging controls | ✓ | ✓ | ✓ | ✓ |
| Excel download | ✓ | ✓ | ✓ | ✓ |
| File opens correctly | ✓ | ✓ | ✓ | ✓ |

---

### 5. Performance Testing

**Metrics to Measure:**

1. **Grid Rendering Time**
   - Target: <1 second for 25 rows
   - Target: <2 seconds for 100 rows

2. **Export Generation Time**
   - Target: <2 seconds for 100 rows
   - Target: <5 seconds for 1,000 rows
   - Target: <30 seconds for 10,000 rows

3. **Memory Usage**
   - Monitor during export of large datasets
   - Ensure no memory leaks on repeated exports

4. **Download Size**
   - Verify Excel file size is reasonable
   - Compare to CSV export size

**Performance Test Code:**
```csharp
[Theory]
[InlineData(100)]
[InlineData(1000)]
[InlineData(10000)]
public async Task ExportPerformance_WithVariousDataSizes(int rowCount)
{
    var data = Enumerable.Range(1, rowCount).Select(i => new DocumentExportDto
    {
        Id = i,
        Name = $"Document {i}",
        // ... populate all properties
    });

    var service = new ExcelExportService(new PropertyMetadataExtractor());
    var stopwatch = Stopwatch.StartNew();

    var stream = await service.GenerateExcelAsync(data);

    stopwatch.Stop();

    Console.WriteLine($"{rowCount} rows exported in {stopwatch.ElapsedMilliseconds}ms");
    Console.WriteLine($"File size: {stream.Length / 1024}KB");

    Assert.True(stream.Length > 0);
}
```

---

### 6. Syncfusion XlsIO Validation

**Verify Excel File Integrity:**

1. **Open in Microsoft Excel**
   - Verify no warnings or errors
   - Verify all data visible
   - Verify formatting applied

2. **Open in Google Sheets**
   - Verify compatibility
   - Verify formatting preserved

3. **Open in LibreOffice Calc**
   - Verify open-source compatibility

4. **Validate Excel Format**
   - Use OpenXML SDK to inspect file structure
   - Verify XLSX format compliance

---

### 7. Validation Test Checklist

**Before Considering Feature Complete:**

- [ ] All unit tests passing (>80% code coverage in ExcelReporting library)
- [ ] All integration tests passing
- [ ] Manual testing completed in all browsers
- [ ] Performance benchmarks meet targets
- [ ] Excel files open correctly in Excel, Google Sheets, LibreOffice
- [ ] Accessibility validated (keyboard navigation, screen readers)
- [ ] Error handling tested (network failures, empty data, invalid data)
- [ ] Documentation updated (README, XML comments, usage examples)
- [ ] Code review completed
- [ ] Deployed to test environment and validated by stakeholders

---

## Extensibility Considerations

### 1. Supporting New DTO Types

**Goal:** Allow developers to add new exportable DTOs with minimal effort.

**Process:**
1. Create new DTO class in `IkeaDocuScan.Shared/DTOs/Excel/`
2. Inherit from `ExportableBase`
3. Add properties with `[ExcelExport]` attributes
4. No changes to ExcelReporting library required
5. No changes to Blazor component required (if using same component)

**Example:**
```csharp
public class CounterPartyExportDto : ExportableBase
{
    [ExcelExport("Counter Party ID", ExcelDataType.Number, Order = 1)]
    public int Id { get; set; }

    [ExcelExport("Counter Party Name", ExcelDataType.String, Order = 2)]
    public string Name { get; set; }

    [ExcelExport("Active", ExcelDataType.Boolean, Order = 3)]
    public bool IsActive { get; set; }
}
```

**Usage:**
```razor
<ExcelPreview T="CounterPartyExportDto" Data="@counterParties" />
```

---

### 2. Custom Data Types

**Scenario:** Need to support new data type not in `ExcelDataType` enum.

**Solution:**
1. Add new enum value to `ExcelDataType`
2. Update `ExcelExportService` to handle new type in switch statement
3. Update `ExcelExportMetadata.GetFormattedValue()` if needed
4. No changes to DTOs or components required

**Example:**
```csharp
public enum ExcelDataType
{
    // Existing types...
    PhoneNumber,  // NEW: Format as (123) 456-7890
    Email,        // NEW: Create mailto: hyperlink
    Url           // NEW: Create clickable hyperlink
}
```

---

### 3. Custom Formatting Rules

**Scenario:** Need complex formatting beyond simple format strings.

**Solution:** Add optional `IValueFormatter` interface

**Design:**
```csharp
public interface IValueFormatter
{
    string Format(object value, ExcelDataType dataType, string format);
}

// Example implementation
public class CustomDocumentFormatter : IValueFormatter
{
    public string Format(object value, ExcelDataType dataType, string format)
    {
        if (dataType == ExcelDataType.Currency && value is decimal amount)
        {
            // Custom logic: show negative amounts in red
            return amount < 0 ? $"({Math.Abs(amount):C})" : $"{amount:C}";
        }
        return value?.ToString();
    }
}
```

**Integration:**
```csharp
services.AddExcelReporting(options =>
{
    options.UseFormatter<CustomDocumentFormatter>();
});
```

---

### 4. Conditional Formatting

**Scenario:** Apply Excel conditional formatting rules (color scales, data bars, etc.)

**Solution:** Extend `ExcelExportOptions`

**Design:**
```csharp
public class ExcelExportOptions
{
    // Existing properties...

    public List<ConditionalFormatRule> ConditionalFormats { get; set; }
}

public class ConditionalFormatRule
{
    public string PropertyName { get; set; }
    public ConditionalFormatType Type { get; set; } // ColorScale, DataBar, IconSet
    public Dictionary<string, object> Parameters { get; set; }
}
```

**Usage:**
```csharp
var options = new ExcelExportOptions
{
    ConditionalFormats = new List<ConditionalFormatRule>
    {
        new ConditionalFormatRule
        {
            PropertyName = "Amount",
            Type = ConditionalFormatType.ColorScale,
            Parameters = new Dictionary<string, object>
            {
                { "MinColor", "#FF0000" },
                { "MidColor", "#FFFF00" },
                { "MaxColor", "#00FF00" }
            }
        }
    }
};
```

---

### 5. Multiple Sheets

**Scenario:** Export related data to multiple sheets in one workbook.

**Solution:** Add overload to `IExcelExportService`

**Design:**
```csharp
public interface IExcelExportService
{
    // Existing methods...

    Task<MemoryStream> GenerateMultiSheetExcelAsync(
        Dictionary<string, (Type type, IEnumerable<ExportableBase> data)> sheets,
        ExcelExportOptions options = null);
}
```

**Usage:**
```csharp
var sheets = new Dictionary<string, (Type, IEnumerable<ExportableBase>)>
{
    { "Documents", (typeof(DocumentExportDto), documents) },
    { "Counter Parties", (typeof(CounterPartyExportDto), counterParties) }
};

var stream = await excelService.GenerateMultiSheetExcelAsync(sheets);
```

---

### 6. Custom Excel Templates

**Scenario:** Use pre-designed Excel template with logo, headers, etc.

**Solution:** Add template support to `ExcelExportOptions`

**Design:**
```csharp
public class ExcelExportOptions
{
    // Existing properties...

    public Stream TemplateStream { get; set; }
    public int DataStartRow { get; set; } = 1; // Row to start writing data
}
```

**Usage:**
```csharp
var template = File.OpenRead("ReportTemplate.xlsx");
var options = new ExcelExportOptions
{
    TemplateStream = template,
    DataStartRow = 5 // Start after header section
};
```

---

### 7. Export to Other Formats

**Scenario:** Support CSV, PDF, or other formats.

**Solution:** Create abstraction over export service

**Design:**
```csharp
public interface IDataExportService
{
    Task<Stream> ExportAsync<T>(
        IEnumerable<T> data,
        ExportFormat format,
        ExportOptions options = null)
        where T : ExportableBase;
}

public enum ExportFormat
{
    Excel,
    Csv,
    Pdf,
    Json
}
```

**Implementation:**
```csharp
public class DataExportService : IDataExportService
{
    private readonly IExcelExportService _excelService;
    private readonly ICsvExportService _csvService;
    private readonly IPdfExportService _pdfService;

    public async Task<Stream> ExportAsync<T>(...)
    {
        return format switch
        {
            ExportFormat.Excel => await _excelService.GenerateExcelAsync(data, options),
            ExportFormat.Csv => await _csvService.GenerateCsvAsync(data, options),
            ExportFormat.Pdf => await _pdfService.GeneratePdfAsync(data, options),
            _ => throw new NotSupportedException()
        };
    }
}
```

---

### 8. Localization Support

**Scenario:** Support multiple languages for display names and formats.

**Solution:** Use resource files with attribute

**Design:**
```csharp
[ExcelExport("Document_Name", ExcelDataType.String, UseResourceFile = true)]
public string Name { get; set; }
```

**Resource File (Resources.resx):**
```
Document_Name = "Document Name"      (English)
Document_Name = "Nom du document"    (French)
```

**Implementation:**
```csharp
public class ExcelExportAttribute
{
    public bool UseResourceFile { get; set; }

    public string GetLocalizedDisplayName(CultureInfo culture)
    {
        if (UseResourceFile)
        {
            return ResourceManager.GetString(DisplayName, culture) ?? DisplayName;
        }
        return DisplayName;
    }
}
```

---

### 9. Asynchronous Export with Progress

**Scenario:** Long-running exports with progress updates.

**Solution:** Use background service with SignalR for progress

**Design:**
```csharp
public interface IExcelExportService
{
    Task<string> QueueExportAsync<T>(
        IEnumerable<T> data,
        ExcelExportOptions options,
        string connectionId);  // SignalR connection ID

    Task<Stream> GetExportResultAsync(string exportId);
}
```

**Progress Updates:**
```csharp
// Server-side
await hubContext.Clients.Client(connectionId).SendAsync(
    "ExportProgress",
    new { Percentage = 50, Message = "Processing row 500 of 1000" });

// Client-side
hubConnection.On<object>("ExportProgress", progress =>
{
    UpdateProgressBar(progress.Percentage);
});
```

---

### 10. Plugin Architecture

**Scenario:** Allow third-party extensions without modifying core library.

**Solution:** Plugin interface for custom export providers

**Design:**
```csharp
public interface IExportProvider
{
    string Name { get; }
    string FileExtension { get; }
    Task<Stream> ExportAsync<T>(IEnumerable<T> data, ExportOptions options);
}

// Plugin registration
services.AddExcelReporting(options =>
{
    options.RegisterProvider(new CustomExportProvider());
});
```

---

## Open Questions and Assumptions

### Open Questions

#### 1. Syncfusion Licensing
**Question:** Does the organization have a Syncfusion license, or do we need to evaluate alternatives?

**Impact:** High - affects library choice
**Recommendation:** Verify license before implementation. If no license, consider ClosedXML (open-source) or EPPlus as alternatives.

---

#### 2. Server-Side vs. Client-Side Export
**Question:** Should Excel generation happen client-side (WebAssembly) or server-side with API endpoint?

**Trade-offs:**
- **Client-Side:** Reduces server load, but requires large WebAssembly download (Syncfusion is ~5MB)
- **Server-Side:** Better performance, smaller client bundle, but increases server load

**Current Assumption:** Client-side for initial implementation (matches ExcelPreview component design)
**Recommendation:** Consider server-side API endpoint for very large exports (>5,000 rows)

---

#### 3. Maximum Export Size
**Question:** What is the maximum number of rows we should support in a single export?

**Impact:** Affects performance testing and UI design
**Recommendation:**
- Soft limit: 10,000 rows (warning message)
- Hard limit: 50,000 rows (enforce pagination or background job)
- Consider streaming export for larger datasets

---

#### 4. Authentication for Export Endpoint
**Question:** If using server-side API endpoint, how should it be secured?

**Current Pattern:** Use `[Authorize(Policy = "HasAccess")]` attribute
**Assumption:** User must have read access to the data being exported
**Recommendation:** Apply same authorization rules as viewing the data

---

#### 5. Audit Trail Integration
**Question:** Should Excel exports be logged in the audit trail?

**Impact:** Compliance and security
**Recommendation:** Yes, log export actions with:
- User ID
- Export timestamp
- DTO type (e.g., "DocumentExportDto")
- Row count
- Filters applied (if any)

**Implementation:**
```csharp
await auditTrailService.LogAsync(
    AuditAction.Export,
    $"Exported {data.Count()} documents to Excel",
    currentUser.Id);
```

---

#### 6. Export Filter Context
**Question:** Should the ExcelPreview component accept filter parameters to show context?

**Example:** "Showing documents for Counter Party: Acme Corp, Date Range: 2025-01-01 to 2025-01-31"

**Recommendation:** Yes, add optional parameter:
```csharp
[Parameter]
public Dictionary<string, string> FilterContext { get; set; }
```

Display above grid:
```razor
@if (FilterContext != null && FilterContext.Any())
{
    <div class="alert alert-info">
        <strong>Filters Applied:</strong>
        @foreach (var filter in FilterContext)
        {
            <span class="badge bg-secondary">@filter.Key: @filter.Value</span>
        }
    </div>
}
```

---

#### 7. Reusable vs. Page-Specific Component
**Question:** Should ExcelPreview be a reusable component or page-specific implementation?

**Current Design:** Reusable generic component
**Alternative:** Create specific pages (DocumentExcelPreview.razor, CounterPartyExcelPreview.razor)

**Recommendation:** Start with reusable generic component for consistency. Create specific pages only if custom UI logic is needed.

---

#### 8. Blazorise DataGrid vs. Plain HTML Table
**Question:** Should we use Blazorise DataGrid component or plain HTML table?

**Trade-offs:**
- **Blazorise DataGrid:** Rich features (sorting, filtering), but adds complexity
- **Plain HTML Table:** Simpler, more control, easier to customize

**Current Assumption:** Plain HTML table for initial implementation
**Recommendation:** Can upgrade to Blazorise DataGrid later if advanced features needed

---

#### 9. Export History
**Question:** Should we maintain a history of exports (downloadable again without regenerating)?

**Impact:** Storage requirements, added complexity
**Recommendation:** Phase 2 feature. For initial implementation, generate on-demand only.

---

#### 10. Scheduled/Background Exports
**Question:** Should we support scheduled or background exports for large datasets?

**Use Case:** User requests large export (50,000 rows), receives email when ready
**Impact:** Requires background job infrastructure (Hangfire, Azure Functions, etc.)

**Recommendation:** Phase 2 feature if needed. Initial implementation supports on-demand only.

---

### Assumptions

#### 1. Target Framework
**Assumption:** .NET 9.0 is the primary target; .NET 10.0 compatibility is bonus
**Validation:** Confirm with project standards

---

#### 2. Existing DTO Structure
**Assumption:** Existing DTOs (DocumentDto, CounterPartyDto, etc.) will NOT be modified. New export-specific DTOs will be created.

**Rationale:** Avoid breaking existing API contracts
**Validation:** Confirm with architectural guidelines

---

#### 3. Single-User Export
**Assumption:** Exports are single-user operations (no multi-user collaboration on export generation)

**Implication:** No need for locking or concurrent export management

---

#### 4. Excel Format
**Assumption:** XLSX (Office Open XML) format is sufficient; no need for legacy XLS support

**Validation:** Confirm all users have Excel 2007 or later / compatible software

---

#### 5. Language and Culture
**Assumption:** Export formatting uses server culture settings (or user culture if available)

**Example:** Date format "MM/dd/yyyy" (US) vs. "dd/MM/yyyy" (EU)
**Recommendation:** Use `ExcelExportOptions` to allow per-export culture override

---

#### 6. No Sensitive Data Protection
**Assumption:** Excel exports do not require encryption or password protection

**If Incorrect:** Add optional password parameter to `ExcelExportOptions` (Syncfusion supports this)

---

#### 7. Browser Download
**Assumption:** All target browsers support Blob download via JavaScript

**Validation:** Test in all supported browsers (see Browser Compatibility Testing)

---

#### 8. Memory Constraints
**Assumption:** Server/client has sufficient memory for in-memory Excel generation

**Typical Usage:** 10,000 rows × 10 columns × ~100 bytes = ~10MB
**If Incorrect:** Implement streaming export to disk, then download

---

#### 9. No Real-Time Updates
**Assumption:** Grid preview does not need real-time updates via SignalR

**Rationale:** Export is a snapshot in time
**If Incorrect:** Add SignalR subscription to refresh grid when data changes

---

#### 10. Component Lifecycle
**Assumption:** ExcelPreview component follows standard Blazor WebAssembly lifecycle

**Implication:**
- Prerendering disabled (`prerender: false`)
- JavaScript interop initialized in `OnAfterRenderAsync`
- No server-side prerendering assumptions

---

## Summary

This implementation plan provides a comprehensive roadmap for adding Excel reporting functionality to the IkeaDocuScan application. The design follows existing architectural patterns, emphasizes separation of concerns, and prioritizes extensibility.

### Key Deliverables

1. **ExcelReporting Class Library** - Standalone, reusable DLL for Excel generation
2. **Metadata-Driven Attributes** - Flexible configuration via `[ExcelExport]` attribute
3. **Dynamic Blazor Component** - Reusable preview and export page
4. **Integration with Syncfusion XlsIO** - Professional Excel generation with formatting
5. **Comprehensive Testing Strategy** - Unit, integration, and performance tests

### Next Steps

1. **Review and Approval:** Stakeholders review this plan and approve
2. **Licensing Verification:** Confirm Syncfusion license or select alternative library
3. **Implementation:** Follow the phased implementation sequence (Phases 1-7)
4. **Testing:** Execute comprehensive testing strategy
5. **Documentation:** Update developer guides and user documentation
6. **Deployment:** Deploy to test environment for stakeholder validation

### Estimated Total Effort

- **Phase 1:** 4-6 hours (Foundation)
- **Phase 2:** 6-8 hours (Syncfusion Integration)
- **Phase 3:** 1-2 hours (DI Configuration)
- **Phase 4:** 2-3 hours (Sample DTOs)
- **Phase 5:** 6-8 hours (Blazor Component)
- **Phase 6:** 3-4 hours (Integration)
- **Phase 7:** 4-6 hours (Testing)

**Total:** 26-37 hours (3-5 developer days)

---

**Document Status:** Ready for Review
**Prepared By:** Claude (AI Assistant)
**Date:** 2025-01-27
