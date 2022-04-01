using System.Net.Sockets;

namespace IOT.Connections.Server.Core;

internal sealed class Device : Connections.Device
{
    public Device(TcpClient client) : base(client)
    {
    }
}