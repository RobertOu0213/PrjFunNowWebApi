using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    private static List<string> ConnectionIds = new List<string>();

    public override Task OnConnectedAsync()
    {
        ConnectionIds.Add(Context.ConnectionId);
        Clients.All.SendAsync("UpdList", System.Text.Json.JsonSerializer.Serialize(ConnectionIds));
        Clients.Caller.SendAsync("UpdSelfID", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        ConnectionIds.Remove(Context.ConnectionId);
        Clients.All.SendAsync("UpdList", System.Text.Json.JsonSerializer.Serialize(ConnectionIds));
        return base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string senderId, string message, string receiverId)
    {
        if (string.IsNullOrEmpty(receiverId) || !ConnectionIds.Contains(receiverId))
        {
            // 如果 receiverId 為空或不在連線列表中,則廣播給所有用戶端
            await Clients.All.SendAsync("UpdContent", $"{senderId}: {message}");
        }
        else
        {
            // 如果 receiverId 有效,則傳送給特定用戶端
            await Clients.Client(receiverId).SendAsync("UpdContent", $"{senderId}: {message}");
        }
    }
}