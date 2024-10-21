using MemoryPack;
using Serilog;
using System.Net;

namespace Jykoserver.Protocols
{
    public class Ping : IProtocol
    {
        public RequestType reqType { get; }

        public Ping()
        {
            reqType = RequestType.PING;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            //convert current time into byte array and write into response
            Log.Logger.ForContext("Type", "SYS").Information("+--+ PING invoked");
            Request? req = await MemoryPackSerializer.DeserializeAsync<Request>(httpContext.Request.Body);
            var myGUID = req.UserGUID;
            var myMsg = MemoryPackSerializer.Deserialize<string>(req.Msg);
            Log.Logger.ForContext("Type", "SYS").Information("[Request] from GUID: {0}", myGUID);
            Log.Logger.ForContext("Type", "SYS").Information("[Request] Message: {0}", myMsg);
            // bad request flow
            if (req == null || !myMsg.Equals("Ping"))
            {
                Log.Logger.ForContext("Type", "SYS").Error("[Request Error] request is either null or NOT Ping");
                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;   // 400 error
                return;
            }

            if (myGUID != null)
            {
                var cm = ClientManager.Instance;
                cm.AddClient(new ClientInfo
                {
                    guid = myGUID,
                    PingTime = DateTime.UtcNow,
                });
                cm.LogAllClients();
            }

            //normal flow
            Log.Logger.ForContext("Type", "SYS").Information("ping dateTime : {0}", DateTime.UtcNow.Ticks);
            var response = MemoryPackSerializer.Serialize(DateTime.UtcNow.Ticks);
            await httpContext.Response.Body.WriteAsync(response.AsMemory(0, response.Length));
            return;
        }
    }
}
