using MemoryPack;
using Serilog;
using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace Jykoserver.Protocols
{
    public class Chat : IProtocol
    {
        public RequestType reqType { get; }

        private static List<WebSocket> connectedClients = new List<WebSocket>();


        public Chat()
        {
            reqType = RequestType.CHAT;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            // websocket handler
            Log.Logger.ForContext("Type", "SYS").Information("+--+ WebSocket CHAT invoked");

            if (httpContext.WebSockets.IsWebSocketRequest)
            {
                Request? req = await MemoryPackSerializer.DeserializeAsync<Request>(httpContext.Request.Body);
                var myGUID = req.UserGUID;
                var myMsg = MemoryPackSerializer.Deserialize<string>(req.Msg);
                Log.Logger.ForContext("Type", "SYS").Information("[Request] from GUID: {0}", myGUID);
                Log.Logger.ForContext("Type", "SYS").Information("[Request] Message: {0}", myMsg);
                var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
                await Broadcast(myMsg);
            }
            else
            {
                Log.Logger.ForContext("Type", "SYS").Error("[Request Error] NOT websocket request");
                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;   // 400 error
            }
            return;
        }

        private async Task Broadcast(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);

            foreach (var client in connectedClients)
            {
                if (client.State == WebSocketState.Open)
                    await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }

        }


    }
}
