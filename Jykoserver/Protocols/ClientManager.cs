using Serilog;
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

        public void AddClient(ClientInfo clientInfo)
        {
            clients.TryAdd(clientInfo.guid, clientInfo);
        }
        public void RemoveClient(Guid guid)
        {
            clients.TryRemove(guid, out var clientInfo);
        }
        public IEnumerable<Guid> GetAllClientGuids()
        {
            return clients.Keys;
        }
        public ClientInfo GetClientInfoByGuid(Guid guid)
        {
            clients.TryGetValue(guid, out ClientInfo? clientInfo);
            return clientInfo;
        }
        public void LogAllClients()
        {
            Log.Logger.ForContext("Type", "SYS").Information("[ClientManager] + show lists +");
            foreach (var client in clients.Values)
            {
                Log.Logger.ForContext("Type", "SYS").Information("[ClientManager] : GUID : {Guid}, PingTime : {PingTime}", client.guid, client.PingTime);
            }
            Log.Logger.ForContext("Type", "SYS").Information("[ClientManager] + end list +");
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
        public Guid guid { get; set; }
        public DateTime PingTime { get; set; }
    }
}
