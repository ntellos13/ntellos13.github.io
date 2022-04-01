using System.Net;
using System.Text;

using IOT.Connections.Client;

Console.WriteLine("Hello, World!");

var data = Encoding.UTF8.GetBytes($"Hello World!{Environment.NewLine}");

var defaultTokenSource = new CancellationTokenSource();

var tasks = Enumerable.Range(0, 1000)
    .Select(x => Task.Run(async () =>
    {
        while(true)
        {
            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(defaultTokenSource.Token);
            var cancellationToken = tokenSource.Token;

            try
            {
                var client = new ClientDevice();

                await client.ConnectAsync(IPAddress.Loopback.ToString(), 6789, cancellationToken);

                _ = client.ListenAsync(async (data, cancellationToken) =>
                {
                    await Task.CompletedTask;

                    var text = Encoding.UTF8.GetString(data.Buffer, data.Offset, data.Count);

                    Console.Write($"{DateTime.UtcNow}: {text}");

                }, cancellationToken);

                while(!cancellationToken.IsCancellationRequested)
                {
                    await client.SendAsync(data, cancellationToken);

                    await Task.Delay(1 * 1000);
                }
            }
            catch(Exception)
            {

            }
            finally
            {
                tokenSource?.Cancel();
                tokenSource = null;
            }

            await Task.Delay(1 * 1000);
        };
    }))
    .ToList();

await Task.WhenAll(tasks);