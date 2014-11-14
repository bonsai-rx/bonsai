using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    static class ArrHelper
    {
        public static Mat EnsureFormat(Mat output, Size size, Depth depth, int channels)
        {
            if (output == null || output.Size != size || output.Depth != depth || output.Channels != channels)
            {
                if (output != null) output.Close();
                return new Mat(size, depth, channels);
            }

            return output;
        }

        public static IplImage EnsureFormat(IplImage output, Size size, IplDepth depth, int channels)
        {
            if (output == null || output.Size != size || output.Depth != depth || output.Channels != channels)
            {
                if (output != null) output.Close();
                return new IplImage(size, depth, channels);
            }

            return output;
        }

        public static IplImage EnsureColorCopy(IplImage output, IplImage image)
        {
            output = EnsureFormat(output, image.Size, image.Depth, 3);
            if (image.Channels == 1) CV.CvtColor(image, output, ColorConversion.Gray2Bgr);
            else CV.Copy(image, output);
            return output;
        }

        public static Depth FromIplDepth(IplDepth depth)
        {
            const int ConversionOffset = (int)
                (Depth.U8 +
                ((int)Depth.U16 << 4) +
                ((int)Depth.F32 << 8) +
                ((int)Depth.F64 << 16) +
                ((int)Depth.S8 << 20) +
                ((int)Depth.S16 << 24) +
                ((int)Depth.S32 << 28));
            var depthShift = (((int)depth & 0xF0) >> 2) + (unchecked((int)depth & 0x80000000) != 0 ? 20 : 0);
            return (Depth)((ConversionOffset >> depthShift) & 15);
        }

        public static IplDepth FromDepth(Depth depth)
        {
            switch (depth)
            {
                case Depth.U8: return IplDepth.U8;
                case Depth.S8: return IplDepth.S8;
                case Depth.U16: return IplDepth.U16;
                case Depth.S16: return IplDepth.S16;
                case Depth.S32: return IplDepth.S32;
                case Depth.F32: return IplDepth.F32;
                case Depth.F64: return IplDepth.F64;
                case Depth.UserType:
                default:
                    throw new ArgumentException("Invalid depth type.", "depth");
            }
        }

        public static byte[] ToArray(Mat input, MatrixLayout layout = MatrixLayout.RowMajor)
        {
            var step = input.ElementSize * input.Cols;
            var data = new byte[step * input.Rows];
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                Mat dataHeader;
                switch (layout)
                {
                    case MatrixLayout.ColumnMajor:
                        dataHeader = new Mat(input.Cols, input.Rows, input.Depth, input.Channels, dataHandle.AddrOfPinnedObject(), input.ElementSize * input.Rows);
                        CV.Transpose(input, dataHeader);
                        break;
                    default:
                    case MatrixLayout.RowMajor:
                        dataHeader = new Mat(input.Rows, input.Cols, input.Depth, input.Channels, dataHandle.AddrOfPinnedObject(), step);
                        CV.Copy(input, dataHeader);
                        break;
                }
            }
            finally { dataHandle.Free(); }
            return data;
        }
    }
}
