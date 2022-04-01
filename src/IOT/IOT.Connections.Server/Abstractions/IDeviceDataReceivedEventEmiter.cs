namespace IOT.Connections.Server;

public interface IDeviceDataReceivedEventEmiter : IDisposable
{
    void RegisterDevice(Device device);

    void UnregisterDevice(Device device);
}