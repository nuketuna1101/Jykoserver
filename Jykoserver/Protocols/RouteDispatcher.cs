using Serilog;
using System.Linq.Expressions;

namespace Jykoserver.Protocols
{
    public class RouteDispatcher
    {
        public static Dictionary<RequestType, Func<IProtocol>> MapDispatcher()
        {
            // assembly scan을 통한 dictionary 생성
            var dispatcher = new Dictionary<RequestType, Func<IProtocol>>();
            var curAssembly = AppDomain.CurrentDomain.GetAssemblies();
            var childTypes = curAssembly.Where(a => a.GetName().Name!.Equals("Jykoserver"))
                                        .SelectMany(s => s.GetTypes())
                                        .Where(p => typeof(IProtocol).IsAssignableFrom(p) && p.IsClass);
            foreach (var type in childTypes)
            {
                var protocol = Activator.CreateInstance(type) as IProtocol;
                dispatcher.Add(protocol.reqType, Expression.Lambda<Func<IProtocol>>(Expression.New(type)).Compile());
                // 임시로 딕셔너리 확인 용
                Log.Logger.ForContext("Type", "SYS").Information("protocol.reqType: {0} / iprotocol : {1}",
                    protocol.reqType, Expression.Lambda<Func<IProtocol>>(Expression.New(type)).Compile());
            }
            return dispatcher;
        }
        public static RequestType ConvertToReqType(string str)
        {
            // converter 기능
            switch (str)
            {
                case "ping":
                    return RequestType.PING;
                case "send":
                    return RequestType.SEND;
                case "chat":
                    return RequestType.CHAT;
                case "websockethandler":
                    return RequestType.WSH;
                default:
                    return RequestType.PING;
            }
        }
    }
}
