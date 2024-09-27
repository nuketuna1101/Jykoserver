using MemoryPack;

namespace Jykoserver
{
    public enum RequestType
    {
        PING = 20,
        SEND = 30,   
    }

    [MemoryPackable]
    public partial class Request
    {
        public Guid UserGUID { get; set; }
        public byte[] Msg { get; set; }
        [MemoryPackConstructor]
        public Request()
        {
            UserGUID = Guid.Empty;
            Msg = Array.Empty<byte>();
        }
    }
}
