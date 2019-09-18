using OpenCV.Net;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Audio
{
    static class BufferHelper
    {
        public static void UpdateBuffer(int bid, Mat buffer, int sampleRate)
        {
            var transpose = buffer.Rows < buffer.Cols;
            var channels = transpose ? buffer.Rows : buffer.Cols;
            if (channels > 2)
            {
                throw new InvalidOperationException("Unsupported number of channels for the specified output format.");
            }

            var format = channels == 2 ? ALFormat.Stereo16 : ALFormat.Mono16;
            var convertDepth = buffer.Depth != Depth.S16;
            if (convertDepth || transpose)
            {
                // Convert if needed
                if (convertDepth)
                {
                    var temp = new Mat(buffer.Rows, buffer.Cols, Depth.S16, 1);
                    CV.Convert(buffer, temp);
                    buffer = temp;
                }

                // Transpose multichannel to column order
                if (transpose)
                {
                    var temp = new Mat(buffer.Cols, buffer.Rows, buffer.Depth, 1);
                    CV.Transpose(buffer, temp);
                    buffer = temp;
                }
            }

            AL.BufferData(bid, format, buffer.Data, buffer.Rows * buffer.Step, sampleRate);
        }
    }
}
