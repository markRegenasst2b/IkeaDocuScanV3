using IkeaDocuScan_Web;
using IkeaDocuScan_Web.Components;
using IkeaDocuScan_Web.Middleware;
using IkeaDocuScan_Web.Services;
using IkeaDocuScan_Web.Endpoints;
using IkeaDocuScan_Web.Hubs;
using IkeaDocuScan_Web.Authorization;
using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.Configuration;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;

var builder = WebApplication.CreateBuilder(args);

// Configure configuration sources with layered approach
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEncryptedJsonFile("secrets.encrypted.json", optional: true, reloadOnChange: false, "ConnectionStrings:DefaultConnection")
    .AddEnvironmentVariables();

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Add Blazorise
builder.Services
    .AddBlazorise()
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();

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

// Authorization with custom policies
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

// Register authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, UserAccessHandler>();
builder.Services.AddScoped<IAuthorizationHandler, SuperUserHandler>();

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

// Validate configuration on startup
var options = builder.Configuration.GetSection(IkeaDocuScanOptions.SectionName).Get<IkeaDocuScanOptions>();
options?.Validate();

var emailOptions = builder.Configuration.GetSection(EmailOptions.SectionName).Get<EmailOptions>();
emailOptions?.Validate();

// Memory cache for file list caching
builder.Services.AddMemoryCache();

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

app.UseAuthentication();
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
app.MapCounterPartyEndpoints();
app.MapCountryEndpoints();
app.MapDocumentTypeEndpoints();
app.MapUserPermissionEndpoints();

app.Run();
