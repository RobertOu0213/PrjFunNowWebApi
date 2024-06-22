using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ChatHub : Hub
{
    private static Dictionary<string, string> UserConnections = new Dictionary<string, string>();
    private static Dictionary<string, bool> UserOnlineStatus = new Dictionary<string, bool>(); // 記錄用戶是否在線

    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"Connected: {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        var user = UserConnections.FirstOrDefault(x => x.Value == Context.ConnectionId);
        if (user.Key != null)
        {
            UserConnections.Remove(user.Key);
            UserOnlineStatus.Remove(user.Key);
            Clients.All.SendAsync("UpdList", System.Text.Json.JsonSerializer.Serialize(UserOnlineStatus));
        }
        Console.WriteLine($"Disconnected: {Context.ConnectionId}");
        return base.OnDisconnectedAsync(exception);
    }

    public async Task RegisterUser(string userId)
    {
        if (!UserConnections.ContainsKey(userId))
        {
            UserConnections.Add(userId, Context.ConnectionId);
            UserOnlineStatus[userId] = false; // 初始狀態為未上線
        }
        else
        {
            UserConnections[userId] = Context.ConnectionId;
        }
        await Clients.Caller.SendAsync("UpdSelfID", userId);
        await Clients.All.SendAsync("UpdList", System.Text.Json.JsonSerializer.Serialize(UserOnlineStatus));
    }

    public async Task ConfirmConnection(string userId)
    {
        if (UserOnlineStatus.ContainsKey(userId))
        {
            UserOnlineStatus[userId] = true; // 更新狀態為上線
            await Clients.Client(UserConnections[userId]).SendAsync("ConfirmedConnection");
            await Clients.All.SendAsync("UpdList", System.Text.Json.JsonSerializer.Serialize(UserOnlineStatus));
        }
    }

    public async Task SendMessage(string senderId, string message, string receiverId)
    {
        try
        {
            if (string.IsNullOrEmpty(receiverId) || !UserConnections.ContainsKey(receiverId))
            {
                await Clients.All.SendAsync("UpdContent", $"{senderId}: {message}");
            }
            else
            {
                await Clients.Client(UserConnections[receiverId]).SendAsync("UpdContent", $"{senderId}: {message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SendMessage: {ex.Message}");
            throw;
        }
    }

    public async Task GetMessages(string userId)
    {
        // 示例實現：發送消息記錄
        await Clients.Caller.SendAsync("UpdContent", "Here are your messages...");
    }
}
