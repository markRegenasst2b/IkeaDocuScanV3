using ExcelReporting.Models;
using ExcelReporting.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExcelReporting.Extensions;

/// <summary>
/// Extension methods for registering Excel reporting services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Excel reporting services to the dependency injection container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Optional configuration for export options</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddExcelReporting(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        // Register PropertyMetadataExtractor as singleton for thread-safe caching
        services.AddSingleton<PropertyMetadataExtractor>();

        // Register IExcelExportService as scoped (matches existing service patterns)
        services.AddScoped<IExcelExportService, ExcelExportService>();

        // Configure ExcelExportOptions from configuration if provided
        if (configuration != null)
        {
            services.Configure<ExcelExportOptions>(
                configuration.GetSection("ExcelExport"));
        }

        return services;
    }
}
