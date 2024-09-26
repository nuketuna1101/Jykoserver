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
            //IDatabase? _db = httpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>().GetDatabase();

            // request 패킷엔 : guid와 dto 담겨서 옴.
            Request? req = await MemoryPackSerializer.DeserializeAsync<Request>(httpContext.Request.Body);
            var myGUID = req.UserGUID;
            var myMsg = MemoryPackSerializer.Deserialize<string>(req.Msg);

            using (var _db = httpContext.RequestServices.GetRequiredService<MyDbContext>())
            {
                // guid에 해당하는 유저를 db 유저 게임 데이터에서 찾기
                var targetUser = await _db.USER_GAMEDATA.FirstAsync(user => user.UserGuid == myGUID);
                // 잘못된 유저 정보일 flow 처리
                if (targetUser == null)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;   // 404 error
                    return;
                }

                // 해당 유저에 관해 dto로 넘어온 데이터 세이브

                // 복붙하고 세이브
                await _db.SaveChangesAsync();
            }
            return;
        }



    }
}
