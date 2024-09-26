using MemoryPack;

namespace Jykoserver
{
    [MemoryPackable]
    public partial class Response
    {
        public short TypeNo;
        public byte[] Msg;
        public Response()
        {
            TypeNo = -1;
            Msg = Array.Empty<byte>();
        }
        [MemoryPackConstructor]
        public Response(short TypeNo)
        {
            this.TypeNo = TypeNo;
            Msg = Array.Empty<byte>();
        }
    }
}
