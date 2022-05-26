using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Osc.Net
{
    static class ObservablePipe
    {
        public static async Task TransformAsync(PipeReader reader, IObserver<byte[]> observer, CancellationToken cancellationToken = default)
        {
            Exception error = null;
            try
            {
                while (true)
                {
                    var readResult = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                    if (readResult.IsCanceled || readResult.IsCompleted)
                        break;

                    var buffer = readResult.Buffer;
                    try
                    {
                        foreach (var memory in buffer)
                        {
                            if (MemoryMarshal.TryGetArray(memory, out var arraySegment))
                            {
                                observer.OnNext(arraySegment.ToArray());
                            }
                        }
                    }
                    finally
                    {
                        reader.AdvanceTo(buffer.End);
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
                await reader.CompleteAsync(error).ConfigureAwait(false);
            }
        }
    }
}
