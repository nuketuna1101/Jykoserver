using MemoryPack;
using Serilog;
using System.Net;

namespace Jykoserver.Protocols
{
    public class Send : IProtocol
    {
        public RequestType reqType { get; }
        public Send()
        {
            reqType = RequestType.SEND;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            Log.Logger.ForContext("Type", "SYS").Information("+--+ Send iprotocol invoked");
            // request 패킷엔 : guid와 dto 담겨서 옴.
            Request? req = await MemoryPackSerializer.DeserializeAsync<Request>(httpContext.Request.Body);
            var myGUID = req.UserGUID;
            var myMsg = MemoryPackSerializer.Deserialize<string>(req.Msg);
            Log.Logger.ForContext("Type", "SYS").Information("[Request] from GUID: {0}", myGUID);
            Log.Logger.ForContext("Type", "SYS").Information("[Request] Message: {0}", myMsg);
            // response 생성
            var response = MemoryPackSerializer.Serialize(new Response
            {
                TypeNo = -1,
                Msg = req.Msg,
            });
            // TODO :: 모든 등록된 클라이언트에게 브로드캐스트

            return;
        }
    }
}
