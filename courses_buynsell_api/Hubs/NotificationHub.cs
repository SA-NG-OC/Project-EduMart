using Microsoft.AspNetCore.SignalR;

namespace courses_buynsell_api.Hubs;

public class NotificationHub : Hub
{
    // Phương thức để client join vào group của seller
    public async Task JoinSellerGroup(string sellerId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"seller_{sellerId}");
    }

    // Phương thức để client leave group
    public async Task LeaveSellerGroup(string sellerId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"seller_{sellerId}");
    }

    // Có thể override các method này nếu cần
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
    }
}