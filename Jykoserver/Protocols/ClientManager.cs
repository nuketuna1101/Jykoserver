using System.Collections.Concurrent;

namespace Jykoserver.Protocols
{
    public class ClientManager
    {
        // Singleton으로 존재
        private static readonly Lazy<ClientManager> _instance = new Lazy<ClientManager>(() => new ClientManager());
        private ClientManager() { }
        public static ClientManager Instance => _instance.Value;
        // 클라이언트 목록
        private ConcurrentDictionary<Guid, ClientInfo> clients = new ConcurrentDictionary<Guid, ClientInfo>();

        public void AddClient(Guid guid, ClientInfo clientInfo)
        {
            clients.TryAdd(guid, clientInfo);
        }
        public void RemoveClient(Guid guid)
        {
            clients.TryRemove(guid, out var clientInfo);
        }
        public void Broadcast(byte[] msg)
        {
            foreach (var client in clients.Values)
            {
                //client.send
            }
        }
    }

    public class ClientInfo
    {

    }
}
