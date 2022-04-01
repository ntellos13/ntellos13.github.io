namespace IOT.Connections.Client;

public static class DeviceExtensions
{
    public static Task ListenAsync(this Device device, Func<(byte[] Buffer, int Offset, int Count), CancellationToken, Task> asyncCallback, CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            try
            {
                var readBuffer = new byte[1024];

                while(device.Connected)
                {
                    Array.Clear(readBuffer, 0, readBuffer.Length); // maybe this is redudant

                    var count = await device.ReadAsync(readBuffer, cancellationToken);

                    if(count <= 0)
                    {
                        continue;
                    }
                    
                    // discard => fire & forget
                    _ = asyncCallback.Invoke((readBuffer, 0, count), cancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
            }
        });
    }
}