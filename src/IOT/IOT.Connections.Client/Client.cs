using System.Net.Sockets;

namespace IOT.Connections.Client;

public sealed class ClientDevice : Device
{
    public ClientDevice() : base(new TcpClient())
    {
    }

    public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken)
    {
        await this.TcpClient.ConnectAsync(host, port, cancellationToken);
    }
}