using Serilog;
using Microsoft.AspNetCore.Builder;

using System.Net;
using Jykoserver.Protocols;

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

        var app = builder.Build();

        /*
        // chatmsg
        var chatMsg = new List<ChatMessage>();
        app.MapPost("/api/chat/send-message", (ChatMessage message) =>
        {
            chatMsg.Add(message); // 메시지 추가
            return Results.Ok(); // 200 OK 응답
        });

        // 채팅 메시지 불러오기 엔드포인트
        app.MapGet("/api/chat/get-messages", () =>
        {
            return Results.Ok(chatMsg); // 200 OK 응답과 함께 메시지 목록 반환
        });
        */

        // lifetime logging
        ConfigureLifetimeLogging(app.Lifetime);

        // app running
        app.Run(async context =>
        {
            /*
            // context에 적합한 protocol에 매칭
            Log.Logger.ForContext("Type", "SYS").Information(" [appRun] context.Request.Path.ToString(). ()[1..] " + context.Request.Path.ToString().ToLower()[1..]);
            Log.Logger.ForContext("Type", "SYS").Information(" [appRun] ConvertToReqType() " + RouteDispatcher.ConvertToReqType(context.Request.Path.ToString().ToLower()[1..]));
            // 
            if (dispatcher!.TryGetValue(RouteDispatcher.ConvertToReqType(context.Request.Path.ToString().ToLower()[1..])
                , out Func<IProtocol>? protocol))
                await protocol().InvokeAsync(context);
            else
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;     // 404 error
            */

            var path = context.Request.Path.ToString().ToLower()[1..];
            var protocolKey = RouteDispatcher.ConvertToReqType(path);
            Log.Logger.ForContext("Type", "SYS").Information(" [appRun] path " + path);
            Log.Logger.ForContext("Type", "SYS").Information(" [appRun] protocolKey " + protocolKey);


            // root endpoint         
            if (path == "")
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK; // 200 OK
                await context.Response.WriteAsync("Welcome to Jykoserver! Use /Ping or /Send endpoints.");
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

}

public class ChatMessage
{
    public string Sender { get; set; }
    public string Message { get; set; }
}







#region ASP.NET Core 기본 코드 백업
/*
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();

*/
#endregion

#region gpt code
/*
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // Controller 사용 등록

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers(); // API 엔드포인트 매핑

app.Run();


*/
#endregion
