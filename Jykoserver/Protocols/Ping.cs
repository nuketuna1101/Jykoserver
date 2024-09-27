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
            var req = await MemoryPackSerializer.DeserializeAsync<string>(httpContext.Request.Body);

            /*
             
            ***
            *
            * MemoryPackSerializer.DeserializeAsync<string>(httpContext.Request.Body) 를 바로 호출하는 방식은
            * stream 객체이기 때문에 한 번만 읽을 수 있기 때문에 바로 처리할려고 할때 예상치 못한 데이터 부족 문제 발생
            * 버퍼링 또는 메모리로 데이터를 읽어오는 전처리 과정 필요
             
             
             */

            /*
            // Enable buffering to allow multiple reads
            httpContext.Request.EnableBuffering();

            string? req;
            try
            {
                // Read the entire request body
                using var reader = new StreamReader(httpContext.Request.Body, leaveOpen: true);
                req = await reader.ReadToEndAsync();
                httpContext.Request.Body.Position = 0;  // Reset the position of the stream
            }
            catch (Exception ex)
            {
                Log.Logger.ForContext("Type", "SYS").Error("Failed to read request body: {0}", ex.Message);
                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            */
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
        }

    }
}
