using System.Collections.Concurrent;
using System.Reflection;
using ExcelReporting.Attributes;
using ExcelReporting.Models;

namespace ExcelReporting.Services;

/// <summary>
/// Extracts and caches metadata from DTO types using reflection
/// </summary>
public class PropertyMetadataExtractor
{
    private readonly ConcurrentDictionary<Type, List<ExcelExportMetadata>> _metadataCache;

    public PropertyMetadataExtractor()
    {
        _metadataCache = new ConcurrentDictionary<Type, List<ExcelExportMetadata>>();
    }

    /// <summary>
    /// Extracts metadata for a generic type
    /// </summary>
    /// <typeparam name="T">DTO type inheriting from ExportableBase</typeparam>
    /// <returns>List of metadata for exportable properties</returns>
    public List<ExcelExportMetadata> ExtractMetadata<T>() where T : ExportableBase
    {
        return ExtractMetadata(typeof(T));
    }

    /// <summary>
    /// Extracts metadata for a Type object
    /// </summary>
    /// <param name="type">DTO type to extract metadata from</param>
    /// <returns>List of metadata for exportable properties</returns>
    public List<ExcelExportMetadata> ExtractMetadata(Type type)
    {
        if (!typeof(ExportableBase).IsAssignableFrom(type))
        {
            throw new ArgumentException($"Type {type.Name} must inherit from ExportableBase", nameof(type));
        }

        return _metadataCache.GetOrAdd(type, ExtractMetadataInternal);
    }

    /// <summary>
    /// Internal method that performs the actual metadata extraction
    /// </summary>
    private List<ExcelExportMetadata> ExtractMetadataInternal(Type type)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var metadata = new List<ExcelExportMetadata>();

        foreach (var property in properties)
        {
            var attribute = property.GetCustomAttribute<ExcelExportAttribute>();

            if (attribute != null && attribute.IsExportable)
            {
                metadata.Add(new ExcelExportMetadata
                {
                    Property = property,
                    DisplayName = attribute.DisplayName,
                    DataType = attribute.DataType,
                    Format = attribute.Format,
                    Order = attribute.Order,
                    IsExportable = attribute.IsExportable
                });
            }
        }

        // Sort by Order first, then by DisplayName for consistent column ordering
        return metadata
            .OrderBy(m => m.Order)
            .ThenBy(m => m.DisplayName)
            .ToList();
    }

    /// <summary>
    /// Clears the metadata cache (useful for testing or hot-reload scenarios)
    /// </summary>
    public void ClearCache()
    {
        _metadataCache.Clear();
    }

    /// <summary>
    /// Gets the number of cached types
    /// </summary>
    public int CachedTypeCount => _metadataCache.Count;
}
