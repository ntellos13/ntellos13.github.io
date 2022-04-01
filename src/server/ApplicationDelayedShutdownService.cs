public class ApplicationDelayedShutdownService : BackgroundService
{
    private readonly ILogger<MyService> _logger;

    private readonly IServiceProvider _services;

    public ApplicationDelayedShutdownService(ILogger<MyService> logger, IServiceProvider services)
    {
        this._logger = logger;
        this._services = services;
    }


    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{Worker} running at: {time}", nameof(ApplicationDelayedShutdownService), DateTimeOffset.Now);

        await Task.Delay(10 * 1000);

        var server = this._services.GetRequiredService<IOT.Connections.Server.MyServer>();

        server.Dispose();
    }
}