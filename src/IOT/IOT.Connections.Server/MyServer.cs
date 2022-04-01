using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace IOT.Connections.Server;

public class MyServer : IDeviceListener
{
    private readonly ILogger<MyServer> _logger;
    private readonly IDeviceDataReceivedEventEmiter _handler;

    private readonly CancellationTokenSource _cancellationTokenSource;

    private readonly ConcurrentDictionary<string, IOT.Connections.Server.Core.Device> _devices;

    public MyServer(ILogger<MyServer> logger, IDeviceDataReceivedEventEmiter handler)
    {
        this._cancellationTokenSource = new CancellationTokenSource();
        this._devices = new ConcurrentDictionary<string, IOT.Connections.Server.Core.Device>();

        this._logger = logger;
        this._handler = handler;
    }

    public async Task RunAsync(IPAddress localaddr, int port, CancellationToken cancellationToken)
    {
        var token = CancellationTokenSource.CreateLinkedTokenSource(this._cancellationTokenSource.Token, cancellationToken).Token;

        var listener = new TcpListener(IPAddress.Loopback, 6789);

        listener.Start();

        // run every 1000 msec
        _ = Task
            .Run(async () =>
            {
                while(!token.IsCancellationRequested)
                {
                    DoCleanup(this);

                    await Task.Delay(1000, token);
                }
            })
            .ConfigureAwait(false);

        try
        {
            while (!token.IsCancellationRequested)
            {
                var tcpCLient = await listener.AcceptTcpClientAsync(token);

                var client = new IOT.Connections.Server.Core.Device(tcpCLient);

                this._devices.TryAdd(client.Id, client);
                this._handler.RegisterDevice(client);
            }
        }
        catch(OperationCanceledException)
        {
            this._logger.LogInformation("Server stopped listening");
        }
        catch(Exception)
        {
            throw;
        }
        finally
        {
            listener.Stop();
        }
    }

    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                this._cancellationTokenSource?.Cancel();

                while(this._devices.Any())
                {
                    var device = this._devices.FirstOrDefault().Value;

                    try
                    {
                        this.DisposeClient(device);
                    }
                    catch(Exception e)
                    {
                        this._logger.LogError(e, "foo");
                    }
                }

                this._handler.Dispose();

            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private static void DoCleanup(MyServer server)
    {
        foreach(var device in server._devices.Select(x => x.Value).Where(x => !x.Connected))
        {
            server._logger.LogInformation("Device {Device} disconnected", device.Id);

            server.DisposeClient(device);
        }
    }

    private void DisposeClient(Device client)
    {
        this._handler.UnregisterDevice(client);

        if(client is IOT.Connections.Server.Core.Device _client)
        {
            this._devices.TryRemove(_client.Id, out _);
        }

        client?.Dispose();
    }
}