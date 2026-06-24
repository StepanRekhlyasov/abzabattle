namespace backend.Services;

public class PresenceService
{
    private readonly object _lock = new();
    private readonly Dictionary<string, HashSet<string>> _userConnections = new();
    private readonly Dictionary<string, string> _connectionUsers = new();

    public bool Connect(string connectionId, string name)
    {
        lock (_lock)
        {
            if (!_userConnections.TryGetValue(name, out var connections))
            {
                connections = [];
                _userConnections[name] = connections;
            }

            var becameOnline = connections.Count == 0;
            connections.Add(connectionId);
            _connectionUsers[connectionId] = name;
            return becameOnline;
        }
    }

    public (string? Name, bool WentOffline) Disconnect(string connectionId)
    {
        lock (_lock)
        {
            if (!_connectionUsers.Remove(connectionId, out var name))
            {
                return (null, false);
            }

            if (!_userConnections.TryGetValue(name, out var connections))
            {
                return (name, true);
            }

            connections.Remove(connectionId);
            if (connections.Count > 0)
            {
                return (name, false);
            }

            _userConnections.Remove(name);
            return (name, true);
        }
    }

    public IReadOnlyList<string> GetOnlineUsers()
    {
        lock (_lock)
        {
            return _userConnections.Keys.OrderBy(name => name).ToList();
        }
    }
}
