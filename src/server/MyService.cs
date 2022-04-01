using System.Net;
using System.Diagnostics;

using IOT.Connections.Server;

public class MyService : BackgroundService
{
    private readonly ILogger<MyService> _logger;
    private readonly MyServer _server;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public MyService(ILogger<MyService> logger, IHostApplicationLifetime applicationLifetime, MyServer server)
    {
        this._logger = logger;
        this._server = server;
        this._applicationLifetime = applicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{Worker}(PID: {PID}) running at: {time}", nameof(MyService), Process.GetCurrentProcess().Id, DateTimeOffset.Now);

        await this._server.RunAsync(IPAddress.Loopback, 6789, cancellationToken);
    }
}

//kill -s SIGINT