using ExcelReporting.Models;
using Syncfusion.XlsIO;
using Syncfusion.Drawing;
using ExportDataType = ExcelReporting.Models.ExcelDataType;

namespace ExcelReporting.Services;

/// <summary>
/// Implementation of Excel export service using Syncfusion XlsIO
/// </summary>
public class ExcelExportService : IExcelExportService
{
    private readonly PropertyMetadataExtractor _metadataExtractor;

    public ExcelExportService(PropertyMetadataExtractor metadataExtractor)
    {
        _metadataExtractor = metadataExtractor;
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JEaF5cXmRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXhed3RSRWJeUEV3W0pWYEk=");
    }

    /// <summary>
    /// Generates an Excel file from a collection of DTOs
    /// </summary>
    public async Task<MemoryStream> GenerateExcelAsync<T>(
        IEnumerable<T> data,
        ExcelExportOptions? options = null)
        where T : ExportableBase
    {
        return await Task.Run(() => GenerateExcel(data, options));
    }

    /// <summary>
    /// Synchronous Excel generation method
    /// </summary>
    private MemoryStream GenerateExcel<T>(
        IEnumerable<T> data,
        ExcelExportOptions? options) where T : ExportableBase
    {
        options ??= new ExcelExportOptions();

        // Prepare data for export
        var dataList = data.ToList();
        foreach (var item in dataList)
        {
            item.PrepareForExport();
        }

        // Extract metadata
        var metadata = _metadataExtractor.ExtractMetadata<T>();

        // Create Excel engine and workbook
        using var excelEngine = new ExcelEngine();
        var application = excelEngine.Excel;
        application.DefaultVersion = ExcelVersion.Xlsx;

        var workbook = application.Workbooks.Create(1);
        var worksheet = workbook.Worksheets[0];
        worksheet.Name = options.SheetName;

        int currentRow = 1;

        // Write header row
        if (options.IncludeHeader)
        {
            WriteHeaderRow(worksheet, metadata, options, currentRow);
            currentRow++;
        }

        // Write data rows
        WriteDataRows(worksheet, metadata, dataList, currentRow);

        // Apply formatting
        ApplyFormatting(worksheet, metadata, options, dataList.Count);

        // Save to memory stream
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return stream;
    }

    /// <summary>
    /// Writes the header row with column names
    /// </summary>
    private void WriteHeaderRow(
        IWorksheet worksheet,
        List<ExcelExportMetadata> metadata,
        ExcelExportOptions options,
        int row)
    {
        for (int col = 0; col < metadata.Count; col++)
        {
            var cell = worksheet.Range[row, col + 1];
            cell.Text = metadata[col].DisplayName;

            if (options.ApplyHeaderFormatting)
            {
                cell.CellStyle.Font.Bold = true;
                cell.CellStyle.Color = ConvertHexToColor(options.HeaderBackgroundColor);
                cell.CellStyle.Font.Color = ExcelKnownColors.White;
                cell.CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                cell.CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
            }
        }
    }

    /// <summary>
    /// Writes data rows
    /// </summary>
    private void WriteDataRows<T>(
        IWorksheet worksheet,
        List<ExcelExportMetadata> metadata,
        List<T> data,
        int startRow) where T : ExportableBase
    {
        for (int rowIndex = 0; rowIndex < data.Count; rowIndex++)
        {
            var item = data[rowIndex];
            int excelRow = startRow + rowIndex;

            for (int colIndex = 0; colIndex < metadata.Count; colIndex++)
            {
                var meta = metadata[colIndex];
                var cell = worksheet.Range[excelRow, colIndex + 1];
                var value = meta.GetValue(item);

                if (value != null)
                {
                    SetCellValue(worksheet, cell, value, meta);
                }
            }
        }
    }

