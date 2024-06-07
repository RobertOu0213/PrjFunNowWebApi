using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PrjFunNowWebApi.Hubs
{
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
            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, message);
        }
    }
}
