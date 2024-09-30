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
            var req = await MemoryPackSerializer.DeserializeAsync<Request>(httpContext.Request.Body);
            var myGUID = req.UserGUID;
            var myMsg = MemoryPackSerializer.Deserialize<string>(req.Msg);
            Log.Logger.ForContext("Type", "SYS").Information("::::::::::::::::::Received request body: {0}", req);
            // bad request flow
            if (req == null || !myMsg.Equals("Ping"))
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;   // 400 error
                return;
            }
            //normal flow
            Log.Logger.ForContext("Type", "SYS").Information("ping dateTime : {0}", DateTime.UtcNow.Ticks);
            var response = MemoryPackSerializer.Serialize(DateTime.UtcNow.Ticks);
            await httpContext.Response.Body.WriteAsync(response.AsMemory(0, response.Length));
            return;



            /*
             레거시코드
            //convert current time into byte array and write into response
            Log.Logger.ForContext("Type", "SYS").Information("+--+ PING invoked");
            var req = await MemoryPackSerializer.DeserializeAsync<string>(httpContext.Request.Body);
            Log.Logger.ForContext("Type", "SYS").Information("::::::::::::::::::Received request body: {0}", req);
            // bad request flow
            if (req == null || !req.Equals("Ping"))
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;   // 400 error
                return;
            }
            //normal flow
            Log.Logger.ForContext("Type", "SYS").Information("ping dateTime : {0}", DateTime.UtcNow.Ticks);
            var response = MemoryPackSerializer.Serialize(DateTime.UtcNow.Ticks);
            await httpContext.Response.Body.WriteAsync(response.AsMemory(0, response.Length));
            return;
             
             
             */
        }

    }
}
