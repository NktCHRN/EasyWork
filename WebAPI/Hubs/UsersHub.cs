using Data.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using WebAPI.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace WebAPI.Hubs
{
    public class UsersHub : Hub
    {
        private readonly UserManager<User> _userManager;

        private readonly IUserConnectionsContainer _connections;

        public UsersHub(UserManager<User> userManager, IUserConnectionsContainer connections)
        {
            _userManager = userManager;
            _connections = connections;
        }

        public override async Task OnConnectedAsync()
        {
            await ChangeStatus(true);
            await base.OnConnectedAsync();
        }

        public async Task StartListening(int userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
            await SendStatusChange(Clients.Client(Context.ConnectionId), userId);
        }

        public async Task StopListening(int userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());
        }

        private bool IsOnline(int userId)
        {
            try
            {
                return _connections.UserConnections[userId].Any();
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        private async Task SendStatusChange(IClientProxy recipient, int userId)
        {
            await recipient.SendAsync("StatusChange", IsOnline(userId));
        }

        private async Task ChangeStatus(bool value)
        {
            if (Context.User != null)
            {
                var model = await _userManager.GetUserAsync(Context.User);
                if (model != null)
                {
                    object locker = new();
                    lock (locker)
                    {
                        if (value)
                        {
                            try
                            {
                                _connections.UserConnections[model.Id].Add(Context.ConnectionId);
                            }
                            catch (KeyNotFoundException)
                            {
                                _connections.UserConnections[model.Id] = new List<string>() { Context.ConnectionId };
                            }
                        }
                        else
                        {
                            try
                            {
                                _connections.UserConnections[model.Id].Remove(Context.ConnectionId);
                            }
                            catch (KeyNotFoundException) { }
                        }
                    }
                    await SendStatusChange(Clients.Group(model.Id.ToString()), model.Id);
                    model.LastSeen = DateTimeOffset.Now;
                    await _userManager.UpdateAsync(model);
                }
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await ChangeStatus(false);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
