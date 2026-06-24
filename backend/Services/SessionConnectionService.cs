namespace backend.Services;

public class SessionConnectionService
{
    private readonly object _lock = new();
    private readonly Dictionary<string, string> _connectionUsers = new();

    public void Connect(string connectionId, string name)
    {
        lock (_lock)
        {
            _connectionUsers[connectionId] = name.Trim();
        }
    }

    public void Disconnect(string connectionId)
    {
        lock (_lock)
        {
            _connectionUsers.Remove(connectionId);
        }
    }

    public IReadOnlyList<(string ConnectionId, string Name)> GetConnections()
    {
        lock (_lock)
        {
            return _connectionUsers
                .Select(pair => (pair.Key, pair.Value))
                .ToList();
        }
    }
}
