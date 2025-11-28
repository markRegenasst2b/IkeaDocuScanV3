using IkeaDocuScan_Web;
using IkeaDocuScan_Web.Components;
using IkeaDocuScan_Web.Middleware;
using IkeaDocuScan_Web.Services;
using IkeaDocuScan_Web.Client.Services;
using IkeaDocuScan_Web.Endpoints;
using IkeaDocuScan_Web.Hubs;
using IkeaDocuScan_Web.Authorization;
using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.Configuration;
using ExcelReporting.Extensions;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Configure configuration sources with layered approach
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEncryptedJsonFile("secrets.encrypted.json", optional: true, reloadOnChange: false, "ConnectionStrings:DefaultConnection")
    .AddEnvironmentVariables();

// Configure Serilog for structured logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting IkeaDocuScan application");

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Add Authentication Services
// Note: Negotiate (Windows Auth) only works on Windows with IIS
// For development on Linux/WSL, we need an alternative approach
if (OperatingSystem.IsWindows())
{
    builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
}
else
{
    // For Linux/WSL development, use a test authentication handler
    builder.Services.AddAuthentication("TestScheme")
        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthenticationHandler>("TestScheme", options => { });
}

// Authorization with custom policies and dynamic policy provider
builder.Services.AddAuthorization(options =>
{
    // Policy requiring user to have access to the system
    options.AddPolicy("HasAccess", policy =>
        policy.Requirements.Add(new UserAccessRequirement()));

    // Policy requiring user to be a super user
    options.AddPolicy("SuperUser", policy =>
        policy.Requirements.Add(new SuperUserRequirement()));

    // Default policy requires authentication
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Register dynamic authorization policy provider for database-driven endpoint authorization
builder.Services.AddSingleton<IAuthorizationPolicyProvider, DynamicAuthorizationPolicyProvider>();

// Register authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, UserAccessHandler>();
builder.Services.AddScoped<IAuthorizationHandler, SuperUserHandler>();
builder.Services.AddScoped<IAuthorizationHandler, EndpointAuthorizationHandler>();

// Register endpoint authorization service for dynamic authorization
builder.Services.AddScoped<IEndpointAuthorizationService, EndpointAuthorizationService>();
builder.Services.AddScoped<IEndpointAuthorizationManagementService, EndpointAuthorizationManagementService>();

// Register server-side adapter for EndpointAuthorizationHttpService (used by Blazor components in InteractiveAuto mode)
builder.Services.AddHttpClient<EndpointAuthorizationHttpService>();
builder.Services.AddScoped<EndpointAuthorizationHttpService, EndpointAuthorizationServerAdapter>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserIdentityService>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingServerAuthenticationStateProvider>();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

// Add DbContextFactory for services that need concurrent database access
// This uses a separate registration that doesn't conflict with AddDbContext
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ), ServiceLifetime.Scoped);

// Configuration options
builder.Services.Configure<IkeaDocuScanOptions>(
    builder.Configuration.GetSection(IkeaDocuScanOptions.SectionName));

builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection(EmailOptions.SectionName));

builder.Services.Configure<DocumentSearchOptions>(
    builder.Configuration.GetSection(DocumentSearchOptions.SectionName));

builder.Services.Configure<EmailSearchResultsOptions>(
    builder.Configuration.GetSection(EmailSearchResultsOptions.SectionName));

// Validate configuration on startup
var options = builder.Configuration.GetSection(IkeaDocuScanOptions.SectionName).Get<IkeaDocuScanOptions>();
options?.Validate();

var emailOptions = builder.Configuration.GetSection(EmailOptions.SectionName).Get<EmailOptions>();
emailOptions?.Validate();

var searchOptions = builder.Configuration.GetSection(DocumentSearchOptions.SectionName).Get<DocumentSearchOptions>();
searchOptions?.Validate();

var emailSearchOptions = builder.Configuration.GetSection(EmailSearchResultsOptions.SectionName).Get<EmailSearchResultsOptions>();
emailSearchOptions?.Validate();

// Memory cache for file list caching
builder.Services.AddMemoryCache();

// Session support (required for test identity persistence in DEBUG mode)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(7); // Test identity persists for 7 days
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Data access services
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IAuditTrailService, AuditTrailService>();
builder.Services.AddScoped<IScannedFileService, ScannedFileService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICounterPartyService, CounterPartyService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IDocumentTypeService, DocumentTypeService>();
builder.Services.AddScoped<IUserPermissionService, UserPermissionService>();
builder.Services.AddScoped<IDocumentNameService, DocumentNameService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IActionReminderService, ActionReminderService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ILogViewerService, LogViewerService>();
builder.Services.AddScoped<IAccessAuditService, AccessAuditService>();

#if DEBUG
// Test Identity Service (DEVELOPMENT ONLY)
builder.Services.AddScoped<TestIdentityService>();
#endif

// Configuration management services
builder.Services.AddScoped<ISystemConfigurationManager, IkeaDocuScan.Infrastructure.Services.ConfigurationManagerService>();
builder.Services.AddScoped<IEmailTemplateService, IkeaDocuScan.Infrastructure.Services.EmailTemplateService>();
builder.Services.AddScoped<ConfigurationMigrationService>();

// Excel Reporting Services
builder.Services.AddExcelReporting(builder.Configuration);

// Excel Preview Data Service (for passing data to preview page - shared between server and client)
builder.Services.AddScoped<IkeaDocuScan_Web.Client.Services.ExcelPreviewDataService>();

// Property Metadata Extractor (for extracting ExcelExportAttribute metadata)
builder.Services.AddScoped<ExcelReporting.Services.PropertyMetadataExtractor>();

// SignalR for real-time updates
builder.Services.AddSignalR();

// Exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseHsts();
}

// Use exception handler for all environments
app.UseExceptionHandler();

app.UseHttpsRedirection();

// Session middleware (required for test identity)
app.UseSession();

app.UseAuthentication();

#if DEBUG
// Test Identity Middleware - MUST run before WindowsIdentityMiddleware
app.UseTestIdentity();
#endif

app.UseMiddleware<WindowsIdentityMiddleware>();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(IkeaDocuScan_Web.Client._Imports).Assembly)
   ;

// Map SignalR hub
app.MapHub<DataUpdateHub>("/hubs/data-updates");

// Map API endpoints
app.MapDocumentEndpoints();
app.MapReportEndpoints();
app.MapActionReminderEndpoints();
app.MapCounterPartyEndpoints();
app.MapCountryEndpoints();
app.MapDocumentTypeEndpoints();
app.MapUserPermissionEndpoints();
app.MapUserIdentityEndpoints();
app.MapScannedFileEndpoints();
app.MapAuditTrailEndpoints();
app.MapDocumentNameEndpoints();
app.MapCurrencyEndpoints();
app.MapEmailEndpoints();
app.MapExcelExportEndpoints();
app.MapConfigurationEndpoints();
app.MapLogViewerEndpoints();
app.MapEndpointAuthorizationEndpoints();
app.MapAccessAuditEndpoints();

#if DEBUG
// Test Identity Endpoints (DEVELOPMENT ONLY)
app.MapTestIdentityEndpoints();

// Diagnostic Endpoints (DEVELOPMENT ONLY)
app.MapDiagnosticEndpoints();
#endif

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.Information("Application shutting down");
    Log.CloseAndFlush();
}
