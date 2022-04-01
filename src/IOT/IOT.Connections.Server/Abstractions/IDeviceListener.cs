using System.Net;

namespace IOT.Connections.Server;

public interface IDeviceListener : IDisposable
{
    Task RunAsync(IPAddress localaddr, int port, CancellationToken cancellationToken);
}