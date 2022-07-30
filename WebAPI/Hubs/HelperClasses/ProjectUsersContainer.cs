using WebAPI.Interfaces;

namespace WebAPI.Hubs.HelperClasses
{
    public class ProjectUsersContainer : IProjectUsersContainer
    {
        private readonly IDictionary<int, IDictionary<int, UserConnectionsWithTime>> _connections
            = new Dictionary<int, IDictionary<int, UserConnectionsWithTime>>();

        private readonly object _lockObject = new();

        public bool Add(int projectId, int userId, string connectionId)
        {
            var newUserAdded = false;
            lock (_lockObject)
            {
                if (!_connections.ContainsKey(projectId))
                    _connections.Add(projectId, new Dictionary<int, UserConnectionsWithTime>());
                var projectItem = _connections[projectId];
                if (!projectItem.ContainsKey(userId))
                    projectItem.Add(userId, new UserConnectionsWithTime());
                UserConnectionsWithTime userItem = projectItem[userId];
                if (!userItem.Connections.Any())
                    newUserAdded = true;
                if (!userItem.Connections.Contains(connectionId))
                    userItem.Connections.Add(connectionId);
            }
            return newUserAdded;
        }

        public IEnumerable<int> Delete(int userId, string connectionId)
        {
            var result = new List<int>();
            lock (_lockObject)
            {
                var projectsToBeRemoved = new List<int>();
                foreach (var item in _connections)
                {
                    if (item.Value.ContainsKey(userId))
                    {
                        var wasRemoved = item.Value[userId].Connections.Remove(connectionId);
                        if (!item.Value[userId].Connections.Any())
                        {
                            if (wasRemoved)
                                result.Add(item.Key);
                            item.Value.Remove(userId);
                        }
                    }
                    if (!item.Value.Any())
                        projectsToBeRemoved.Add(item.Key);
                }
                foreach (var item in projectsToBeRemoved)
                    _connections.Remove(item);
            }
            return result;
        }

        public IEnumerable<int> GetUsersOnProject(int projectId)
        {
            IEnumerable<int> result = new List<int>();
            lock (_lockObject)
            {
                if (_connections.ContainsKey(projectId))
                {
                    var users = _connections[projectId];
                    result = users
                        .Where(u => u.Value.Connections.Any())
                        .OrderBy(u => u.Value.Time)
                        .Select(u => u.Key);
                }
            }
            return result;
        }
    }
}
