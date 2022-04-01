using System.Collections.Concurrent;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace IOT.Connections.Server;

public sealed class DeviceDataReceivedEventEmiter : IDeviceDataReceivedEventEmiter
{
    private readonly ILogger<IDeviceDataReceivedEventEmiter> _logger;
    private readonly IDeviceDataReceivedHandler _dataReceivedHandler;

    private readonly ConcurrentDictionary<Device, DeviceRegistration> _devices;
    
    public DeviceDataReceivedEventEmiter(ILogger<IDeviceDataReceivedEventEmiter> logger, IDeviceDataReceivedHandler dataReceivedHandler)
    {
        this._devices = new ConcurrentDictionary<Device, DeviceRegistration>();

        this._logger = logger;
        this._dataReceivedHandler = dataReceivedHandler;
    }

    public void RegisterDevice(Device device)
    {
        var tokenSource = new CancellationTokenSource();

        this._devices.TryAdd(device, new DeviceRegistration
        {
            Device = device,
            ExecutionTask = Task.Run(async () =>
            {
                try
                {
                    var readBuffer = new byte[1024];

                    while(device.Connected)
                    {
                        Array.Clear(readBuffer, 0, readBuffer.Length); // maybe this is redudant

                        var count = await device.ReadAsync(readBuffer, tokenSource.Token);

                        if(count <= 0)
                        {
                            continue;
                        }
                        
                        // discard => fire & forget
                        _ = this._dataReceivedHandler.HandleAsync(device, readBuffer, 0, count, tokenSource.Token);
                    }

                    // throw new InvalidCastException();
                }
                catch (TaskCanceledException)
                {
                    this._logger.LogInformation("TaskCanceledException");
                }
                catch (Exception e)
                {
                    var ex = e.TraverseFind<SocketException>();

                    if(ex is not null && ex.SocketErrorCode == SocketError.OperationAborted)
                    {
                        return;
                    }

                    this._logger.LogError(e, "An error occured");
                }
            })
        });
    }

    public void UnregisterDevice(Device device)
    {
        this._devices.TryRemove(device, out var registration);

        if(registration == null)
        {
            return;
        }

        registration.cancellationTokenSource?.Cancel();
        
        if(registration.ExecutionTask is null)
        {
            return;
        }

        if(registration.ExecutionTask.IsCompleted)
        {
            registration.ExecutionTask.Dispose();
        }

        return;
    }

    private bool disposedValue;

    public void Dispose()
    {
        if (!disposedValue)
        {
            while(this._devices.Any())
            {
                UnregisterDevice(_devices.FirstOrDefault().Key);
            }

            disposedValue = true;
        }

        GC.SuppressFinalize(this);
    }

    class DeviceRegistration
    {
        public Device Device {  get; init; } = default!;

        public CancellationTokenSource cancellationTokenSource {  get; init; } = default!;

        public Task ExecutionTask {  get; init; } = default!;
    }
}