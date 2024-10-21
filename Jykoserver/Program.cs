using Serilog;
using Microsoft.AspNetCore.Builder;

using System.Net;
using Jykoserver.Protocols;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Text;

namespace Jykoserver;


public class Program
{
    public static void Main(string[] args)
    {
        // builder creation
        var builder = WebApplication.CreateBuilder(args);
        // logging
        ConfigureLogging(builder.Logging);
        // dispatcher 
        Dictionary<RequestType, Func<IProtocol>> dispatcher = RouteDispatcher.MapDispatcher();
        // app build
        var app = builder.Build();   
        // lifetime logging
        ConfigureLifetimeLogging(app.Lifetime);
        // websocket for chatting
        app.UseWebSockets();
        // websockethandling
        app.Use(async(context, next) =>
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await HandleWebSocketAsync(context, webSocket); // WebSocket 처리
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest; // 400 error
                    Log.Logger.ForContext("Type", "SYS").Error("[ERROR] WebSocket BadRequest");
                }
            }
            else
            {
                await next();
            }
        });


        // app running: Routing for the endpoints
        app.Run(async context =>
        {
            var path = context.Request.Path.ToString().ToLower()[1..];
            var protocolKey = RouteDispatcher.ConvertToReqType(path);
            Log.Logger.ForContext("Type", "SYS").Information("[appRun] path " + path);
            Log.Logger.ForContext("Type", "SYS").Information("[appRun] protocolKey " + protocolKey);


            // root endpoint         
            if (path == "" || path == "favicon.ico")
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK; // 200 OK
                await context.Response.WriteAsync("Welcome to Jykoserver!");
                return;
            }

            // dispatcher에서 프로토콜 찾기
            if (dispatcher!.TryGetValue(protocolKey, out Func<IProtocol>? protocol))
            {
                await protocol().InvokeAsync(context);
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound; // 404 error
                await context.Response.WriteAsync("Endpoint not found.");
            }

        });
        
        app.Run();

    }

    private static void ConfigureLogging(ILoggingBuilder logging)
    {
        var logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File("Logs\\log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        logging.ClearProviders();
        logging.AddSerilog();
        Log.Logger = logger;
    }

    private static void ConfigureLifetimeLogging(IHostApplicationLifetime lifetime)
    {
        lifetime.ApplicationStarted.Register(() =>
            Log.Logger.ForContext("Type", "SYS").Information("[Jykoserver] INIT :: Start Execution.")
        );
        lifetime.ApplicationStopping.Register(() =>
            Log.Logger.ForContext("Type", "SYS").Information("[Jykoserver] END :: Closing...")
        );
    }

    private static async Task HandleWebSocketAsync(HttpContext httpContext, WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        Log.Logger.ForContext("Type", "SYS").Information("[Jykoserver] [WebSocket] HandleWebSocketAsync on running");

        while (!result.CloseStatus.HasValue)
        {
            // 메시지 내용 로깅
            string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Log.Logger.ForContext("Type", "SYS").Information("[Jykoserver] [WebSocket] Received Text Message: {Message}", receivedMessage);



            // 클라이언트가 보내온 메시지를 처리 (ex: Echo 기능)
            await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

    }
}