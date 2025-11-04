using IkeaDocuScan.ActionReminderService;
using IkeaDocuScan.ActionReminderService.Services;
using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Shared.Configuration;
using Microsoft.EntityFrameworkCore;

// Create the host builder
var builder = Host.CreateApplicationBuilder(args);

// Configure to run as Windows Service
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "IkeaDocuScan Action Reminder Service";
});

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddEventLog(settings =>
{
    settings.SourceName = "IkeaDocuScan Action Reminder";
});

// Load configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Configure options
builder.Services.Configure<ActionReminderServiceOptions>(
    builder.Configuration.GetSection("ActionReminderService"));
builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection("Email"));

// Register database context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Register services
builder.Services.AddScoped<IActionReminderEmailService, ActionReminderEmailService>();
builder.Services.AddScoped<IEmailSender, EmailSenderService>();

// Register the background worker
builder.Services.AddHostedService<ActionReminderWorker>();

// Build and run the host
var host = builder.Build();

// Log startup
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("IkeaDocuScan Action Reminder Service starting...");

try
{
    await host.RunAsync();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Service terminated unexpectedly");
    throw;
}
finally
{
    logger.LogInformation("IkeaDocuScan Action Reminder Service stopped");
}
