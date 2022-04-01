using IOT.Connections.Server;

var host = Host.CreateDefaultBuilder(args)
    .UseSystemd()
    .ConfigureServices(services =>
    {
        services.AddTransient<IDeviceDataReceivedHandler, DeviceDataReceivedHandler>();
        services.AddTransient<DeviceDataReceivedEventEmiter>();
        services.AddScoped<IDeviceDataReceivedEventEmiter>(x =>
        {
            var dataReceivedHandler = x.GetRequiredService<IDeviceDataReceivedHandler>();
            var logger = x.GetRequiredService<ILogger<DeviceDataReceivedEventEmiter>>();

            var result = new DeviceDataReceivedEventEmiter(logger, dataReceivedHandler);

            return result;
        });
        services.AddScoped<MyServer>();

        services.AddHostedService<MyService>();
        // services.AddHostedService<ApplicationDelayedShutdownService>();
    })
    .Build();

await host.RunAsync();