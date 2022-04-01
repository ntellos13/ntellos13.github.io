namespace IOT.Connections.Server;

public interface IDeviceDataReceivedHandler
{
    Task HandleAsync(Device device, byte[] buffer, int offset, int count, CancellationToken cancellationToken);
}