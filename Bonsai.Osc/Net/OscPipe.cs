using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Osc.Net
{
    static class OscPipe
    {
        public static async Task TransformAsync(PipeReader reader, IObserver<Message> observer, CancellationToken cancellationToken = default)
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
                        while (TryParseOscMessage(ref buffer, out var message))
                        {
                            observer.OnNext(message);
                        }

                        // There's no more data to be processed.
                        if (readResult.IsCompleted)
                        {
                            if (buffer.Length > 0)
                            {
                                // The message is incomplete and there's no more data to process.
                                throw new InvalidDataException("Incomplete data packet.");
                            }
                            break;
                        }
                    }
                    finally
                    {
                        // Since all messages in the buffer are being processed, you can use the 
                        // remaining buffer's Start and End position to determine consumed and examined.
                        reader.AdvanceTo(buffer.Start, buffer.End);
                    }
                }

                observer.OnCompleted();
            }
            catch (Exception ex)
            {
                error = ex;
                observer.OnError(ex);
            }
            finally
            {
                await reader.CompleteAsync(error);
            }
        }

        static bool TryParseOscMessage(ref ReadOnlySequence<byte> buffer, out Message message)
        {
            // get the message size
            if (buffer.Length < sizeof(int))
            {
                message = default;
                return false;
            }
            var slice = buffer.Slice(0, sizeof(int));
            var packetSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(slice.ToArray(), 0));

            // get the message raw data
            if (buffer.Length < sizeof(int) + packetSize)
            {
                message = default;
                return false;
            }
            slice = buffer.Slice(sizeof(int), packetSize);
            message = new Message(slice.ToArray());

            buffer = buffer.Slice(slice.End, buffer.End); // trim buffer
            return true;
        }
    }
}
