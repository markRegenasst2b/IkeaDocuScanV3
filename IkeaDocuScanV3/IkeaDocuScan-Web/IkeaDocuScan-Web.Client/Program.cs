using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan_Web.Client;
using IkeaDocuScan_Web.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore(options =>
{
    // Register the same authorization policies as the server
    // Note: Client-side policies are for UI only, server enforces actual security
    options.AddPolicy("HasAccess", policy =>
        policy.RequireAuthenticatedUser());

    options.AddPolicy("SuperUser", policy =>
        policy.RequireAuthenticatedUser());
});
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

// HTTP client with base address
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// Data services
builder.Services.AddScoped<IDocumentService, DocumentHttpService>();
builder.Services.AddScoped<ICounterPartyService, CounterPartyHttpService>();
builder.Services.AddScoped<ICountryService, CountryHttpService>();
builder.Services.AddScoped<IDocumentTypeService, DocumentTypeHttpService>();
builder.Services.AddScoped<IUserPermissionService, UserPermissionHttpService>();
builder.Services.AddScoped<IScannedFileService, ScannedFileHttpService>();
builder.Services.AddScoped<IAuditTrailService, AuditTrailHttpService>();
builder.Services.AddScoped<IDocumentNameService, DocumentNameHttpService>();
builder.Services.AddScoped<ICurrencyService, CurrencyHttpService>();
builder.Services.AddScoped<IActionReminderService, ActionReminderHttpService>();
builder.Services.AddScoped<IReportService, ReportHttpService>();
builder.Services.AddScoped<EmailHttpService>();

// Configuration Management Service
builder.Services.AddScoped<ConfigurationHttpService>();

// Excel Export Service
builder.Services.AddScoped<ExcelExportHttpService>();

// Excel Preview Data Service (for passing data to preview page)
builder.Services.AddScoped<ExcelPreviewDataService>();

// Property Metadata Extractor (for extracting ExcelExportAttribute metadata)
builder.Services.AddScoped<ExcelReporting.Services.PropertyMetadataExtractor>();

// Register DocumentHttpService (using HttpClient factory pattern)
builder.Services.AddScoped<DocumentHttpService>();

await builder.Build().RunAsync();





