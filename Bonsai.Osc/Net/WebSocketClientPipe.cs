using System;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Osc.Net
{
    static class WebSocketClientPipe
    {
        public static async Task GenerateAsync(ClientWebSocket client, int minimumBufferSize, PipeWriter writer, CancellationToken cancellationToken = default)
        {
            while (true)
            {
                var memory = writer.GetMemory(minimumBufferSize);
                if (MemoryMarshal.TryGetArray<byte>(memory, out var array))
                {
                    try
                    {
                        var result = await client.ReceiveAsync(array, cancellationToken);
                        if (result.Count == 0)
                            break;

                        writer.Advance(result.Count);
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

        public static async Task TransformAsync(PipeReader reader, ClientWebSocket client, WebSocketMessageType messageType, CancellationToken cancellationToken = default)
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
                        var enumerator = buffer.GetEnumerator();
                        if (enumerator.MoveNext())
                        {
                            var previous = enumerator.Current;

                            while (enumerator.MoveNext())
                            {
                                if (MemoryMarshal.TryGetArray(previous, out var arraySegment))
                                {
                                    await client.SendAsync(arraySegment, messageType, false, cancellationToken);
                                }

                                previous = enumerator.Current;
                            }

                            if (MemoryMarshal.TryGetArray(previous, out var lastArraySegment))
                            {
                                await client.SendAsync(lastArraySegment, messageType, true, cancellationToken);
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
