using System.Text;

namespace IOT.Connections.Server;

public sealed class DeviceDataReceivedHandler : IDeviceDataReceivedHandler
{
    public async Task HandleAsync(Device device, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var text = Encoding.UTF8.GetString(buffer, offset, count);

        var bytes = Encoding.UTF8.GetBytes($"Hello {device.Id}{Environment.NewLine}");

        await device.SendAsync(bytes, CancellationToken.None);
    }
}