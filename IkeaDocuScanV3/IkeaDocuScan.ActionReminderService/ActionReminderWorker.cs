using IkeaDocuScan.ActionReminderService.Services;
using Microsoft.Extensions.Options;

namespace IkeaDocuScan.ActionReminderService;

/// <summary>
/// Background worker that runs daily to send action reminder emails
/// </summary>
public class ActionReminderWorker : BackgroundService
{
    private readonly ILogger<ActionReminderWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ActionReminderServiceOptions _options;
    private DateTime _lastRunDate;

    public ActionReminderWorker(
        ILogger<ActionReminderWorker> logger,
        IServiceProvider serviceProvider,
        IOptions<ActionReminderServiceOptions> options)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _lastRunDate = DateTime.MinValue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Action Reminder Worker started at: {Time}", DateTimeOffset.Now);

        if (!_options.Enabled)
        {
            _logger.LogWarning("Action Reminder Service is DISABLED in configuration");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;

                // Check if it's time to run
                if (ShouldRunNow(now))
                {
                    _logger.LogInformation("Starting action reminder check at {Time}", now);

                    await ProcessActionRemindersAsync(stoppingToken);

                    _lastRunDate = now.Date;
                    _logger.LogInformation("Action reminder check completed at {Time}", DateTime.Now);
                }
                else
                {
                    _logger.LogDebug("Not time to run yet. Next check: {ScheduleTime}", _options.ScheduleTime);
                }

                // Wait for the configured interval before checking again
                await Task.Delay(TimeSpan.FromMinutes(_options.CheckIntervalMinutes), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Service is stopping
                _logger.LogInformation("Action Reminder Worker is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Action Reminder Worker main loop");
                // Wait before retrying
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("Action Reminder Worker stopped at: {Time}", DateTimeOffset.Now);
    }

    /// <summary>
    /// Determine if the service should run now based on schedule and last run
    /// </summary>
    private bool ShouldRunNow(DateTime now)
    {
        // Don't run if already ran today
        if (_lastRunDate.Date == now.Date)
        {
            return false;
        }

        // Parse the scheduled time
        if (!TimeSpan.TryParse(_options.ScheduleTime, out var scheduledTime))
        {
            _logger.LogError("Invalid ScheduleTime format: {ScheduleTime}. Expected HH:mm", _options.ScheduleTime);
            return false;
        }

        var scheduledDateTime = now.Date.Add(scheduledTime);

        // Run if current time is past scheduled time and we haven't run today
        return now >= scheduledDateTime;
    }

    /// <summary>
    /// Process action reminders by fetching due actions and sending emails
    /// </summary>
    private async Task ProcessActionRemindersAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IActionReminderEmailService>();

            // Send action reminders
            await emailService.SendDailyActionRemindersAsync(_options.DaysAhead, stoppingToken);

            _logger.LogInformation("Successfully processed action reminders");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing action reminders");
            throw; // Re-throw to be caught by outer exception handler
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Action Reminder Worker is stopping...");
        await base.StopAsync(stoppingToken);
    }
}
