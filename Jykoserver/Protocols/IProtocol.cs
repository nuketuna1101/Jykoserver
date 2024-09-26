namespace Jykoserver.Protocols
{
    public interface IProtocol
    {
        public RequestType reqType { get; }
        public Task InvokeAsync(HttpContext httpContext);
    }
}
