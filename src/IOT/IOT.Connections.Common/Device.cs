using System.Net.Sockets;

namespace IOT.Connections;

public abstract class Device : IDisposable
{
    public virtual string Id { get; private set; } = Guid.NewGuid().ToString();

    private bool disposedValue;

    protected readonly TcpClient TcpClient;

    private static readonly byte[] buff = new byte[1];

    public bool Connected
    {
        get
        {
            try
            {
                if (!(this?.TcpClient?.Client?.Connected ?? false))
                {
                    return false;
                }

                /* pear to the documentation on Poll:
                 * When passing SelectMode.SelectRead as a parameter to the Poll method it will return 
                 * -either- true if Socket.Listen(Int32) has been called and a connection is pending;
                 * -or- true if data is available for reading; 
                 * -or- true if the connection has been closed, reset, or terminated; 
                 * otherwise, returns false
                 */

                // Detect if client disconnected
                if (this.TcpClient.Client.Poll(0, SelectMode.SelectRead)
                    && this.TcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }

    protected Device(TcpClient client)
    {
        this.TcpClient = client;
    }

    public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        var stream = this.TcpClient.GetStream();

        if(!this.TcpClient.Connected || !stream.CanRead)
        {
            throw new InvalidOperationException($"{nameof(ReadAsync)} method is not supported");
        }

        return await stream.ReadAsync(buffer, cancellationToken);
    }

    public async Task SendAsync(byte[] data, CancellationToken cancellationToken)
    {
        await this.SendAsync(data, 0, data.Length, cancellationToken);
    }

    private async Task SendAsync(byte[] data, int offset, int count, CancellationToken cancellationToken)
    {
        var stream = this.TcpClient.GetStream();

        await stream.WriteAsync(data, offset, count, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                try
                {
                    this.TcpClient?.Dispose();
                }
                catch(Exception)
                {

                }
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}