    /// <summary>
    /// Sets cell value with appropriate type and formatting
    /// </summary>
    private void SetCellValue(IWorksheet worksheet, IRange cell, object value, ExcelExportMetadata metadata)
    {
        switch (metadata.DataType)
        {
            case ExportDataType.Date:
                if (value is DateTime dt)
                {
                    cell.DateTime = dt;
                    cell.NumberFormat = metadata.Format;
                }
                else
                {
                    cell.Text = value.ToString();
                }
                break;

            case ExportDataType.Number:
                if (IsNumeric(value))
                {
                    cell.Number = Convert.ToDouble(value);
                    cell.NumberFormat = metadata.Format;
                }
                else
                {
                    cell.Text = value.ToString();
                }
                break;

            case ExportDataType.Currency:
                if (IsNumeric(value))
                {
                    cell.Number = Convert.ToDouble(value);
                    cell.NumberFormat = metadata.Format;
                }
                else
                {
                    cell.Text = value.ToString();
                }
                break;

            case ExportDataType.Percentage:
                if (IsNumeric(value))
                {
                    cell.Number = Convert.ToDouble(value);
                    cell.NumberFormat = metadata.Format;
                }
                else
                {
                    cell.Text = value.ToString();
                }
                break;

            case ExportDataType.Boolean:
                if (value is bool b)
                {
                    cell.Text = b ? "Yes" : "No";
                }
                else
                {
                    cell.Text = value.ToString();
                }
                break;

            case ExportDataType.Hyperlink:
                var url = value.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(url))
                {
                    var hyperlink = worksheet.HyperLinks.Add(cell);
                    hyperlink.Type = ExcelHyperLinkType.Url;
                    hyperlink.Address = url;
                    hyperlink.ScreenTip = url;
                    cell.Text = url;
                    cell.CellStyle.Font.Underline = ExcelUnderline.Single;
                    cell.CellStyle.Font.Color = ExcelKnownColors.Blue;
                }
                break;

            default:
                cell.Text = value.ToString();
                break;
        }
    }

    /// <summary>
    /// Checks if a value is numeric
    /// </summary>
    private bool IsNumeric(object value)
    {
        return value is int || value is long || value is decimal ||
               value is double || value is float || value is short ||
               value is byte || value is uint || value is ulong ||
               value is ushort || value is sbyte;
    }

    /// <summary>
    /// Applies formatting to the worksheet
    /// </summary>
    private void ApplyFormatting(
        IWorksheet worksheet,
        List<ExcelExportMetadata> metadata,
        ExcelExportOptions options,
        int dataRowCount)
    {
        // Auto-fit columns
        if (options.AutoFitColumns)
        {
            for (int col = 1; col <= metadata.Count; col++)
            {
                worksheet.AutofitColumn(col);

                // Apply max width if specified
                if (options.MaxColumnWidth.HasValue)
                {
                    var column = worksheet.Columns[col - 1];
                    if (column.ColumnWidth > options.MaxColumnWidth.Value)
                    {
                        column.ColumnWidth = options.MaxColumnWidth.Value;
                    }
                }
            }
        }

        // Freeze header row
        if (options.FreezeHeaderRow && options.IncludeHeader)
        {
            worksheet.Range[2, 1].FreezePanes();
        }

        // Enable auto-filters
        if (options.EnableFilters && options.IncludeHeader && dataRowCount > 0)
        {
            worksheet.AutoFilters.FilterRange = worksheet.Range[1, 1, dataRowCount + 1, metadata.Count];
        }
    }

    /// <summary>
    /// Gets metadata for a generic type
    /// </summary>
    public List<ExcelExportMetadata> GetMetadata<T>() where T : ExportableBase
    {
        return _metadataExtractor.ExtractMetadata<T>();
    }

    /// <summary>
    /// Gets metadata for a Type object
    /// </summary>
    public List<ExcelExportMetadata> GetMetadata(Type type)
    {
        return _metadataExtractor.ExtractMetadata(type);
    }

    /// <summary>
    /// Validates data size against configured limits
    /// </summary>
    public ExcelExportValidationResult ValidateDataSize<T>(
        IEnumerable<T> data,
        ExcelExportOptions options) where T : ExportableBase
    {
        var rowCount = data.Count();

        if (rowCount > options.MaximumRowCount)
        {
            return new ExcelExportValidationResult
            {
                IsValid = false,
                HasWarning = false,
                RowCount = rowCount,
                Message = $"Export exceeds maximum allowed rows ({options.MaximumRowCount:N0}). " +
                         $"Current row count: {rowCount:N0}. Please apply filters to reduce the data size."
            };
        }

        if (rowCount > options.WarningRowCount)
        {
            return new ExcelExportValidationResult
            {
                IsValid = true,
                HasWarning = true,
                RowCount = rowCount,
                Message = $"Large export detected ({rowCount:N0} rows). " +
                         $"This may take several seconds to generate. Continue?"
            };
        }

        return new ExcelExportValidationResult
        {
            IsValid = true,
            HasWarning = false,
            RowCount = rowCount,
            Message = null
        };
    }

    /// <summary>
    /// Converts hex color string to Syncfusion Color
    /// </summary>
    private static Color ConvertHexToColor(string hexColor)
    {
        // Remove # if present
        hexColor = hexColor.TrimStart('#');

        if (hexColor.Length == 6)
        {
            var r = Convert.ToByte(hexColor.Substring(0, 2), 16);
            var g = Convert.ToByte(hexColor.Substring(2, 2), 16);
            var b = Convert.ToByte(hexColor.Substring(4, 2), 16);
            return Color.FromArgb(r, g, b);
        }
        else if (hexColor.Length == 8)
        {
            var a = Convert.ToByte(hexColor.Substring(0, 2), 16);
            var r = Convert.ToByte(hexColor.Substring(2, 2), 16);
            var g = Convert.ToByte(hexColor.Substring(4, 2), 16);
            var b = Convert.ToByte(hexColor.Substring(6, 2), 16);
            return Color.FromArgb(a, r, g, b);
        }

        // Default to black if invalid format
        return Color.Black;
    }
}
