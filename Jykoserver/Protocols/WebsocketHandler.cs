using MemoryPack;
using Serilog;
using System.Net;
using System.Net.WebSockets;

namespace Jykoserver.Protocols
{
    public class WebsocketHandler : IProtocol
    {
        public RequestType reqType { get; }

        public WebsocketHandler()
        {
            reqType = RequestType.WSH;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            // websocket handler
            Log.Logger.ForContext("Type", "SYS").Information("+--+ WebsocketHandler invoked");
            Log.Logger.ForContext("Type", "SYS").Information("+--+ httpContext.WebSockets.IsWebSocketRequest : " + httpContext.WebSockets.IsWebSocketRequest);

            if (httpContext.WebSockets.IsWebSocketRequest)
            {
                Request? req = await MemoryPackSerializer.DeserializeAsync<Request>(httpContext.Request.Body);
                var myGUID = req.UserGUID;
                var myMsg = MemoryPackSerializer.Deserialize<string>(req.Msg);
                Log.Logger.ForContext("Type", "SYS").Information("[Request] from GUID: {0}", myGUID);
                Log.Logger.ForContext("Type", "SYS").Information("[Request] Message: {0}", myMsg);
                var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
                //await Broadcast(myMsg);
            }
            else
            {
                Log.Logger.ForContext("Type", "SYS").Error("[Request Error] NOT websocket request");
                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;   // 400 error
            }
            return;
        }
    }
}
