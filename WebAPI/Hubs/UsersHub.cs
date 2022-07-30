using Data.Entities;
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

        private static readonly object _locker = new();

        public UsersHub(UserManager<User> userManager, IUserConnectionsContainer connections)
        {
            _userManager = userManager;
            _connections = connections;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        [Authorize]
        public async Task Login()
        {
            if (Context.User == null)
                throw new HubException("You are not authorized");
            var model = await _userManager.GetUserAsync(Context.User);
            if (model == null)
                throw new HubException("You are not authorized");
            lock (_locker)
            {
                try
                {
                    if (_connections.UserConnections[model.Id].FirstOrDefault(c => c == Context.ConnectionId) == default)
                        _connections.UserConnections[model.Id].Add(Context.ConnectionId);
                }
                catch (KeyNotFoundException)
                {
                    _connections.UserConnections[model.Id] = new List<string>() { Context.ConnectionId };
                }
            }
            await SendStatusChange(Clients.Group(model.Id.ToString()), model.Id);
            model.LastSeen = DateTimeOffset.Now;
            await _userManager.UpdateAsync(model);
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

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.User != null)
            {
                var model = await _userManager.GetUserAsync(Context.User);
                if (model != null)
                {
                    lock (_locker)
                    {
                        try
                        {
                            _connections.UserConnections[model.Id].Remove(Context.ConnectionId);
                        }
                        catch (KeyNotFoundException) { }
                    }
                    await SendStatusChange(Clients.Group(model.Id.ToString()), model.Id);
                    model.LastSeen = DateTimeOffset.Now;
                    await _userManager.UpdateAsync(model);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
