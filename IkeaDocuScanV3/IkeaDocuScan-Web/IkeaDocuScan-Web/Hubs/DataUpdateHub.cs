using Microsoft.AspNetCore.SignalR;

namespace IkeaDocuScan_Web.Hubs;

/// <summary>
/// SignalR hub for broadcasting real-time data updates to all connected clients
/// Clients should listen for these events:
/// - DocumentCreated
/// - DocumentUpdated
/// - DocumentDeleted
/// </summary>
public class DataUpdateHub : Hub
{
    private readonly ILogger<DataUpdateHub> _logger;

    public DataUpdateHub(ILogger<DataUpdateHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
