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
            Log.Logger.ForContext("Type", "SYS").Information("::::::::::::::::::Received request body: {0}", req);
            // response 생성
            var response = MemoryPackSerializer.Serialize(new Response
            {
                TypeNo = -1,
                Msg = req.Msg,
            });
            // TODO :: 모든 등록된 클라이언트에게 브로드캐스트

            return;
        }

        private async Task Broadcast(ClientInfo client, string message)
        {
            try
            {
                Log.Logger.ForContext("Type", "SYS").Information("[Send] Send message to client: {0}", client.guid);                using var clientResponse = await ClientManager.HttpClient.PostAsync(client.ClientUri, new StringContent(message));
                //using var clientResponse = await ClientManager.HttpClient.PostAsync(client.ClientUri, new StringContent(message));


            }
            catch (Exception ex)
            {
                Log.Logger.ForContext("Type", "SYS").Error("Failed to send message to client: {0}. Error: {1}", client.guid, ex.Message);

            }
        }
}
