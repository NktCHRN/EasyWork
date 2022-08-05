using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WebAPI.Hubs
{
    [Authorize(Roles = "Admin")]
    public class UserBansHub: Hub
    {
        public async Task GetConnectionId()
        {
            await Clients.Caller.SendAsync("ConnectionId", Context.ConnectionId);
        }

        public async Task StartListening(int userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
        }

        public async Task StopListening(int userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());
        }
    }
}
