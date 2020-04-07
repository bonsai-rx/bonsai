using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Osc.Net
{
    static class TcpClientPipe
    {
        public static async Task GenerateAsync(NetworkStream stream, int minimumBufferSize, PipeWriter writer, CancellationToken cancellationToken = default)
        {
            while (true)
            {
                var memory = writer.GetMemory(minimumBufferSize);
                if (MemoryMarshal.TryGetArray<byte>(memory, out var array))
                {
                    try
                    {
                        var bytesRead = await stream.ReadAsync(array.Array, array.Offset, array.Count, cancellationToken);
                        if (bytesRead == 0)
                            break;

                        writer.Advance(bytesRead);
                    }
                    catch 
                    {
                        break;
                    }
                }

                var flushResult = await writer.FlushAsync(cancellationToken);
                if (flushResult.IsCanceled || flushResult.IsCompleted)
                    break;
            }

            await writer.CompleteAsync();
        }

        public static async Task Transform(PipeReader reader, NetworkStream stream, CancellationToken cancellationToken = default)
        {
            Exception error = null;
            try
            {
                while (true)
                {
                    var readResult = await reader.ReadAsync(cancellationToken);
                    if (readResult.IsCanceled || readResult.IsCompleted)
                        break;

                    var buffer = readResult.Buffer;
                    try
                    {
                        foreach (var memory in buffer)
                        {
                            if (MemoryMarshal.TryGetArray(memory, out var arraySegment))
                            {
                                await stream.WriteAsync(arraySegment.Array, arraySegment.Offset, arraySegment.Count, cancellationToken);
                            }
                        }
                    }
                    finally
                    {
                        // Since all messages in the buffer are being processed, you can use the 
                        // remaining buffer's Start and End position to determine consumed and examined.
                        reader.AdvanceTo(buffer.Start, buffer.End);
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                await reader.CompleteAsync(error);
            }
        }
    }
}
