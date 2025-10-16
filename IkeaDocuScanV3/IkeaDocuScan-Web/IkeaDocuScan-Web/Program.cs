using IkeaDocuScan_Web;
using IkeaDocuScan_Web.Components;
using IkeaDocuScan_Web.Middleware;
using IkeaDocuScan_Web.Services;
using IkeaDocuScan_Web.Endpoints;
using IkeaDocuScan_Web.Hubs;
using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddAuthorization();
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

// Data access services
builder.Services.AddScoped<IDocumentService, DocumentService>();

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

app.Run();
