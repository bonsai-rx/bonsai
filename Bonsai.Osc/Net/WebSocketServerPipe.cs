using System;
using System.Buffers;
using System.Globalization;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

// based on:
// https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API/Writing_WebSocket_server
// https://stackoverflow.com/a/54201295

namespace Bonsai.Osc.Net
{
    static class WebSocketServerPipe
    {
        public static async Task TransformAsync(PipeReader reader, NetworkStream stream, PipeWriter writer, CancellationToken cancellationToken = default)
        {
            Exception error = null;
            try
            {
                // perform handshake
                while (true)
                {
                    var readResult = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                    var buffer = readResult.Buffer;

                    try
                    {
                        if (TryParseWebSocketHandshake(ref buffer, out var response))
                        {
                            await stream.WriteAsync(response, 0, response.Length, cancellationToken).ConfigureAwait(false);
                            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                            break;
                        }
                    }
                    finally
                    {
                        reader.AdvanceTo(buffer.Start, buffer.End);
                    }
                }

                // process packets
                while (true)
                {
                    var readResult = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                    var buffer = readResult.Buffer;

                    try
                    {
                        while (TryParseWebSocketPacket(ref buffer, writer))
                        {
                            var flushResult = await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                            if (flushResult.IsCanceled || flushResult.IsCompleted)
                                break;
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
            }
            catch (Exception ex)
            {
                error = ex;
                throw;
            }
            finally
            {
                await reader.CompleteAsync(error).ConfigureAwait(false);
                await writer.CompleteAsync(error).ConfigureAwait(false);
            }
        }

        static bool TryParseWebSocketHandshake(ref ReadOnlySequence<byte> buffer, out byte[] response)
        {
            var s = Encoding.UTF8.GetString(buffer.ToArray());
            var end = s.IndexOf("\r\n\r\n");
            if (end >= 0 && s.StartsWith("get", true, CultureInfo.InvariantCulture))
            {
                // 1. Obtain the value of the "Sec-WebSocket-Key" request header without any leading or trailing whitespace
                // 2. Concatenate it with "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" (a special GUID specified by RFC 6455)
                // 3. Compute SHA-1 and Base64 hash of the new value
                // 4. Write the hash back as the value of "Sec-WebSocket-Accept" response header in an HTTP response
                var swk = Regex.Match(s, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
                var swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                var swkaSha1 = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
                var swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

                // HTTP/1.1 defines the sequence CR LF as the end-of-line marker
                var responseString = new StringBuilder()
                    .Append("HTTP/1.1 101 Switching Protocols\r\n")
                    .Append("Connection: Upgrade\r\n")
                    .Append("Upgrade: websocket\r\n")
                    .Append($"Sec-WebSocket-Accept: {swkaSha1Base64}\r\n\r\n")
                    .ToString();
                response = Encoding.UTF8.GetBytes(responseString);

                buffer = buffer.Slice(end + 4, buffer.End); // trim buffer
                return true;
            }

            response = default;
            return false;
        }

        static bool TryParseWebSocketPacket(ref ReadOnlySequence<byte> buffer, PipeWriter writer)
        {
            if (buffer.Length >= 6)
            {
                var bytes = buffer.ToArray();
                try
                {
                    // decode data
                    var opcode = (Opcode)(bytes[0] & 0b00001111); 

                    var fin = (bytes[0] & 0b10000000) != 0;
                    var mask = (bytes[1] & 0b10000000) != 0;
                    if (!mask)
                        throw new InvalidDataException("Messages from client must be masked.");

                    var offset = 2;
                    var msglen = bytes[1] - 128; // & 0111 1111

                    switch (msglen)
                    {
                        case 126:
                            offset = 4;
                            msglen = BitConverter.ToUInt16(new byte[] { bytes[3], bytes[2] }, 0);
                            break;
                        case 127:
                            offset = 10;
                            msglen = (int)BitConverter.ToInt64(new byte[] { bytes[9], bytes[8], bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2] }, 0);
                            break;
                    }

                    if (opcode == Opcode.Ping) 
                    {
                        // TODO
                    }
                    else if (opcode == Opcode.Pong) 
                    {
                        // do nothing
                    }
                    else if (msglen > 0)
                    {
                        var masks = new byte[] { bytes[offset], bytes[offset + 1], bytes[offset + 2], bytes[offset + 3] };
                        offset += 4;

                        var memory = writer.GetMemory(msglen);
                        if (MemoryMarshal.TryGetArray<byte>(memory, out var arraySegment))
                        {
                            for (var i = 0; i < msglen; i++)
                                arraySegment.Array[arraySegment.Offset + i] = (byte)(bytes[offset + i] ^ masks[i % 4]);

                            writer.Advance(msglen);
                        }
                    }

                    buffer = buffer.Slice(offset + msglen, buffer.End); // trim buffer
                    return true;
                }
                catch
                {
                    // do nothing
                }
            }

            return false;
        }

        public enum Opcode
        {
            /* Denotes a continuation code */
            Fragment = 0,

            /* Denotes a text code */
            Text = 1,

            /* Denotes a binary code */
            Binary = 2,

            /* Denotes a closed connection */
            ClosedConnection = 8,

            /* Denotes a ping*/
            Ping = 9,

            /* Denotes a pong */
            Pong = 10
        }

        /// <summary>Gets an encoded websocket frame to send to a client</summary>
        /// <param name="Message">The message to encode into the frame</param>
        /// <param name="Opcode">The opcode of the frame</param>
        /// <returns>Byte array in form of a websocket frame</returns>
        public static byte[] Encode(ArraySegment<byte> bytesRaw, Opcode opcode)
        {
            var length = bytesRaw.Count;
            var frame = new byte[10];
            int indexStartRawData;

            frame[0] = (byte)(128 + (int)opcode);
            if (length <= 125)
            {
                frame[1] = (byte)length;

                indexStartRawData = 2;
            }
            else if (length >= 126 && length <= 65535)
            {
                frame[1] = (byte)126;
                frame[2] = (byte)((length >> 8) & 255);
                frame[3] = (byte)(length & 255);

                indexStartRawData = 4;
            }
            else
            {
                frame[1] = (byte)127;
                frame[2] = (byte)((length >> 56) & 255);
                frame[3] = (byte)((length >> 48) & 255);
                frame[4] = (byte)((length >> 40) & 255);
                frame[5] = (byte)((length >> 32) & 255);
                frame[6] = (byte)((length >> 24) & 255);
                frame[7] = (byte)((length >> 16) & 255);
                frame[8] = (byte)((length >> 8) & 255);
                frame[9] = (byte)(length & 255);

                indexStartRawData = 10;
            }

            var response = new byte[indexStartRawData + length];
            Array.Copy(frame, 0, response, 0, indexStartRawData);
            Array.Copy(bytesRaw.Array, bytesRaw.Offset, response, indexStartRawData, length);
            return response;
        }
    }
}
