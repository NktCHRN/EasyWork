using WebAPI.Interfaces;

namespace WebAPI.Hubs.HelperClasses
{
    public class UserConnectionsContainer : IUserConnectionsContainer
    {
        public IDictionary<int, ICollection<string>> UserConnections => _userConnections;

        private readonly IDictionary<int, ICollection<string>> _userConnections = new Dictionary<int, ICollection<string>>();
    }
}
